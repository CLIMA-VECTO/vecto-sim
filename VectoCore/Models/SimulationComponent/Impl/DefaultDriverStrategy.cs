using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

		protected Meter BrakeTriggerDistance { get; set; }


		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			switch (CurrentDrivingMode) {
				case DrivingMode.DrivingModeDrive:
					var currentDistance = Driver.DataBus.Distance();
					var nextAction = GetNextDrivingAction(currentDistance);
					if (currentDistance.IsEqual(nextAction.ActionDistance)) {
						CurrentDrivingMode = DrivingMode.DrivingModeBrake;
						BrakeTriggerDistance = nextAction.TriggerDistance;
						break;
					}
					if (currentDistance + ds > nextAction.ActionDistance) {
						return new ResponseDrivingCycleDistanceExceeded() {
							MaxDistance = nextAction.ActionDistance - currentDistance
						};
					}
					break;
				case DrivingMode.DrivingModeBrake:
					if (Driver.DataBus.Distance() >= BrakeTriggerDistance) {
						CurrentDrivingMode = DrivingMode.DrivingModeDrive;
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
				DeclarationData.Driver.LookAhead.Deceleration);

			var lookaheadData = Driver.DataBus.LookAhead(1.2 * lookaheadDistance);

			Log.Debug("Lookahead distance: {0} @ current speed {1}", lookaheadDistance, currentSpeed);
			var nextActions = new List<DrivingBehaviorEntry>();
			for (var i = 0; i < lookaheadData.Count; i++) {
				var entry = lookaheadData[i];
				if (entry.VehicleTargetSpeed < currentSpeed) {
					var breakingDistance = Driver.ComputeDecelerationDistance(entry.VehicleTargetSpeed);
					Log.Debug("distance to decelerate from {0} to {1}: {2}", currentSpeed, entry.VehicleTargetSpeed,
						breakingDistance);
					Log.Debug("adding 'Braking' starting at distance {0}", entry.Distance - breakingDistance);
					nextActions.Add(
						new DrivingBehaviorEntry {
							Action = DefaultDriverStrategy.DrivingBehavior.Braking,
							ActionDistance = entry.Distance - breakingDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = entry.VehicleTargetSpeed
						});
					var coastingDistance = Formulas.DecelerationDistance(currentSpeed, entry.VehicleTargetSpeed,
						DeclarationData.Driver.LookAhead.Deceleration);
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

			return nextActions.OrderBy(x => x.ActionDistance).First();
		}
	}

	//=====================================


	public interface IDriverMode
	{
		DefaultDriverStrategy DriverStrategy { get; set; }

		IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient);
	}

	//=====================================

	public class DriverModeDrive : IDriverMode
	{
		public DefaultDriverStrategy DriverStrategy { get; set; }

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			IResponse response = null;
			if (DriverStrategy.Driver.DataBus.ClutchState() != ClutchState.ClutchOpened) {
				// drive along
				response = DriverStrategy.Driver.DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
				response.Switch().
					Case<ResponseGearShift>(() => {
						response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, gradient);
						response.Switch().
							Case<ResponseOverload>(() => {
								// overload may happen if driver limits acceleration when rolling downhill
								response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient);
							});
					}).
					Case<ResponseOverload>(() => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient);
					});
			} else {
				response = DriverStrategy.Driver.DrivingActionRoll(absTime, ds, gradient);
				response.Switch().
					Case<ResponseOverload>(() => {
						response = DriverStrategy.Driver.DrivingActionBrake(absTime, ds, targetVelocity, gradient);
					});
			}
			return response;
		}
	}

	//=====================================

	public class DriverModeBrake : IDriverMode
	{
		public DefaultDriverStrategy DriverStrategy { get; set; }

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			throw new System.NotImplementedException();
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