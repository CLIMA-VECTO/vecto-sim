using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
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
			Log.DebugFormat(
				"Request: absTime: {0},  ds: {1}, targetVelocity: {2}, gradient: {3} | distance: {4}, velocity: {5}", absTime, ds,
				targetVelocity,
				gradient, DataBus.Distance(), DataBus.VehicleSpeed());

			var retVal = DoHandleRequest(absTime, ds, targetVelocity, gradient);

			CurrentState.Response = retVal;

			switch (retVal.ResponseType) {
				case ResponseType.FailOverload:

					break;
			}
			return retVal;
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			Log.Debug("==== DRIVER Request ====");
			Log.DebugFormat("Request: absTime: {0},  dt: {1}, targetVelocity: {2}, gradient: {3}", absTime, dt, targetVelocity
				, gradient);
			var retVal = DoHandleRequest(absTime, dt, targetVelocity, gradient);

			CurrentState.Response = retVal;

			//switch (retVal.ResponseType) {}
			return retVal;
		}


		protected IResponse DoHandleRequest(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			var currentDistance = DataBus.Distance();
			var nextDrivingActions = GetNextDrivingActions(currentDistance);

			if (CurrentState.DrivingAction.Action == DrivingBehavior.Stopped && targetVelocity >= DataBus.VehicleSpeed()) {
				CurrentState.DrivingAction.Action = DrivingBehavior.Drive;
			}

			if (nextDrivingActions.Count > 0) {
				// if we exceeded the previous action (by accident), set the action anyway in case there is no 'next action'...
				var previousActionss = nextDrivingActions.Where(x => x.Key < currentDistance).ToList();
				if (previousActionss.Count > 0) {
					CurrentState.DrivingAction = previousActionss.Last().Value;
				}

				var nextActions = nextDrivingActions.Where(x => x.Key >= currentDistance).ToList();
				var nextDrivingAction = nextActions.GetEnumerator();
				nextDrivingAction.MoveNext();
				var hasNextEntry = true;

				// if the current position matches the next action - set new action.
				if (nextDrivingAction.Current.Key.IsEqual(currentDistance,
					Constants.SimulationSettings.DriverActionDistanceTolerance.Value())) {
					CurrentState.DrivingAction = nextDrivingAction.Current.Value;
					hasNextEntry = nextDrivingAction.MoveNext(); // the current action has already been processed, look at next one...
				}
				// check if desired distance exeeds next acttion point
				if (hasNextEntry && nextDrivingAction.Current.Key < currentDistance + ds) {
					Log.DebugFormat(
						"current Distance: {3} -- Simulation Distance {0} exceeds next DrivingAction at {1}, reducing interval to {2}", ds,
						nextDrivingAction.Current.Key, nextDrivingAction.Current.Key - currentDistance, currentDistance);
					return new ResponseDrivingCycleDistanceExceeded() { MaxDistance = nextDrivingAction.Current.Key - currentDistance };
				}
			} else {
				if (targetVelocity > DataBus.VehicleSpeed()) {
					CurrentState.DrivingAction.Action = DrivingBehavior.Accelerating;
				}
			}
			Log.DebugFormat("DrivingAction: {0}", CurrentState.DrivingAction.Action);
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

			//if (dryRun.EngineDeltaDragLoad > 0) {
			//	throw new VectoSimulationException("No breaking necessary, still above full drag load!");
			//}

			var newDs = ds;
			var success = SearchBreakingPower(absTime, ref newDs, gradient, dryRun, true);

			if (!success) {
				Log.ErrorFormat("Failed to find operating point for breaking!");
				throw new VectoSimulationException("Failed to find operating point for breaking!");
			}

			if (!ds.IsEqual(newDs)) {
				Log.DebugFormat(
					"SearchOperatingPoint Breaking reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!", newDs,
					ds);
				return new ResponseDrivingCycleDistanceExceeded() { MaxDistance = newDs, SimulationInterval = CurrentState.dt };
			}

			Log.DebugFormat("Found operating point for breaking. dt: {0}, acceleration: {1}", CurrentState.dt,
				CurrentState.Acceleration);
			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;
			switch (retVal.ResponseType) {
				case ResponseType.Success:
					retVal.SimulationInterval = CurrentState.dt;
					return retVal;
			}
			Log.DebugFormat("unhandled response from powertrain: {0}", retVal);
			return retVal; //new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		private bool SearchBreakingPower(Second absTime, ref Meter ds, Radian gradient, ResponseDryRun response, bool coasting)
		{
			var exceeded = new List<Watt>(); // only used while testing
			var breakingPower = VectoMath.Abs(response.EngineDeltaDragLoad);
			if (DataBus.ClutchState() != ClutchState.ClutchClosed) {
				breakingPower = VectoMath.Abs(response.AxlegearPowerRequest);
			}
			var searchInterval = breakingPower;
			var originalDs = ds;

			do {
				ds = originalDs;
				var delta = DataBus.ClutchState() == ClutchState.ClutchClosed
					? -response.EngineDeltaDragLoad
					: -response.AxlegearPowerRequest;

				exceeded.Add(delta);
				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					Log.DebugFormat("found operating point in {0} iterations, delta: {1}", exceeded.Count, delta);
					return true;
				}
				if (delta > 0) {
					breakingPower -= searchInterval;
				} else {
					breakingPower += searchInterval;
				}

				searchInterval /= 2.0;
				ComputeTimeInterval(CurrentState.Acceleration, ref ds, out CurrentState.dt);
				DataBus.BreakPower = breakingPower;
				response = (ResponseDryRun)Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			Log.DebugFormat("Exceeded max iterations when searching for operating point!");
			//Log.DebugFormat("acceleration: {0} ... {1}", string.Join(", ", acceleration.Take(5)),
			//	string.Join(", ", acceleration.GetRange(acceleration.Count - 6, 5)));
			Log.DebugFormat("exceeded: {0} ... {1}", string.Join(", ", exceeded.Take(5)),
				string.Join(", ", exceeded.GetRange(exceeded.Count - 6, 5)));

			return false;
		}

		private IResponse DriveOrAccelerate(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			ComputeAcceleration(ref ds, targetVelocity);
			if (CurrentState.dt.IsEqual(0)) {
				return new ResponseFailTimeInterval();
			}

			do {
				var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
				switch (retVal.ResponseType) {
					case ResponseType.Success:
						retVal.SimulationInterval = CurrentState.dt;
						return retVal;
					case ResponseType.FailOverload:
						SearchOperatingPoint(absTime, ref ds, gradient, retVal);
						Log.DebugFormat("Found operating point for Drive/Accelerate. dt: {0}, acceleration: {1}", CurrentState.dt,
							CurrentState.Acceleration);

						break;
				}
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			return new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		protected List<KeyValuePair<Meter, DrivingBehaviorEntry>> GetNextDrivingActions(Meter minDistance)
		{
			var currentSpeed = DataBus.VehicleSpeed();
			// distance until halt: s = - v * v / (2 * a)
			var lookaheadDistance =
				(-currentSpeed * currentSpeed / (2 * DeclarationData.Driver.LookAhead.Deceleration)).Cast<Meter>();
			var lookaheadData = DataBus.LookAhead(1.2 * lookaheadDistance);

			Log.DebugFormat("Lookahead distance: {0} @ current speed {1}", lookaheadDistance, currentSpeed);
			var nextActions = new List<KeyValuePair<Meter, DrivingBehaviorEntry>>();
			for (var i = 0; i < lookaheadData.Count; i++) {
				var entry = lookaheadData[i];
				if (entry.VehicleTargetSpeed < currentSpeed) {
					var breakingDistance = ComputeDecelerationDistance(entry.VehicleTargetSpeed);
					Log.DebugFormat("distance to decelerate from {0} to {1}: {2}", currentSpeed, entry.VehicleTargetSpeed,
						breakingDistance);
					Log.DebugFormat("adding 'Braking' starting at distance {0}", entry.Distance - breakingDistance);
					nextActions.Add(new KeyValuePair<Meter, DrivingBehaviorEntry>(entry.Distance - breakingDistance,
						new DrivingBehaviorEntry() {
							Action = DrivingBehavior.Breaking,
							ActionDistance = entry.Distance - breakingDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = entry.VehicleTargetSpeed
						}));
					Log.DebugFormat("adding 'Coasting' starting at distance {0}", entry.Distance - lookaheadDistance);
					nextActions.Add(new KeyValuePair<Meter, DrivingBehaviorEntry>(entry.Distance - lookaheadDistance,
						new DrivingBehaviorEntry() {
							Action = DrivingBehavior.Coasting,
							ActionDistance = entry.Distance - lookaheadDistance,
							TriggerDistance = entry.Distance,
							NextTargetSpeed = entry.VehicleTargetSpeed
						}));
				}
				if (entry.VehicleTargetSpeed > currentSpeed) {
					nextActions.Add(new KeyValuePair<Meter, DrivingBehaviorEntry>(entry.Distance, new DrivingBehaviorEntry() {
						Action = DrivingBehavior.Accelerating,
						NextTargetSpeed = entry.VehicleTargetSpeed,
						TriggerDistance = entry.Distance,
						ActionDistance = entry.Distance
					}));
				}
			}

			nextActions.Sort((x, y) => x.Key.CompareTo(y.Key));

			return nextActions;
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

			var response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);

			var newDs = ds;
			var success = SearchOperatingPoint(absTime, ref newDs, gradient, response, true);

			if (!success) {
				Log.ErrorFormat("Failed to find operating point for coasting!");
				throw new VectoSimulationException("Failed to find operating point for coasting!");
			}

			if (!ds.IsEqual(newDs)) {
				Log.DebugFormat(
					"SearchOperatingPoint reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!", newDs, ds);
				return new ResponseDrivingCycleDistanceExceeded() { MaxDistance = newDs, SimulationInterval = CurrentState.dt };
			}

			Log.DebugFormat("Found operating point for coasting. dt: {0}, acceleration: {1}", CurrentState.dt,
				CurrentState.Acceleration);
			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;
			switch (retVal.ResponseType) {
				case ResponseType.Success:
					retVal.SimulationInterval = CurrentState.dt;
					return retVal;
			}
			Log.DebugFormat("unhandled response from powertrain: {0}", retVal);
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
		/// <returns></returns>
		private bool SearchOperatingPoint(Second absTime, ref Meter ds, Radian gradient,
			IResponse response, bool coasting = false)
		{
			var exceeded = new List<Watt>(); // only used while testing
			var acceleration = new List<double>(); // only used while testing
			var searchInterval = CurrentState.Acceleration.Value() / 2.0;
			Meter originalDs = ds;

			do {
				var delta = 0.0.SI<Watt>();
				ds = originalDs;
				switch (response.ResponseType) {
					case ResponseType.FailOverload:
						delta = ((ResponseFailOverload)response).Delta;
						break;
					case ResponseType.DryRun:
						delta = coasting
							? -((ResponseDryRun)response).EngineDeltaDragLoad
							: ((ResponseDryRun)response).EngineDeltaFullLoad;
						break;
					default:
						throw new VectoSimulationException("Unknown response type");
				}

				exceeded.Add(delta);
				acceleration.Add(CurrentState.Acceleration.Value());
				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					Log.DebugFormat("found operating point in {0} iterations. Engine Power req: {2},  delta: {1}", exceeded.Count,
						delta, response.EnginePowerRequest);
					return true;
				}
				if (delta > 0) {
					CurrentState.Acceleration -= searchInterval;
				} else {
					CurrentState.Acceleration += searchInterval;
				}
				// check for minimum acceleration, add some safety margin due to search
				if (Math.Abs(CurrentState.Acceleration.Value()) < Constants.SimulationSettings.MinimumAcceleration.Value() / 5.0 &&
					Math.Abs(searchInterval) < Constants.SimulationSettings.MinimumAcceleration.Value() / 20.0) {
					throw new VectoSimulationException("Could not achieve minimum acceleration");
				}
				searchInterval /= 2.0;
				ComputeTimeInterval(CurrentState.Acceleration, ref ds, out CurrentState.dt);
				response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			Log.DebugFormat("Exceeded max iterations when searching for operating point!");
			Log.DebugFormat("acceleration: {0} ... {1}", string.Join(", ", acceleration.Take(5)),
				string.Join(", ", acceleration.GetRange(acceleration.Count - 6, 5)));
			Log.DebugFormat("exceeded: {0} ... {1}", string.Join(", ", exceeded.Take(5)),
				string.Join(", ", exceeded.GetRange(exceeded.Count - 6, 5)));

			return false;
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
			ComputeTimeInterval(CurrentState.Acceleration, ref ds, out CurrentState.dt);
			if (!ds.IsEqual(tmpDs)) {
				Log.ErrorFormat(
					"Unexpected Condition: Distance has been adjusted from {0} to {1}, currentVelocity: {2} acceleration: {3}, targetVelocity: {4}",
					tmpDs, ds, currentSpeed, CurrentState.Acceleration, targetVelocity);
				throw new VectoSimulationException("Simulation distance unexpectedly adjusted!");
			}
		}

		/// <summary>
		/// compute the time interval for driving the given distance ds with the vehicle's current speed and the given acceleration
		/// if the given distance ds can not be reached (i.e., the vehicle would halt before ds is reached) then the distance parameter is adjusted
		/// the computed time interval is returned via the out parameter dt
		/// </summary>
		/// <param name="acceleration"></param>
		/// <param name="ds"></param>
		/// <param name="dt"></param>
		private void ComputeTimeInterval(MeterPerSquareSecond acceleration, ref Meter ds, out Second dt)
		{
			if (!(ds > 0)) {
				throw new VectoSimulationException("distance has to be greater than 0!");
			}
			var currentSpeed = DataBus.VehicleSpeed();

			if (acceleration.IsEqual(0)) {
				if (!(currentSpeed > 0)) {
					Log.ErrorFormat("vehicle speed is {0}, acceleration is {1}", currentSpeed, acceleration);
					throw new VectoSimulationException("vehicle speed has to be > 0 if acceleration = 0");
				}
				dt = (ds / currentSpeed).Cast<Second>();
				return;
			}

			// we need to accelerate / decelerate. solve quadratic equation...
			// ds = acceleration / 2 * dt^2 + currentSpeed * dt   => solve for dt
			var solutions = VectoMath.QuadraticEquationSolver(acceleration.Value() / 2.0, currentSpeed.Value(),
				-ds.Value());

			if (solutions.Count == 0) {
				// no real-valued solutions, required distance can not be reached (vehicle stopped), adapt ds...
				dt = -(currentSpeed / acceleration).Cast<Second>();
				var stopDistance = (currentSpeed * dt + acceleration / 2 * dt * dt).Cast<Meter>();
				if (stopDistance > ds) {
					Log.WarnFormat(
						"Could not find solution for computing required time interval to drive distance {0}. currentSpeed: {1}, acceleration: {2}",
						ds, currentSpeed, acceleration);
					throw new VectoSimulationException("Could not find solution");
				}
				Log.InfoFormat(
					"Adjusted distance when computing time interval: currentSpeed: {0}, acceleration: {1}, distance: {2} -> {3}, timeInterval: {4}",
					currentSpeed, acceleration, ds, stopDistance, dt);
				ds = stopDistance;
				return;
			}
			solutions = solutions.Where(x => x >= 0).ToList();
			// if there are 2 positive solutions (i.e. when decelerating), take the smaller time interval
			// (the second solution means that you reach negative speed 
			solutions.Sort();
			dt = solutions.First().SI<Second>();
		}


		protected IResponse DoHandleRequest(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			if (!targetVelocity.IsEqual(0) || !DataBus.VehicleSpeed().IsEqual(0)) {
				throw new NotImplementedException("TargetVelocity or VehicleVelocity is not zero!");
			}
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
			if (CurrentState.Response.ResponseType != ResponseType.Success) {
				throw new VectoSimulationException("Previois request did not succeed!");
			}
			CurrentState.RetryCount = 0;
			CurrentState.Response = null;

			if (CurrentState.DrivingAction.NextTargetSpeed != null &&
				DataBus.VehicleSpeed().IsEqual(CurrentState.DrivingAction.NextTargetSpeed)) {
				Log.DebugFormat("reached target Speed {0} - set Driving action to {1}", CurrentState.DrivingAction.NextTargetSpeed,
					DrivingBehavior.Drive);
				CurrentState.DrivingAction.Action = DrivingBehavior.Drive;
			}
			if (DataBus.VehicleSpeed().IsEqual(0)) {
				Log.DebugFormat("vehicle stopped {0} - set Driving action to {1}", DataBus.VehicleSpeed(), DrivingBehavior.Stopped);
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