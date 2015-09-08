using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
			switch (CurrentDrivingMode) {
				case DrivingMode.DrivingModeDrive:
					var currentDistance = Driver.DataBus.Distance;
					var nextAction = GetNextDrivingAction(currentDistance);
					if (nextAction != null && currentDistance.IsEqual(nextAction.ActionDistance)) {
						CurrentDrivingMode = DrivingMode.DrivingModeBrake;
						DrivingModes[CurrentDrivingMode].ResetMode();
						BrakeTrigger = nextAction;
						break;
					}
					if (nextAction != null && currentDistance + ds > nextAction.ActionDistance) {
						return new ResponseDrivingCycleDistanceExceeded() {
							MaxDistance = nextAction.ActionDistance - currentDistance
						};
					}
					break;
				case DrivingMode.DrivingModeBrake:
					if (Driver.DataBus.Distance >= BrakeTrigger.TriggerDistance) {
						CurrentDrivingMode = DrivingMode.DrivingModeDrive;
						DrivingModes[CurrentDrivingMode].ResetMode();
					}
					break;
			}

			return DrivingModes[CurrentDrivingMode].Request(absTime, ds, targetVelocity, gradient);
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			return Driver.DrivingActionHalt(absTime, dt, targetVelocity, gradient);
		}


		protected DrivingBehaviorEntry GetNextDrivingAction(Meter minDistance)
		{
			var currentSpeed = Driver.DataBus.VehicleSpeed();

			// distance until halt
			var lookaheadDistance = Formulas.DecelerationDistance(currentSpeed, 0.SI<MeterPerSecond>(),
				Driver.LookaheadDeceleration);

			var lookaheadData = Driver.DataBus.LookAhead(1.2 * lookaheadDistance);

			Log.Debug("Lookahead distance: {0} @ current speed {1}", lookaheadDistance, currentSpeed);
			var nextActions = new List<DrivingBehaviorEntry>();
			for (var i = 0; i < lookaheadData.Count; i++) {
				var entry = lookaheadData[i];
				if (entry.VehicleTargetSpeed < currentSpeed) {
					//var breakingDistance = Driver.ComputeDecelerationDistance(entry.VehicleTargetSpeed);
					//Log.Debug("distance to decelerate from {0} to {1}: {2}", currentSpeed, entry.VehicleTargetSpeed,
					//	breakingDistance);
					//Log.Debug("adding 'Braking' starting at distance {0}", entry.Distance - breakingDistance);
					//nextActions.Add(
					//	new DrivingBehaviorEntry {
					//		Action = DefaultDriverStrategy.DrivingBehavior.Braking,
					//		ActionDistance = entry.Distance - breakingDistance,
					//		TriggerDistance = entry.Distance,
					//		NextTargetSpeed = entry.VehicleTargetSpeed
					//	});
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

	public class DriverModeDrive : IDriverMode
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
					});
			}
			return response;
		}

		public void ResetMode() {}
	}

	//=====================================

	public class DriverModeBrake : IDriverMode
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
			if (DriverStrategy.Driver.DataBus.VehicleSpeed() <= DriverStrategy.BrakeTrigger.NextTargetSpeed) {
				response = DriverStrategy.Driver.DrivingActionAccelerate(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
					gradient);
				response.Switch().
					Case<ResponseGearShift>(() => {
						response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						response.Switch().
							Case<ResponseUnderload>(r => {
								// under-load may happen if driver limits acceleration when rolling downhill
								response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
									gradient, r);
							});
					}).
					Case<ResponseUnderload>(r => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
							gradient, r);
					});
				return response;
			}
			var currentDistance = DriverStrategy.Driver.DataBus.Distance;
			if (Phase == BrakingPhase.Coast) {
				var breakingDistance = DriverStrategy.Driver.ComputeDecelerationDistance(DriverStrategy.BrakeTrigger.NextTargetSpeed);
				if (currentDistance + ds > DriverStrategy.BrakeTrigger.TriggerDistance - breakingDistance) {
					return new ResponseDrivingCycleDistanceExceeded() {
						MaxDistance = DriverStrategy.BrakeTrigger.TriggerDistance - breakingDistance - currentDistance
					};
				}
				if (currentDistance + Constants.SimulationSettings.DriverActionDistanceTolerance >
					DriverStrategy.BrakeTrigger.TriggerDistance - breakingDistance) {
					Phase = BrakingPhase.Brake;
				}
			}
			switch (Phase) {
				case BrakingPhase.Coast:
					response = DriverStrategy.Driver.DrivingActionCoast(absTime, ds, targetVelocity, gradient);
					response.Switch().
						Case<ResponseUnderload>(r => {
							// coast would decelerate more than driver's max deceleration => issue brakes to decelerate with driver's max deceleration
							response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
								gradient, r);
							Phase = BrakingPhase.Brake;
						}).
						Case<ResponseGearShift>(r => {
							response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, targetVelocity, gradient);
						});
					break;
				case BrakingPhase.Brake:
					response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, DriverStrategy.BrakeTrigger.NextTargetSpeed,
						gradient);
					break;
			}
			// todo: @@@quam: add resonse.switch to indicate expected responses?
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