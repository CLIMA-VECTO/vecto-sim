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
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class DefaultDriverStrategy : LoggingObject, IDriverStrategy
	{
		public enum DrivingMode
		{
			DrivingModeDrive,
			DrivingModeBrake,
		}

		public enum DrivingBehavior
		{
			Accelerating,
			Drive,
			Coasting,
			Braking,
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
			DrivingBehaviorEntry nextAction = null;
			switch (CurrentDrivingMode) {
				case DrivingMode.DrivingModeDrive:
					var currentDistance = Driver.DataBus.Distance;
					nextAction = GetNextDrivingAction(currentDistance);
					if (nextAction != null) {
						if (currentDistance.IsEqual(nextAction.ActionDistance,
							Constants.SimulationSettings.DriverActionDistanceTolerance.Value())) {
							CurrentDrivingMode = DrivingMode.DrivingModeBrake;
							DrivingModes[CurrentDrivingMode].ResetMode();
							Log.Debug("Switching to DrivingMode BRAKE");

							BrakeTrigger = nextAction;
							break;
						}
						if ((currentDistance + ds).IsGreater(nextAction.ActionDistance)) {
							Log.Debug("Current simulation interval exceeds next action distance at {0}. reducing maxDistance to {1}",
								nextAction.ActionDistance, nextAction.ActionDistance - currentDistance);
							return new ResponseDrivingCycleDistanceExceeded() {
								Source = this,
								MaxDistance = nextAction.ActionDistance - currentDistance
							};
						}
					}
					break;
				case DrivingMode.DrivingModeBrake:
					if (Driver.DataBus.Distance >= BrakeTrigger.TriggerDistance) {
						CurrentDrivingMode = DrivingMode.DrivingModeDrive;
						DrivingModes[CurrentDrivingMode].ResetMode();
						Log.Debug("Switching to DrivingMode DRIVE");
					}
					break;
			}

			var retVal = DrivingModes[CurrentDrivingMode].Request(absTime, ds, targetVelocity, gradient);

			if (nextAction == null || !(retVal is ResponseSuccess)) {
				return retVal;
			}
			var v2 = Driver.DataBus.VehicleSpeed + retVal.Acceleration * retVal.SimulationInterval;
			if (v2 <= nextAction.NextTargetSpeed) {
				return retVal;
			}

			var coastingDistance = Formulas.DecelerationDistance(v2, nextAction.NextTargetSpeed,
				Driver.LookaheadDeceleration);
			if (Driver.DataBus.Distance.IsEqual(nextAction.TriggerDistance - coastingDistance)) {
				return retVal;
			}
			if ((Driver.DataBus.Distance + ds).IsSmallerOrEqual(nextAction.TriggerDistance - coastingDistance)) {
				return retVal;
			}
			Log.Debug("Exceeding next ActionDistance at {0}. Reducing max Distance to {1}", nextAction.ActionDistance,
				nextAction.TriggerDistance - coastingDistance - Driver.DataBus.Distance);
			return new ResponseDrivingCycleDistanceExceeded() {
				Source = this,
				MaxDistance = nextAction.TriggerDistance - coastingDistance - Driver.DataBus.Distance,
			};
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			return Driver.DrivingActionHalt(absTime, dt, targetVelocity, gradient);
		}


		protected DrivingBehaviorEntry GetNextDrivingAction(Meter minDistance)
		{
			var currentSpeed = Driver.DataBus.VehicleSpeed;

			// distance until halt
			var lookaheadDistance = Formulas.DecelerationDistance(currentSpeed, 0.SI<MeterPerSecond>(),
				Driver.LookaheadDeceleration);

			var lookaheadData = Driver.DataBus.LookAhead(1.2 * lookaheadDistance);

			Log.Debug("Lookahead distance: {0} @ current speed {1}", lookaheadDistance, currentSpeed);
			var nextActions = new List<DrivingBehaviorEntry>();
			for (var i = 0; i < lookaheadData.Count; i++) {
				var entry = lookaheadData[i];
				if (entry.VehicleTargetSpeed < currentSpeed) {
					var coastingDistance = Formulas.DecelerationDistance(currentSpeed, entry.VehicleTargetSpeed,
						Driver.LookaheadDeceleration);
					Log.Debug("adding 'Coasting' starting at distance {0}", entry.Distance - coastingDistance);
					nextActions.Add(
						new DrivingBehaviorEntry {
							Action = DefaultDriverStrategy.DrivingBehavior.Coasting,
							ActionDistance = entry.Distance - coastingDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = entry.VehicleTargetSpeed
						});
				}
				if (entry.VehicleTargetSpeed > currentSpeed) {
					nextActions.Add(new DrivingBehaviorEntry {
						Action = DefaultDriverStrategy.DrivingBehavior.Accelerating,
						NextTargetSpeed = entry.VehicleTargetSpeed,
						TriggerDistance = entry.Distance,
						ActionDistance = entry.Distance
					});
				}
			}

			if (nextActions.Count == 0) {
				return null;
			}
			return nextActions.OrderBy(x => x.ActionDistance).First();
		}
	}

	//=====================================


	public interface IDriverMode
	{
		DefaultDriverStrategy DriverStrategy { get; set; }

		IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient);

		void ResetMode();
	}

	//=====================================

	public class DriverModeDrive : LoggingObject, IDriverMode
	{
		public DefaultDriverStrategy DriverStrategy { get; set; }

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			IResponse response = null;
			if (DriverStrategy.Driver.DataBus.ClutchClosed(absTime)) {
				// drive along
				response = DriverStrategy.Driver.DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
				response.Switch().
					Case<ResponseGearShift>(() => {
						response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						response.Switch().
							Case<ResponseUnderload>(() => {
								// overload may happen if driver limits acceleration when rolling downhill
								response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient);
							}).
							Case<ResponseSpeedLimitExceeded>(() => {
								response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient);
							});
					}).
					Case<ResponseUnderload>(r => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient, r);
					});
			} else {
				response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
				response.Switch().
					Case<ResponseUnderload>(r => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient, r);
					}).Case<ResponseSpeedLimitExceeded>(() => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient);
					});
			}
			return response;
		}

		public void ResetMode() {}
	}

	//=====================================

	public class DriverModeBrake : LoggingObject, IDriverMode
	{
		protected enum BrakingPhase
		{
			Coast,
			Brake
		}

		public DefaultDriverStrategy DriverStrategy { get; set; }

		protected BrakingPhase Phase;

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			IResponse response = null;
			if (DriverStrategy.Driver.DataBus.VehicleSpeed <= DriverStrategy.BrakeTrigger.NextTargetSpeed) {
				response = DriverStrategy.Driver.DataBus.ClutchClosed(absTime)
					? DriverStrategy.Driver.DrivingActionAccelerate(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed, gradient)
					: DriverStrategy.Driver.DrivingActionRoll(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed, gradient);
				response.Switch().
					Case<ResponseGearShift>(() => {
						response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						response.Switch().
							Case<ResponseUnderload>(r => {
								// under-load may happen if driver limits acceleration when rolling downhill
								response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
									gradient, r);
							}).
							Case<ResponseSpeedLimitExceeded>(() => {
								response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.Driver.DataBus.VehicleSpeed,
									gradient);
							});
					}).
					Case<ResponseSpeedLimitExceeded>(() => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.Driver.DataBus.VehicleSpeed,
							gradient);
					}).
					Case<ResponseUnderload>(r => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
							gradient, r);
					});
				return response;
			}
			var currentDistance = DriverStrategy.Driver.DataBus.Distance;
			if (Phase == BrakingPhase.Coast) {
				var brakingDistance = DriverStrategy.Driver.ComputeDecelerationDistance(DriverStrategy.BrakeTrigger.NextTargetSpeed);
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
				if (DriverStrategy.Driver.DataBus.VehicleSpeed < 5.KMPHtoMeterPerSecond()) {
					Phase = BrakingPhase.Brake;
					Log.Debug("Switching to BRAKE Phase. currentDistance: {0}  v: {1}", currentDistance,
						DriverStrategy.Driver.DataBus.VehicleSpeed);
				}
			}
			switch (Phase) {
				case BrakingPhase.Coast:
					response = DriverStrategy.Driver.DataBus.ClutchClosed(absTime)
						? DriverStrategy.Driver.DrivingActionCoast(absTime, ds, targetVelocity, gradient)
						: DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
					response.Switch().
						Case<ResponseUnderload>(r => {
							// coast would decelerate more than driver's max deceleration => issue brakes to decelerate with driver's max deceleration
							response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
								gradient, r);
							Phase = BrakingPhase.Brake;
						}).
						Case<ResponseGearShift>(r => {
							response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						}).
						Case<ResponseSpeedLimitExceeded>(() => {
							response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.Driver.DataBus.VehicleSpeed,
								gradient);
						});
					break;
				case BrakingPhase.Brake:
					var brakingDistance =
						DriverStrategy.Driver.ComputeDecelerationDistance(DriverStrategy.BrakeTrigger.NextTargetSpeed);
					Log.Debug("Phase: BRAKE. breaking distance: {0} start braking @ {1}", brakingDistance,
						DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance);
					if (DriverStrategy.BrakeTrigger.TriggerDistance - brakingDistance < currentDistance) {
						Log.Warn("Expected Braking Deceleration could not be reached!");
					}
					response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
						gradient);
					break;
			}

			return response;
		}

		public void ResetMode()
		{
			Phase = BrakingPhase.Coast;
		}
	}

	//=====================================

	[DebuggerDisplay("ActionDistance: {ActionDistance}, TriggerDistance: {TriggerDistance}, Action: {Action}")]
	public class DrivingBehaviorEntry
	{
		public DefaultDriverStrategy.DrivingBehavior Action;
		public MeterPerSecond NextTargetSpeed;
		public Meter TriggerDistance;
		public Meter ActionDistance;
	}
}