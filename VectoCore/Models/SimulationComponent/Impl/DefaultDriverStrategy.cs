using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using NLog.Fluent;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class DefaultDriverStrategy : LoggingObject, IDriverStrategy
	{
		protected DrivingBehaviorEntry NextDrivingAction;

		public enum DrivingMode
		{
			DrivingModeDrive,
			DrivingModeBrake,
		}

		protected DrivingMode CurrentDrivingMode;
		protected Dictionary<DrivingMode, IDriverMode> DrivingModes = new Dictionary<DrivingMode, IDriverMode>();

		public DefaultDriverStrategy()
		{
			DrivingModes.Add(DrivingMode.DrivingModeDrive, new DriverModeDrive() { DriverStrategy = this });
			DrivingModes.Add(DrivingMode.DrivingModeBrake, new DriverModeBrake() { DriverStrategy = this });
			CurrentDrivingMode = DrivingMode.DrivingModeDrive;
		}

		public IDriverActions Driver { get; set; }

		protected internal DrivingBehaviorEntry BrakeTrigger { get; set; }


		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			if (CurrentDrivingMode == DrivingMode.DrivingModeBrake) {
				if (Driver.DataBus.Distance.IsGreaterOrEqual(BrakeTrigger.TriggerDistance)) {
					CurrentDrivingMode = DrivingMode.DrivingModeDrive;
					NextDrivingAction = null;
					DrivingModes[CurrentDrivingMode].ResetMode();
					Log.Debug("Switching to DrivingMode DRIVE");
				}
			}
			if (CurrentDrivingMode == DrivingMode.DrivingModeDrive) {
				var currentDistance = Driver.DataBus.Distance;
				UpdateDrivingAction(currentDistance);
				if (NextDrivingAction != null) {
					if (currentDistance.IsEqual(NextDrivingAction.ActionDistance,
						Constants.SimulationSettings.DriverActionDistanceTolerance)) {
						CurrentDrivingMode = DrivingMode.DrivingModeBrake;
						DrivingModes[CurrentDrivingMode].ResetMode();
						Log.Debug("Switching to DrivingMode BRAKE");

						BrakeTrigger = NextDrivingAction;
						//break;
					} else if ((currentDistance + ds).IsGreater(NextDrivingAction.ActionDistance)) {
						Log.Debug("Current simulation interval exceeds next action distance at {0}. reducing maxDistance to {1}",
							NextDrivingAction.ActionDistance, NextDrivingAction.ActionDistance - currentDistance);
						return new ResponseDrivingCycleDistanceExceeded() {
							Source = this,
							MaxDistance = NextDrivingAction.ActionDistance - currentDistance
						};
					}
				}
			}

			var retVal = DrivingModes[CurrentDrivingMode].Request(absTime, ds, targetVelocity, gradient);

			if (NextDrivingAction == null || !(retVal is ResponseSuccess)) {
				return retVal;
			}

			// if we accelerate in the current simulation interval the ActionDistance of the next action
			// changes and we might pass the ActionDistance - check again...
			if (retVal.Acceleration <= 0) {
				return retVal;
			}

			// if the speed at the end of the simulation interval is below the next target speed 
			// we are fine (no need to brake right now)
			var v2 = Driver.DataBus.VehicleSpeed + retVal.Acceleration * retVal.SimulationInterval;
			if (v2 <= NextDrivingAction.NextTargetSpeed) {
				return retVal;
			}


			Meter newds;
			switch (NextDrivingAction.Action) {
				case DrivingBehavior.Coasting:
					var coastingDistance = Formulas.DecelerationDistance(v2, NextDrivingAction.NextTargetSpeed,
						Driver.DriverData.LookAheadCoasting.Deceleration);

					// if the distance at the end of the simulation interval is smaller than the new ActionDistance
					// we are safe - go ahead...
					if ((Driver.DataBus.Distance + ds).IsSmallerOrEqual(NextDrivingAction.TriggerDistance - coastingDistance,
						Constants.SimulationSettings.DriverActionDistanceTolerance)) {
						return retVal;
					}
					newds = EstimateAccelerationDistanceBeforeBrake(retVal, NextDrivingAction);
					break;
				case DrivingBehavior.Braking:
					var brakingDistance = Driver.DriverData.AccelerationCurve.ComputeAccelerationDistance(v2,
						NextDrivingAction.NextTargetSpeed);
					if ((Driver.DataBus.Distance + ds).IsSmaller(NextDrivingAction.TriggerDistance - brakingDistance)) {
						return retVal;
					}
					newds = (NextDrivingAction.TriggerDistance - brakingDistance) - Driver.DataBus.Distance;
					break;
				default:
					return retVal;
			}
			if (newds.IsEqual(0, 1e-3) || ds.IsEqual(newds, 1e-3.SI<Meter>())) {
				return retVal;
			}
			Log.Debug("Exceeding next ActionDistance at {0}. Reducing max Distance to {1}", NextDrivingAction.ActionDistance,
				newds);
			return new ResponseDrivingCycleDistanceExceeded() {
				Source = this,
				MaxDistance = newds,
			};
		}

		private void UpdateDrivingAction(Meter currentDistance)
		{
			var nextAction = GetNextDrivingAction(currentDistance);
			if (NextDrivingAction == null) {
				if (nextAction != null) {
					// take the new action
					NextDrivingAction = nextAction;
				}
			} else {
				// update action distance for current 'next action'
				if (Driver.DataBus.VehicleSpeed > NextDrivingAction.NextTargetSpeed) {
					switch (NextDrivingAction.Action) {
						case DrivingBehavior.Coasting:
							var coastingDistance = Formulas.DecelerationDistance(Driver.DataBus.VehicleSpeed,
								NextDrivingAction.NextTargetSpeed,
								Driver.DriverData.LookAheadCoasting.Deceleration);
							NextDrivingAction.ActionDistance = NextDrivingAction.TriggerDistance - coastingDistance;
							break;
						case DrivingBehavior.Braking:
							var brakingDistance = Driver.ComputeDecelerationDistance(NextDrivingAction.NextTargetSpeed);
							NextDrivingAction.ActionDistance = NextDrivingAction.TriggerDistance - brakingDistance;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				if (nextAction != null) {
					if (nextAction.HasEqualTrigger(NextDrivingAction)) {
						// if the action changes and the vehicle has not yet exceeded the action distance => update the action
						// otherwise do nothing, NextDrivingAction's action distance has already been updated
						if (nextAction.Action != NextDrivingAction.Action && nextAction.ActionDistance > currentDistance) {
							NextDrivingAction = nextAction;
						}
					} else {
						// hmm, we've got a new action that is closer to what we got before?
						if (nextAction.ActionDistance < NextDrivingAction.ActionDistance) {
							NextDrivingAction = nextAction;
						}
					}
				} else {
					NextDrivingAction = null;
				}
			}
			Log.Debug("Next Driving Action: {0}", NextDrivingAction);
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			DriverBehavior = DrivingBehavior.Halted;
			return Driver.DrivingActionHalt(absTime, dt, targetVelocity, gradient);
		}


		public DrivingBehavior DriverBehavior { get; internal set; }

		private Meter EstimateAccelerationDistanceBeforeBrake(IResponse retVal, DrivingBehaviorEntry nextAction)
		{
			// estimate the distance to drive when accelerating with the current acceleration (taken from retVal) and then 
			// coasting with the dirver's LookaheadDeceleration. 
			// TriggerDistance - CurrentDistance = s_accelerate + s_lookahead
			// s_coast(dt) = - (currentSpeed + a * dt + nextTargetSpeed) * (nextTargetSpeed - (currentSpeed + a * dt))  / (2 * a_lookahead)
			// s_acc(dt) = currentSpeed * dt + a / 2 * dt^2
			// => solve for dt, compute ds = currentSpeed * dt + a / 2 * dt^2
			var dtList = VectoMath.QuadraticEquationSolver(
				(retVal.Acceleration / 2 -
				retVal.Acceleration * retVal.Acceleration / 2 / Driver.DriverData.LookAheadCoasting.Deceleration).Value(),
				(Driver.DataBus.VehicleSpeed -
				Driver.DataBus.VehicleSpeed * retVal.Acceleration / Driver.DriverData.LookAheadCoasting.Deceleration).Value(),
				(nextAction.NextTargetSpeed * nextAction.NextTargetSpeed / 2 / Driver.DriverData.LookAheadCoasting.Deceleration -
				Driver.DataBus.VehicleSpeed * Driver.DataBus.VehicleSpeed / 2 / Driver.DriverData.LookAheadCoasting.Deceleration -
				(nextAction.TriggerDistance - Driver.DataBus.Distance)).Value());
			dtList.Sort();
			var dt = dtList.First(x => x > 0).SI<Second>();
			var newds = Driver.DataBus.VehicleSpeed * dt + retVal.Acceleration / 2 * dt * dt;
			return newds;
		}

		protected DrivingBehaviorEntry GetNextDrivingAction(Meter minDistance)
		{
			var currentSpeed = Driver.DataBus.VehicleSpeed;

			// distance until halt
			var lookaheadDistance = Formulas.DecelerationDistance(currentSpeed, 0.SI<MeterPerSecond>(),
				Driver.DriverData.LookAheadCoasting.Deceleration);

			var lookaheadData = Driver.DataBus.LookAhead(1.2 * lookaheadDistance);

			Log.Debug("Lookahead distance: {0} @ current speed {1}", lookaheadDistance, currentSpeed);
			var nextActions = new List<DrivingBehaviorEntry>();
			foreach (var entry in lookaheadData) {
				var nextTargetSpeed = OverspeedAllowed(entry.RoadGradient, entry.VehicleTargetSpeed)
					? entry.VehicleTargetSpeed + Driver.DriverData.OverSpeedEcoRoll.OverSpeed
					: entry.VehicleTargetSpeed;
				if (nextTargetSpeed < currentSpeed) {
					// TODO @@@quam  currentSpeed ? targetSpeed? nextTargetSpeed?
					if (!Driver.DriverData.LookAheadCoasting.Enabled ||
						currentSpeed < Driver.DriverData.LookAheadCoasting.MinSpeed) {
						var brakingDistance = Driver.ComputeDecelerationDistance(nextTargetSpeed);
						Log.Debug("adding 'Braking' starting at distance {0}. brakingDistance: {1}, triggerDistance: {2}",
							entry.Distance - brakingDistance, brakingDistance, entry.Distance);
						nextActions.Add(new DrivingBehaviorEntry {
							Action = DrivingBehavior.Braking,
							ActionDistance = entry.Distance - brakingDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = nextTargetSpeed
						});
					} else {
						var coastingDistance = Formulas.DecelerationDistance(currentSpeed, nextTargetSpeed,
							Driver.DriverData.LookAheadCoasting.Deceleration);
						Log.Debug("adding 'Coasting' starting at distance {0}. coastingDistance: {1}, triggerDistance: {2}",
							entry.Distance - coastingDistance, coastingDistance, entry.Distance);
						nextActions.Add(
							new DrivingBehaviorEntry {
								Action = DrivingBehavior.Coasting,
								ActionDistance = entry.Distance - coastingDistance,
								TriggerDistance = entry.Distance,
								NextTargetSpeed = nextTargetSpeed
							});
					}
				}
				//if (nextTargetSpeed > currentSpeed) {
				//	nextActions.Add(new DrivingBehaviorEntry {
				//		Action = DefaultDriverStrategy.DrivingBehavior.Accelerating,
				//		NextTargetSpeed = entry.VehicleTargetSpeed,
				//		TriggerDistance = entry.Distance,
				//		ActionDistance = entry.Distance
				//	});
				//}
			}

			return nextActions.Count == 0 ? null : nextActions.OrderBy(x => x.ActionDistance).First();
		}

		public bool OverspeedAllowed(Radian gradient, MeterPerSecond velocity)
		{
			return Driver.DriverData.OverSpeedEcoRoll.Mode == DriverData.DriverMode.Overspeed &&
					gradient < 0 && velocity > Driver.DriverData.OverSpeedEcoRoll.MinSpeed;
		}
	}

	//=====================================


	public interface IDriverMode
	{
		DefaultDriverStrategy DriverStrategy { get; set; }

		IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient);

		void ResetMode();
	}

	public abstract class AbstractDriverMode : LoggingObject, IDriverMode
	{
		private DefaultDriverStrategy _driverStrategy;
		private IDriverActions _driver;
		private DriverData _driverData;
		private IDataBus _dataBus;

		public DefaultDriverStrategy DriverStrategy
		{
			get { return _driverStrategy; }
			set { _driverStrategy = value; }
		}

		protected IDriverActions Driver
		{
			get { return _driver ?? (_driver = _driverStrategy.Driver); }
		}

		protected DriverData DriverData
		{
			get { return _driverData ?? (_driverData = Driver.DriverData); }
		}

		protected IDataBus DataBus
		{
			get { return _dataBus ?? (_dataBus = Driver.DataBus); }
		}

		public abstract IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient);
		public abstract void ResetMode();
	}

	//=====================================

	public class DriverModeDrive : AbstractDriverMode
	{
		public override IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			IResponse response = null;

			var velocity = targetVelocity;
			if (DriverStrategy.OverspeedAllowed(gradient, targetVelocity)) {
				velocity += DriverData.OverSpeedEcoRoll.OverSpeed;
			}
			if (DataBus.ClutchClosed(absTime)) {
				// drive along
				if (DriverStrategy.OverspeedAllowed(gradient, targetVelocity) && DataBus.VehicleSpeed.IsEqual(targetVelocity)) {
					response = Driver.DrivingActionCoast(absTime, ds, velocity, gradient);
					if (response is ResponseSuccess && response.Acceleration < 0) {
						response = Driver.DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
					}
				} else {
					response = Driver.DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
				}
				response.Switch().
					Case<ResponseGearShift>(() => {
						response = Driver.DrivingActionRoll(absTime, ds, velocity, gradient);
						response.Switch().
							Case<ResponseUnderload>(() => {
								// overload may happen if driver limits acceleration when rolling downhill
								response = Driver.DrivingActionBrake(absTime, ds, velocity, gradient);
							}).
							Case<ResponseSpeedLimitExceeded>(() => {
								response = Driver.DrivingActionBrake(absTime, ds, velocity, gradient);
							});
					}).
					Case<ResponseUnderload>(r => {
						if (DriverStrategy.OverspeedAllowed(gradient, targetVelocity)) {
							response = Driver.DrivingActionCoast(absTime, ds, velocity, gradient);
							if (response is ResponseUnderload || response is ResponseSpeedLimitExceeded) {
								response = Driver.DrivingActionBrake(absTime, ds, velocity, gradient);
							}
						} else {
							response = Driver.DrivingActionBrake(absTime, ds, velocity, gradient);
						}
					});
			} else {
				response = Driver.DrivingActionRoll(absTime, ds, velocity, gradient);
				response.Switch().
					Case<ResponseUnderload>(r => {
						response = Driver.DrivingActionBrake(absTime, ds, velocity, gradient, r);
					}).Case<ResponseSpeedLimitExceeded>(() => {
						response = Driver.DrivingActionBrake(absTime, ds, velocity, gradient);
					});
			}
			return response;
		}

		public override void ResetMode() {}
	}

	//=====================================

	public class DriverModeBrake : AbstractDriverMode
	{
		protected enum BrakingPhase
		{
			Coast,
			Brake
		}

		protected BrakingPhase Phase;

		public override IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			IResponse response = null;
			if (DataBus.VehicleSpeed <= DriverStrategy.BrakeTrigger.NextTargetSpeed) {
				response = DataBus.ClutchClosed(absTime)
					? Driver.DrivingActionAccelerate(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed, gradient)
					: Driver.DrivingActionRoll(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed, gradient);
				response.Switch().
					Case<ResponseGearShift>(() => {
						response = Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						response.Switch().
							Case<ResponseUnderload>(r => {
								// under-load may happen if driver limits acceleration when rolling downhill
								response = Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
									gradient, r);
							}).
							Case<ResponseSpeedLimitExceeded>(() => {
								response = Driver.DrivingActionBrake(absTime, ds, DataBus.VehicleSpeed,
									gradient);
							});
					}).
					Case<ResponseSpeedLimitExceeded>(() => {
						response = Driver.DrivingActionBrake(absTime, ds, DataBus.VehicleSpeed,
							gradient);
					}).
					Case<ResponseUnderload>(r => {
						response = Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
							gradient, r);
					});
				return response;
			}
			var currentDistance = DataBus.Distance;

			if (Phase == BrakingPhase.Coast) {
				var brakingDistance = Driver.ComputeDecelerationDistance(DriverStrategy.BrakeTrigger.NextTargetSpeed);
				Log.Debug("breaking distance: {0}, start braking @ {1}", brakingDistance,
					DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance);
				if (currentDistance + Constants.SimulationSettings.DriverActionDistanceTolerance >
					DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance) {
					Phase = BrakingPhase.Brake;
					Log.Debug("Switching to BRAKE Phase. currentDistance: {0}", currentDistance);
				} else {
					if ((currentDistance + ds).IsGreater(DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance)) {
						return new ResponseDrivingCycleDistanceExceeded() {
							//Source = this,
							MaxDistance = DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance - currentDistance
						};
					}
				}
				if (DataBus.VehicleSpeed < Constants.SimulationSettings.MinVelocityForCoast) {
					Phase = BrakingPhase.Brake;
					Log.Debug("Switching to BRAKE Phase. currentDistance: {0}  v: {1}", currentDistance,
						DataBus.VehicleSpeed);
				}
			}
			switch (Phase) {
				case BrakingPhase.Coast:
					DriverStrategy.DriverBehavior = DrivingBehavior.Coasting;
					response = DataBus.ClutchClosed(absTime)
						? Driver.DrivingActionCoast(absTime, ds, targetVelocity, gradient)
						: Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
					response.Switch().
						Case<ResponseUnderload>(r => {
							// coast would decelerate more than driver's max deceleration => issue brakes to decelerate with driver's max deceleration
							response = Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
								gradient, r);
							Phase = BrakingPhase.Brake;
						}).
						Case<ResponseOverload>(r => {
							// limiting deceleration while coast may result in an overload => issue brakes to decelerate with driver's max deceleration
							if (DataBus.ClutchClosed(absTime)) {
								response = Driver.DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
							} else {
								response = Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
							}
							//Phase = BrakingPhase.Brake;
						}).
						Case<ResponseGearShift>(r => {
							response = Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						}).
						Case<ResponseSpeedLimitExceeded>(() => {
							response = Driver.DrivingActionBrake(absTime, ds, DataBus.VehicleSpeed,
								gradient);
						});
					break;
				case BrakingPhase.Brake:
					var brakingDistance = Driver.ComputeDecelerationDistance(DriverStrategy.BrakeTrigger.NextTargetSpeed);
					Log.Debug("Phase: BRAKE. breaking distance: {0} start braking @ {1}", brakingDistance,
						DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance);
					if (DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance < currentDistance) {
						Log.Warn("Expected Braking Deceleration could not be reached!");
					}
					var targetDistance = DataBus.VehicleSpeed < Constants.SimulationSettings.MinVelocityForCoast
						? DriverStrategy.BrakeTrigger.TriggerDistance
						: null;
					DriverStrategy.DriverBehavior = DrivingBehavior.Braking;
					response = Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
						gradient, targetDistance: targetDistance);
					response.Switch().
						Case<ResponseOverload>(r => {
							Log.Warn("Got OverloadResponse during brake action - desired deceleration could not be reached! response: {0}", r);
							if (!DataBus.ClutchClosed(absTime)) {
								Log.Warn("Clutch is open - trying RollAction");
								response = Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
							}
						});
					break;
			}

			return response;
		}

		public override void ResetMode()
		{
			Phase = BrakingPhase.Coast;
		}
	}

	//=====================================

	[DebuggerDisplay("ActionDistance: {ActionDistance}, TriggerDistance: {TriggerDistance}, Action: {Action}")]
	public class DrivingBehaviorEntry
	{
		public DrivingBehavior Action;
		public MeterPerSecond NextTargetSpeed;
		public Meter TriggerDistance;
		public Meter ActionDistance;

		public bool HasEqualTrigger(DrivingBehaviorEntry other)
		{
			return TriggerDistance.IsEqual(other.TriggerDistance) && NextTargetSpeed.IsEqual(other.NextTargetSpeed);
		}

		public override string ToString()
		{
			return string.Format("action: {0} @ {1}. trigger: {2} targetSpeed: {3}", Action, ActionDistance, TriggerDistance,
				NextTargetSpeed);
		}
	}
}