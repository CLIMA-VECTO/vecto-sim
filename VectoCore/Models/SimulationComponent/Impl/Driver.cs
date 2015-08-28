using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Driver : VectoSimulationComponent, IDriver, IDrivingCycleOutPort, IDriverDemandInPort
	{
		internal DriverState CurrentState = new DriverState();

		protected IDriverDemandOutPort Next;

		protected DriverData DriverData;

		public enum DrivingBehavior
		{
			Stopped,
			Accelerating,
			Drive,
			Coasting,
			Breaking,
			//EcoRoll,
			//OverSpeed,
		}

		[DebuggerDisplay("ActionDistance: {ActionDistance}, TriggerDistance: {TriggerDistance}, Action: {Action}")]
		public class DrivingBehaviorEntry
		{
			public DrivingBehavior Action;
			public MeterPerSecond NextTargetSpeed;
			public Meter TriggerDistance;
			public Meter ActionDistance;
		}

		public Driver(VehicleContainer container, DriverData driverData) : base(container)
		{
			DriverData = driverData;
		}

		public IDriverDemandInPort InPort()
		{
			return this;
		}

		public void Connect(IDriverDemandOutPort other)
		{
			Next = other;
		}


		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			return Next.Initialize(vehicleSpeed, roadGradient);
		}


		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			Log.Debug("==== DRIVER Request ====");
			Log.Debug(
				"Request: absTime: {0},  ds: {1}, targetVelocity: {2}, gradient: {3} | distance: {4}, velocity: {5}", absTime, ds,
				targetVelocity, gradient, DataBus.Distance(), DataBus.VehicleSpeed());

			var retVal = DoHandleRequest(absTime, ds, targetVelocity, gradient);

			CurrentState.Response = retVal;
			return retVal;
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			Log.Debug("==== DRIVER Request ====");
			Log.Debug("Request: absTime: {0},  dt: {1}, targetVelocity: {2}, gradient: {3}", absTime, dt, targetVelocity,
				gradient);
			var retVal = DoHandleRequest(absTime, dt, targetVelocity, gradient);

			CurrentState.Response = retVal;
			return retVal;
		}


		protected IResponse DoHandleRequest(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			var currentDistance = DataBus.Distance();
			var nextDrivingActions = GetNextDrivingActions(currentDistance);

			Log.Debug(", ".Join(nextDrivingActions.Select(x => string.Format("({0}: {1})", x.ActionDistance, x.Action))));


			if (CurrentState.DrivingAction.Action == DrivingBehavior.Stopped && targetVelocity >= DataBus.VehicleSpeed()) {
				CurrentState.DrivingAction.Action = DrivingBehavior.Drive;
			}

			if (nextDrivingActions.Count > 0) {
				// if we exceeded the previous action (by accident), set the action anyway in case there is no 'next action'...
				CurrentState.DrivingAction = nextDrivingActions.LastOrDefault(x => x.ActionDistance < currentDistance) ??
											CurrentState.DrivingAction;

				var nextActions = nextDrivingActions.Where(x => x.ActionDistance >= currentDistance).ToList();
				var nextDrivingAction = nextActions.GetEnumerator();
				nextDrivingAction.MoveNext();
				var hasNextEntry = true;

				// if the current position matches the next action - set new action.
				if (nextDrivingAction.Current.ActionDistance.IsEqual(currentDistance,
					Constants.SimulationSettings.DriverActionDistanceTolerance.Value())) {
					CurrentState.DrivingAction = nextDrivingAction.Current;
					hasNextEntry = nextDrivingAction.MoveNext(); // the current action has already been processed, look at next one...
				}
				// check if desired distance exceeds next action point
				if (hasNextEntry && nextDrivingAction.Current.ActionDistance < currentDistance + ds) {
					Log.Debug(
						"current Distance: {3} -- Simulation Distance {0} exceeds next DrivingAction at {1}, reducing interval to {2}", ds,
						nextDrivingAction.Current.ActionDistance, nextDrivingAction.Current.ActionDistance - currentDistance,
						currentDistance);
					return new ResponseDrivingCycleDistanceExceeded {
						MaxDistance = nextDrivingAction.Current.ActionDistance - currentDistance
					};
				}
			} else {
				if (targetVelocity > DataBus.VehicleSpeed()) {
					CurrentState.DrivingAction.Action = DrivingBehavior.Accelerating;
				}
			}
			Log.Debug("DrivingAction: {0}", CurrentState.DrivingAction.Action);
			//CurrentState.DrivingAction = nextAction;
			switch (CurrentState.DrivingAction.Action) {
				case DrivingBehavior.Accelerating:
					return DriveOrAccelerate(absTime, ds, targetVelocity, gradient);
				case DrivingBehavior.Drive:
					return DriveOrAccelerate(absTime, ds, targetVelocity, gradient);
				case DrivingBehavior.Coasting:
					return DoCoast(absTime, ds, gradient);
				case DrivingBehavior.Breaking:
					return DoBreak(absTime, ds, gradient, CurrentState.DrivingAction.NextTargetSpeed);
			}
			throw new VectoSimulationException("unhandled driving action " + CurrentState.DrivingAction);
		}

		private IResponse DoBreak(Second absTime, Meter ds, Radian gradient, MeterPerSecond nextTargetSpeed)
		{
			ComputeAcceleration(ref ds, nextTargetSpeed);

			// todo: still required?
			if (CurrentState.dt.IsEqual(0)) {
				return new ResponseFailTimeInterval();
			}

			var response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);

			var dryRun = response as ResponseDryRun;
			if (dryRun == null) {
				throw new VectoSimulationException("Expected DryRunResponse after Dry-Run Request!");
			}

			var newDs = ds;
			SearchBreakingPower(absTime, ref newDs, gradient, dryRun, true);

			if (!ds.IsEqual(newDs)) {
				Log.Debug(
					"SearchOperatingPoint Breaking reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!", newDs,
					ds);
				return new ResponseDrivingCycleDistanceExceeded { MaxDistance = newDs, SimulationInterval = CurrentState.dt };
			}

			Log.Debug("Found operating point for breaking. dt: {0}, acceleration: {1}", CurrentState.dt,
				CurrentState.Acceleration);
			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;

			retVal.Switch().
				Case<ResponseSuccess>(r => r.SimulationInterval = CurrentState.dt).
				Default(() => Log.Debug("unhandled response from powertrain: {0}", retVal));

			return retVal; //new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		private void SearchBreakingPower(Second absTime, ref Meter ds, Radian gradient, ResponseDryRun response, bool coasting)
		{
			var debug = new List<dynamic>(); // only used while testing
			var breakingPower = (DataBus.ClutchState() != ClutchState.ClutchClosed)
				? response.AxlegearPowerRequest.Abs()
				: response.DeltaDragLoad.Abs();

			var searchInterval = Constants.SimulationSettings.BreakingPowerInitialSearchInterval;

			var originalDs = ds;
			Watt origDelta = null;

			// double the searchInterval until a good interval was found
			var intervalFactor = 2.0;

			do {
				ds = originalDs;
				var delta = DataBus.ClutchState() == ClutchState.ClutchClosed
					? -response.DeltaDragLoad
					: -response.AxlegearPowerRequest;

				debug.Add(new { breakingPower, searchInterval, delta });

				if (origDelta == null) {
					origDelta = delta;
				} else {
					// check if a correct searchInterval was found (when the delta changed signs, we stepped through the 0-point)
					// from then on the searchInterval can be bisected.
					if (origDelta.Sign() != delta.Sign()) {
						intervalFactor = 0.5;
					}
				}

				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					Log.Debug("found operating point in {0} iterations, delta: {1}", debug.Count, delta);
					return;
				}


				breakingPower += searchInterval * -delta.Sign();
				searchInterval *= intervalFactor;

				CurrentState.dt = ComputeTimeInterval(CurrentState.Acceleration, ref ds);
				DataBus.BreakPower = breakingPower;
				response = (ResponseDryRun)Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			Log.Debug("Exceeded max iterations when searching for operating point!");
			Log.Debug("exceeded: {0} ... {1}", ", ".Join(debug.Take(5)), ", ".Join(debug.Slice(-6)));
			Log.Error("Failed to find operating point for breaking!");
			throw new VectoSimulationException("Failed to find operating point for breaking!");
		}

		private IResponse DriveOrAccelerate(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			ComputeAcceleration(ref ds, targetVelocity);
			if (CurrentState.dt.IsEqual(0)) {
				return new ResponseFailTimeInterval();
			}

			IResponse retVal = null;
			do {
				var response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
				response.Switch().
					Case<ResponseSuccess>(r => {
						r.SimulationInterval = CurrentState.dt;
						retVal = r;
					}).
					Case<ResponseEngineOverload>(r => {
						if (r != null && r.Delta < 0) {
							// if Delta is negative we are already below the Drag-load curve. activate breaks
							retVal = DoBreak(absTime, ds, gradient, targetVelocity);
						} else {
							var doAccelerate = (DataBus.VehicleSpeed() - targetVelocity).Abs() > 0.1 * targetVelocity;

							SearchOperatingPoint(absTime, ref ds, gradient, r, accelerating: doAccelerate);
							Log.Debug("Found operating point for Drive/Accelerate. dt: {0}, acceleration: {1}, doAccelerate: {2}",
								CurrentState.dt, CurrentState.Acceleration, doAccelerate);
						}
					}).
					Case<ResponseGearShift>(() => { }).
					Case<ResponseGearboxOverload>(r => {
						if (r != null && r.Delta < 0) {
							// if Delta is negative we are below the Drag-load curve: activate breaks
							retVal = DoBreak(absTime, ds, gradient, targetVelocity);
						} else {
							var doAccelerate = (DataBus.VehicleSpeed() - targetVelocity).Abs() > 0.1 * targetVelocity;

							SearchOperatingPoint(absTime, ref ds, gradient, r, accelerating: doAccelerate);
							Log.Debug("Found operating point for Drive/Accelerate. dt: {0}, acceleration: {1}, doAccelerate: {2}",
								CurrentState.dt, CurrentState.Acceleration, doAccelerate);
						}
					}).
					Default(r => { throw new VectoException(string.Format("Unknown Response: {0}", r)); });
				if (retVal != null) {
					return retVal;
				}
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			return new ResponseDrivingCycleDistanceExceeded { SimulationInterval = CurrentState.dt };
		}

		protected IList<DrivingBehaviorEntry> GetNextDrivingActions(Meter minDistance)
		{
			var currentSpeed = DataBus.VehicleSpeed();

			// distance until halt
			var lookaheadDistance = Formulas.DecelerationDistance(currentSpeed, 0.SI<MeterPerSecond>(),
				DeclarationData.Driver.LookAhead.Deceleration);

			var lookaheadData = DataBus.LookAhead(1.2 * lookaheadDistance);

			Log.Debug("Lookahead distance: {0} @ current speed {1}", lookaheadDistance, currentSpeed);
			var nextActions = new List<DrivingBehaviorEntry>();
			for (var i = 0; i < lookaheadData.Count; i++) {
				var entry = lookaheadData[i];
				if (entry.VehicleTargetSpeed < currentSpeed) {
					var breakingDistance = ComputeDecelerationDistance(entry.VehicleTargetSpeed);
					Log.Debug("distance to decelerate from {0} to {1}: {2}", currentSpeed, entry.VehicleTargetSpeed,
						breakingDistance);
					Log.Debug("adding 'Braking' starting at distance {0}", entry.Distance - breakingDistance);
					nextActions.Add(
						new DrivingBehaviorEntry {
							Action = DrivingBehavior.Breaking,
							ActionDistance = entry.Distance - breakingDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = entry.VehicleTargetSpeed
						});
					var coastingDistance = Formulas.DecelerationDistance(currentSpeed, entry.VehicleTargetSpeed,
						DeclarationData.Driver.LookAhead.Deceleration);
					Log.Debug("adding 'Coasting' starting at distance {0}", entry.Distance - coastingDistance);
					nextActions.Add(
						new DrivingBehaviorEntry {
							Action = DrivingBehavior.Coasting,
							ActionDistance = entry.Distance - coastingDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = entry.VehicleTargetSpeed
						});
				}
				if (entry.VehicleTargetSpeed > currentSpeed) {
					nextActions.Add(new DrivingBehaviorEntry {
						Action = DrivingBehavior.Accelerating,
						NextTargetSpeed = entry.VehicleTargetSpeed,
						TriggerDistance = entry.Distance,
						ActionDistance = entry.Distance
					});
				}
			}

			return nextActions.OrderBy(x => x.ActionDistance).ToList();
		}

		protected internal Meter ComputeDecelerationDistance(MeterPerSecond targetSpeed)
		{
			return DriverData.AccelerationCurve.ComputeAccelerationDistance(DataBus.VehicleSpeed(), targetSpeed);
		}

		protected internal IResponse DoCoast(Second absTime, Meter ds, Radian gradient)
		{
			ComputeAcceleration(ref ds, 0.SI<MeterPerSecond>());

			// todo: still required?
			if (CurrentState.dt.IsEqual(0)) {
				return new ResponseFailTimeInterval();
			}

			var response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, dryRun: true);

			var newDs = ds;
			SearchOperatingPoint(absTime, ref newDs, gradient, response, coasting: true);

			if (!ds.IsEqual(newDs)) {
				Log.Debug(
					"SearchOperatingPoint reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!", newDs, ds);
				return new ResponseDrivingCycleDistanceExceeded { MaxDistance = newDs, SimulationInterval = CurrentState.dt };
			}

			Log.Debug("Found operating point for coasting. dt: {0}, acceleration: {1}", CurrentState.dt,
				CurrentState.Acceleration);

			if (CurrentState.Acceleration < DeclarationData.Driver.LookAhead.Deceleration) {
				Log.Debug("Limiting coasting deceleration from {0} to {1}", CurrentState.Acceleration,
					DeclarationData.Driver.LookAhead.Deceleration);
				CurrentState.Acceleration = DeclarationData.Driver.LookAhead.Deceleration;
				//CurrentState.dt = ComputeTimeInterval(CurrentState.Acceleration, ref ds);
				Log.Debug("Changed dt due to limited coasting deceleration. dt: {0}", CurrentState.dt);
			}


			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;

			retVal.Switch().
				Case<ResponseSuccess>(r => r.SimulationInterval = CurrentState.dt).
				Default(() => Log.Debug("unhandled response from powertrain: {0}", retVal));

			return retVal; //new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		/// <summary>
		/// search for the operating point where the engine's requested power is either on the full-load curve or  on the drag curve (parameter 'coasting').
		/// before the search can  be performed either a normal request or a dry-run request has to be made and the response is passed to this method.
		/// perform a binary search starting with the currentState's acceleration value.
		/// while searching it might be necessary to reduce the simulation distance because the vehicle already stopped before reaching the given ds. However,
		/// it for every new iteration of the search the original distance is used. The resulting distance is returned.
		/// After the search operation a normal request has to be made by the caller of this method. The final acceleration and time interval is stored in CurrentState.
		/// </summary>
		/// <param name="absTime">absTime from the original request</param>
		/// <param name="ds">ds from the original request</param>
		/// <param name="gradient">gradient from the original request</param>
		/// <param name="response">response of the former request that resulted in an overload response (or a dry-run response)</param>
		/// <param name="coasting">if true approach the drag-load curve, otherwise full-load curve</param>
		/// <param name="accelerating"></param>
		/// <returns></returns>
		private void SearchOperatingPoint(Second absTime, ref Meter ds, Radian gradient,
			IResponse response, bool coasting = false, bool accelerating = true)
		{
			var debug = new List<dynamic>(); // only used while testing
			var searchInterval = Constants.SimulationSettings.OperatingPointInitialSearchIntervalAccelerating;

			var originalDs = ds;

			if (coasting) {
				accelerating = false;
			}

			// double the searchInterval until a good interval was found
			var intervalFactor = 2.0;

			var delta = 0.SI<Watt>();
			Watt origDelta = null;

			do {
				ds = originalDs;
				response.Switch().
					Case<ResponseEngineOverload>(r => delta = r.Delta).
					Case<ResponseGearboxOverload>(r => delta = r.Delta).
					Case<ResponseDryRun>(r => delta = coasting ? r.DeltaDragLoad : r.DeltaFullLoad).
					Default(r => { throw new VectoSimulationException(string.Format("Unknown response type. {0}", r)); });

				if (origDelta == null) {
					origDelta = delta;
				} else {
					// check if a correct searchInterval was found (when the delta changed signs, we stepped through the 0-point)
					// from then on the searchInterval can be bisected.
					if (origDelta.Sign() != delta.Sign()) {
						intervalFactor = 0.5;
					}
				}

				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					Log.Debug(
						"found operating point in {0} iterations. Engine Power req: {2}, Gearbox Power req: {3} delta: {1}",
						debug.Count, delta, response.EnginePowerRequest, response.GearboxPowerRequest);
					return;
				}

				searchInterval *= intervalFactor;
				CurrentState.Acceleration += searchInterval * -delta.Sign();

				// check for minimum acceleration, add some safety margin due to search
				if (!coasting && accelerating &&
					CurrentState.Acceleration.Abs() < Constants.SimulationSettings.MinimumAcceleration.Value() / 5.0 &&
					searchInterval.Abs() < Constants.SimulationSettings.MinimumAcceleration / 20.0) {
					throw new VectoSimulationException("Could not achieve minimum acceleration");
				}
				debug.Add(new { delta, acceleration = CurrentState.Acceleration, searchInterval, intervalFactor });

				CurrentState.dt = ComputeTimeInterval(CurrentState.Acceleration, ref ds);

				response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			Log.Debug("Exceeded max iterations when searching for operating point!");
			Log.Debug("acceleration: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.acceleration)),
				", ".Join(debug.Slice(-6).Select(x => x.acceleration)));
			Log.Debug("exceeded: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.delta)),
				", ".Join(debug.Slice(-6).Select(x => x.delta)));
			Log.Error("Failed to find operating point! absTime: {0}", absTime);
			throw new VectoSimulationException(string.Format("Failed to find operating point! absTime: {0}", absTime));
		}

		/// <summary>
		/// compute the acceleration and time-interval such that the vehicle's velocity approaches the given target velocity
		/// - first compute the acceleration to reach the targetVelocity within the given distance
		/// - limit the acceleration/deceleration by the driver's acceleration curve
		/// - compute the time interval required to drive the given distance with the computed acceleration
		/// computed acceleration and time interval are stored in CurrentState!
		/// </summary>
		/// <param name="ds"></param>
		/// <param name="targetVelocity"></param>
		private void ComputeAcceleration(ref Meter ds, MeterPerSecond targetVelocity)
		{
			var currentSpeed = DataBus.VehicleSpeed();

			var requiredAverageSpeed = (targetVelocity + currentSpeed) / 2.0;
			var requiredAcceleration =
				(((targetVelocity - currentSpeed) * requiredAverageSpeed) / ds).Cast<MeterPerSquareSecond>();
			var maxAcceleration = DriverData.AccelerationCurve.Lookup(currentSpeed);

			if (requiredAcceleration > maxAcceleration.Acceleration) {
				requiredAcceleration = maxAcceleration.Acceleration;
			}
			if (requiredAcceleration < maxAcceleration.Deceleration) {
				requiredAcceleration = maxAcceleration.Deceleration;
			}

			CurrentState.Acceleration = requiredAcceleration;
			var tmpDs = ds;
			CurrentState.dt = ComputeTimeInterval(CurrentState.Acceleration, ref ds);
			if (!ds.IsEqual(tmpDs)) {
				Log.Error(
					"Unexpected Condition: Distance has been adjusted from {0} to {1}, currentVelocity: {2} acceleration: {3}, targetVelocity: {4}",
					tmpDs, ds, currentSpeed, CurrentState.Acceleration, targetVelocity);
				throw new VectoSimulationException("Simulation distance unexpectedly adjusted!");
			}
		}

		/// <summary>
		/// Computes the time interval for driving the given distance ds with the vehicle's current speed and the given acceleration.
		/// If the distance ds can not be reached (i.e., the vehicle would halt before ds is reached) then the distance parameter is adjusted.
		/// The computed time interval is returned via the out parameter dt
		/// </summary>
		/// <param name="acceleration"></param>
		/// <param name="ds"></param>
		private Second ComputeTimeInterval(MeterPerSquareSecond acceleration, ref Meter ds)
		{
			if (!(ds > 0)) {
				throw new VectoSimulationException("distance has to be greater than 0!");
			}
			var currentSpeed = DataBus.VehicleSpeed();

			if (acceleration.IsEqual(0)) {
				if (!(currentSpeed > 0)) {
					Log.Error("vehicle speed is {0}, acceleration is {1}", currentSpeed, acceleration);
					throw new VectoSimulationException("vehicle speed has to be > 0 if acceleration = 0");
				}
				return ds / currentSpeed;
			}

			// we need to accelerate / decelerate. solve quadratic equation...
			// ds = acceleration / 2 * dt^2 + currentSpeed * dt   => solve for dt
			var solutions = VectoMath.QuadraticEquationSolver(acceleration.Value() / 2.0, currentSpeed.Value(),
				-ds.Value());

			if (solutions.Count == 0) {
				// no real-valued solutions, required distance can not be reached (vehicle stopped), adapt ds...
				var dt = -currentSpeed / acceleration;
				var stopDistance = currentSpeed * dt + acceleration / 2 * dt * dt;
				if (stopDistance > ds) {
					Log.Warn(
						"Could not find solution for computing required time interval to drive distance {0}. currentSpeed: {1}, acceleration: {2}",
						ds, currentSpeed, acceleration);
					throw new VectoSimulationException("Could not find solution");
				}
				Log.Info(
					"Adjusted distance when computing time interval: currentSpeed: {0}, acceleration: {1}, distance: {2} -> {3}, timeInterval: {4}",
					currentSpeed, acceleration, ds, stopDistance, dt);
				ds = stopDistance;
				return dt;
			}
			solutions = solutions.Where(x => x >= 0).ToList();
			// if there are 2 positive solutions (i.e. when decelerating), take the smaller time interval
			// (the second solution means that you reach negative speed 
			return solutions.Min().SI<Second>();
		}


		protected IResponse DoHandleRequest(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			if (!targetVelocity.IsEqual(0) || !DataBus.VehicleSpeed().IsEqual(0)) {
				throw new NotImplementedException("TargetVelocity or VehicleVelocity is not zero!");
			}
			DataBus.Gear = 0;
			DataBus.BreakPower = double.PositiveInfinity.SI<Watt>();
			var retVal = Next.Request(absTime, dt, 0.SI<MeterPerSquareSecond>(), gradient);
			retVal.SimulationInterval = dt;
			return retVal;
		}


		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.acc] = CurrentState.Acceleration;
		}

		protected override void DoCommitSimulationStep()
		{
			if (!(CurrentState.Response is ResponseSuccess)) {
				throw new VectoSimulationException("Previous request did not succeed!");
			}
			CurrentState.RetryCount = 0;
			CurrentState.Response = null;

			if (CurrentState.DrivingAction.NextTargetSpeed != null &&
				DataBus.VehicleSpeed().IsEqual(CurrentState.DrivingAction.NextTargetSpeed)) {
				Log.Debug("reached target Speed {0} - set Driving action to {1}", CurrentState.DrivingAction.NextTargetSpeed,
					DrivingBehavior.Drive);
				CurrentState.DrivingAction.Action = DrivingBehavior.Drive;
			}
			if (DataBus.VehicleSpeed().IsEqual(0)) {
				Log.Debug("vehicle stopped {0} - set Driving action to {1}", DataBus.VehicleSpeed(), DrivingBehavior.Stopped);
				CurrentState.DrivingAction.Action = DrivingBehavior.Stopped;
			}
		}


		public class DriverState
		{
			public DriverState()
			{
				DrivingAction = new DrivingBehaviorEntry();
			}

			public Second dt;
			public MeterPerSquareSecond Acceleration;
			public IResponse Response;
			public int RetryCount;
			public DrivingBehaviorEntry DrivingAction;
		}
	}
}