using System;
using System.CodeDom;
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
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Driver : VectoSimulationComponent, IDriver, IDrivingCycleOutPort, IDriverDemandInPort, IDriverActions
	{
		internal DriverState CurrentState = new DriverState();

		protected IDriverDemandOutPort Next;

		protected DriverData DriverData;

		protected IDriverStrategy DriverStrategy;


		public Driver(VehicleContainer container, DriverData driverData, IDriverStrategy strategy) : base(container)
		{
			DriverData = driverData;
			DriverStrategy = strategy;
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

			var retVal = DriverStrategy.Request(absTime, ds, targetVelocity, gradient);
			//DoHandleRequest(absTime, ds, targetVelocity, gradient);

			CurrentState.Response = retVal;
			return retVal;
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			Log.Debug("==== DRIVER Request ====");
			Log.Debug("Request: absTime: {0},  dt: {1}, targetVelocity: {2}, gradient: {3}", absTime, dt, targetVelocity,
				gradient);

			var retVal = DriverStrategy.Request(absTime, dt, targetVelocity, gradient);
			//DoHandleRequest(absTime, dt, targetVelocity, gradient);

			CurrentState.Response = retVal;
			return retVal;
		}


		//protected IResponse DoHandleRequest(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		//{
		//	var currentDistance = DataBus.Distance();
		//	var nextDrivingActions = GetNextDrivingActions(currentDistance);

		//	Log.Debug(", ".Join(nextDrivingActions.Select(x => string.Format("({0}: {1})", x.ActionDistance, x.Action))));


		//	if (CurrentState.DrivingAction.Action == DefaultDriverStrategy.DrivingBehavior.Stopped && targetVelocity >= DataBus.VehicleSpeed()) {
		//		CurrentState.DrivingAction.Action = DefaultDriverStrategy.DrivingBehavior.Drive;
		//	}

		//	if (nextDrivingActions.Count > 0) {
		//		// if we exceeded the previous action (by accident), set the action anyway in case there is no 'next action'...
		//		CurrentState.DrivingAction = nextDrivingActions.LastOrDefault(x => x.ActionDistance < currentDistance) ??
		//									CurrentState.DrivingAction;

		//		var nextActions = nextDrivingActions.Where(x => x.ActionDistance >= currentDistance).ToList();
		//		var nextDrivingAction = nextActions.GetEnumerator();
		//		nextDrivingAction.MoveNext();
		//		var hasNextEntry = true;

		//		// if the current position matches the next action - set new action.
		//		if (nextDrivingAction.Current.ActionDistance <=
		//			currentDistance + Constants.SimulationSettings.DriverActionDistanceTolerance) {
		//			CurrentState.DrivingAction = nextDrivingAction.Current;
		//			hasNextEntry = nextDrivingAction.MoveNext(); // the current action has already been processed, look at next one...
		//		}

		//		// check if desired distance exceeds next action point
		//		if (hasNextEntry && nextDrivingAction.Current.ActionDistance < currentDistance + ds) {
		//			Log.Debug(
		//				"current Distance: {3} -- Simulation Distance {0} exceeds next DrivingAction at {1}, reducing interval to {2}", ds,
		//				nextDrivingAction.Current.ActionDistance, nextDrivingAction.Current.ActionDistance - currentDistance,
		//				currentDistance);
		//			return new ResponseDrivingCycleDistanceExceeded {
		//				MaxDistance = nextDrivingAction.Current.ActionDistance - currentDistance
		//			};
		//		}
		//	} else {
		//		if (targetVelocity > DataBus.VehicleSpeed()) {
		//			CurrentState.DrivingAction.Action = DefaultDriverStrategy.DrivingBehavior.Accelerating;
		//		}
		//	}
		//	Log.Debug("DrivingAction: {0}", CurrentState.DrivingAction.Action);
		//	//CurrentState.DrivingAction = nextAction;
		//	switch (CurrentState.DrivingAction.Action) {
		//		case DefaultDriverStrategy.DrivingBehavior.Accelerating:
		//			return DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
		//		case DefaultDriverStrategy.DrivingBehavior.Drive:
		//			return DrivingActionAccelerate(absTime, ds, targetVelocity, gradient);
		//		case DefaultDriverStrategy.DrivingBehavior.Coasting:
		//			return DrivingActionCoast(absTime, ds, gradient);
		//		case DefaultDriverStrategy.DrivingBehavior.Braking:
		//			return DoBreak(absTime, ds, gradient, CurrentState.DrivingAction.NextTargetSpeed);
		//	}
		//	throw new VectoSimulationException("unhandled driving action " + CurrentState.DrivingAction);
		//}

		IDataBus IDriverActions.DataBus
		{
			get { return DataBus; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="targetVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		public IResponse DrivingActionAccelerate(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			var operatingPoint = ComputeAcceleration(ds, targetVelocity);

			if (operatingPoint.SimulationInterval <= 0) {
				return new ResponseFailTimeInterval();
			}

			IResponse retVal = null;
			var response = Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);
			response.Switch().
				Case<ResponseSuccess>(r => {
					r.SimulationInterval = operatingPoint.SimulationInterval;
					retVal = r; // => return
				}).
				Case<ResponseEngineOverload>(r => {
					if (r.Delta < 0) {
						// if Delta is negative we are already below the Drag-load curve. activate breaks
						retVal = r; // => return, strategy should brake
					}
				}).
				Case<ResponseGearShift>(r => { retVal = r; }).
				Default(r => { throw new VectoException(string.Format("Unknown Response: {0}", r)); });

			if (retVal == null) {
				// unhandled response (overload, delta > 0) - we need to search for a valid operating point..				
				operatingPoint = SearchOperatingPoint(absTime, ds, gradient, operatingPoint.Acceleration, response);

				operatingPoint = LimitAccelerationByDriverModel(operatingPoint, false);
				Log.Debug("Found operating point for Drive/Accelerate. dt: {0}, acceleration: {1}",
					CurrentState.dt, CurrentState.Acceleration);

				retVal = Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);
				retVal.SimulationInterval = operatingPoint.SimulationInterval;
			}
			CurrentState.Acceleration = operatingPoint.Acceleration;
			CurrentState.dt = operatingPoint.SimulationInterval;
			CurrentState.Response = retVal;

			return retVal;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		public IResponse DrivingActionCoast(Second absTime, Meter ds, Radian gradient)
		{
			var operatingPoint = ComputeAcceleration(ds, 0.SI<MeterPerSecond>());

			if (operatingPoint.SimulationInterval <= 0) {
				return new ResponseFailTimeInterval();
			}

			CurrentState.Acceleration = operatingPoint.Acceleration;

			var response = Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient,
				dryRun: true);

			operatingPoint = SearchOperatingPoint(absTime, operatingPoint.SimulationDistance, gradient,
				operatingPoint.Acceleration, response, coasting: true);

			if (!ds.IsEqual(operatingPoint.SimulationDistance)) {
				// vehicle is at low speed, coasting would lead to stop before ds is reached.
				Log.Debug("SearchOperatingPoint reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!",
					operatingPoint.SimulationDistance, ds);
				return new ResponseDrivingCycleDistanceExceeded {
					MaxDistance = operatingPoint.SimulationDistance,
					SimulationInterval = CurrentState.dt
				};
			}

			Log.Debug("Found operating point for coasting. dt: {0}, acceleration: {1}", CurrentState.dt,
				CurrentState.Acceleration);

			operatingPoint = LimitAccelerationByDriverModel(operatingPoint, true);

			CurrentState.dt = operatingPoint.SimulationInterval;
			CurrentState.Acceleration = operatingPoint.Acceleration;

			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;

			retVal.Switch().
				Case<ResponseSuccess>(r => r.SimulationInterval = CurrentState.dt).
				Case<ResponseEngineOverload>(() => {
					/* an overload may occur due to limiting the acceleration. strategy has to handle this */
				}).
				Case<ResponseGearShift>(r => retVal = r).
				Default(() => { throw new VectoSimulationException("unhandled response from powertrain: {0}", retVal); });

			return retVal; //new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		public IResponse DrivingActionRoll(Second absTime, Meter ds, Radian gradient)
		{
			var operatingPoint = ComputeAcceleration(ds, 0.SI<MeterPerSecond>());

			if (operatingPoint.SimulationInterval <= 0) {
				return new ResponseFailTimeInterval();
			}

			CurrentState.Acceleration = operatingPoint.Acceleration;

			var response = Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient,
				dryRun: true);

			operatingPoint = SearchOperatingPoint(absTime, operatingPoint.SimulationDistance, gradient,
				operatingPoint.Acceleration, response, coasting: true);

			if (!ds.IsEqual(operatingPoint.SimulationDistance)) {
				// vehicle is at low speed, coasting would lead to stop before ds is reached.
				Log.Debug("SearchOperatingPoint reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!",
					operatingPoint.SimulationDistance, ds);
				return new ResponseDrivingCycleDistanceExceeded {
					MaxDistance = operatingPoint.SimulationDistance,
					SimulationInterval = CurrentState.dt
				};
			}

			Log.Debug("Found operating point for coasting. dt: {0}, acceleration: {1}", CurrentState.dt,
				CurrentState.Acceleration);

			operatingPoint = LimitAccelerationByDriverModel(operatingPoint, true);

			CurrentState.dt = operatingPoint.SimulationInterval;
			CurrentState.Acceleration = operatingPoint.Acceleration;

			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;

			retVal.Switch().
				Case<ResponseSuccess>(r => r.SimulationInterval = CurrentState.dt).
				Case<ResponseEngineOverload>(() => {
					/* an overload may occur due to limiting the acceleration. strategy has to handle this */
				}).
				Default(() => { throw new VectoSimulationException("unhandled response from powertrain: {0}", retVal); });

			return retVal; //new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="gradient"></param>
		/// <param name="nextTargetSpeed"></param>
		/// <returns></returns>
		public IResponse DrivingActionBrake(Second absTime, Meter ds, Radian gradient, MeterPerSecond nextTargetSpeed)
		{
			var operatingPoint = ComputeAcceleration(ds, nextTargetSpeed);

			// todo: still required?
			if (operatingPoint.SimulationInterval <= 0) {
				return new ResponseFailTimeInterval();
			}

			var response = Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient, true);

			var dryRun = response as ResponseDryRun;
			if (dryRun == null) {
				throw new VectoSimulationException("Expected DryRunResponse after Dry-Run Request!");
			}

			if (dryRun.DeltaDragLoad > 0) {
				throw new VectoSimulationException("Braking not required, above drag load! Use engine brake!");
			}

			operatingPoint = SearchBreakingPower(absTime, operatingPoint.SimulationDistance, gradient,
				operatingPoint.Acceleration, dryRun);

			if (!ds.IsEqual(operatingPoint.SimulationDistance)) {
				Log.Debug(
					"SearchOperatingPoint Breaking reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!",
					operatingPoint.SimulationDistance, ds);
				return new ResponseDrivingCycleDistanceExceeded {
					MaxDistance = operatingPoint.SimulationDistance,
					SimulationInterval = operatingPoint.SimulationInterval
				};
			}

			Log.Debug("Found operating point for breaking. dt: {0}, acceleration: {1}", operatingPoint.SimulationInterval,
				operatingPoint.Acceleration);

			var retVal = Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);

			CurrentState.Acceleration = operatingPoint.Acceleration;
			CurrentState.dt = operatingPoint.SimulationInterval;
			CurrentState.Response = retVal;

			retVal.SimulationInterval = CurrentState.dt;
			return retVal;
		}


		// ================================================

		/// <summary>
		/// 
		/// </summary>
		/// <param name="operatingPoint"></param>
		/// <param name="limitByLookahead"></param>
		/// <returns></returns>
		private OperatingPoint LimitAccelerationByDriverModel(OperatingPoint operatingPoint, bool limitByLookahead)
		{
			// todo: limit by driver model!

			if (limitByLookahead && operatingPoint.Acceleration < DeclarationData.Driver.LookAhead.Deceleration) {
				Log.Debug("Limiting coasting deceleration from {0} to {1}", CurrentState.Acceleration,
					DeclarationData.Driver.LookAhead.Deceleration);
				operatingPoint.Acceleration = DeclarationData.Driver.LookAhead.Deceleration;
				operatingPoint.SimulationInterval =
					ComputeTimeInterval(CurrentState.Acceleration, operatingPoint.SimulationDistance).SimulationInterval;
				Log.Debug("Changed dt due to limited coasting deceleration. dt: {0}", CurrentState.dt);
			}
			return operatingPoint;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="gradient"></param>
		/// <param name="acceleration"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		private OperatingPoint SearchBreakingPower(Second absTime, Meter ds, Radian gradient,
			MeterPerSquareSecond acceleration, IResponse response)
		{
			var debug = new List<dynamic>(); // only used while testing

			var searchInterval = Constants.SimulationSettings.BreakingPowerInitialSearchInterval;

			var operatingPoint = new OperatingPoint() { SimulationDistance = ds, Acceleration = acceleration };
			var origDelta = DataBus.ClutchState() == ClutchState.ClutchClosed
				? response.DeltaDragLoad
				: response.GearboxPowerRequest;

			var breakingPower = origDelta.Abs();


			// double the searchInterval until a good interval was found
			var intervalFactor = 2.0;
			var retryCount = 0;

			do {
				var delta = DataBus.ClutchState() == ClutchState.ClutchClosed
					? response.DeltaDragLoad
					: response.GearboxPowerRequest;

				debug.Add(new { breakingPower, searchInterval, delta });

				// check if a correct searchInterval was found (when the delta changed signs, we stepped through the 0-point)
				// from then on the searchInterval can be bisected.
				if (origDelta.Sign() != delta.Sign()) {
					intervalFactor = 0.5;
				}

				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					Log.Debug("found operating point in {0} iterations, delta: {1}", debug.Count, delta);
					return operatingPoint;
				}

				breakingPower += searchInterval * -delta.Sign();
				searchInterval *= intervalFactor;

				operatingPoint = ComputeTimeInterval(operatingPoint.Acceleration, ds);
				DataBus.BreakPower = breakingPower;
				response =
					(ResponseDryRun)
						Next.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient, true);
			} while (retryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			Log.Warn("Exceeded max iterations when searching for operating point!");
			Log.Warn("exceeded: {0} ... {1}", ", ".Join(debug.Take(5)), ", ".Join(debug.Slice(-6)));
			Log.Error("Failed to find operating point for breaking!");
			throw new VectoSimulationException("Failed to find operating point for breaking!");
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
		/// <param name="acceleration"></param>
		/// <param name="response">response of the former request that resulted in an overload response (or a dry-run response)</param>
		/// <param name="coasting">if true approach the drag-load curve, otherwise full-load curve</param>
		/// <returns></returns>
		protected OperatingPoint SearchOperatingPoint(Second absTime, Meter ds, Radian gradient,
			MeterPerSquareSecond acceleration, IResponse response, bool coasting = false)
		{
			// remove accelerating param
			var debug = new List<dynamic>();
			var searchInterval = Constants.SimulationSettings.OperatingPointInitialSearchIntervalAccelerating;

			// double the searchInterval until a good interval was found
			var intervalFactor = 2.0;

			var retVal = new OperatingPoint() { Acceleration = acceleration, SimulationDistance = ds };

			var actionRoll = DataBus.ClutchState() == ClutchState.ClutchOpened;

			var delta = 0.SI<Watt>();
			Watt origDelta = null;
			var retryCount = 0;

			do {
				if (actionRoll) {
					response.Switch().
						Case<ResponseEngineOverload>().
						Case <
				} else {
					response.Switch().
						Case<ResponseEngineOverload>(r => delta = r.Delta).
						Case<ResponseDryRun>(r => delta = coasting ? r.DeltaDragLoad : r.DeltaFullLoad).
						Default(r => { throw new VectoSimulationException(string.Format("Unknown response type. {0}", r)); });
				}
				debug.Add(new { delta, acceleration = retVal.Acceleration, searchInterval, intervalFactor });

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
					return retVal;
				}

				searchInterval *= intervalFactor;
				retVal.Acceleration += searchInterval * -delta.Sign();


				// TODO: move to driving mode
				// check for minimum acceleration, add some safety margin due to search
				if (!coasting && retVal.Acceleration.Abs() < Constants.SimulationSettings.MinimumAcceleration.Value() / 5.0 /* &&
				 searchInterval.Abs() < Constants.SimulationSettings.MinimumAcceleration / 20.0 */) {
					throw new VectoSimulationException("Could not achieve minimum acceleration");
				}

				var tmp = ComputeTimeInterval(retVal.Acceleration, ds);
				retVal.SimulationInterval = tmp.SimulationInterval;

				response = Next.Request(absTime, retVal.SimulationInterval, retVal.Acceleration, gradient, true);
			} while (retryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			Log.Warn("Exceeded max iterations when searching for operating point!");
			Log.Warn("acceleration: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.acceleration)),
				", ".Join(debug.Slice(-6).Select(x => x.acceleration)));
			Log.Warn("exceeded: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.delta)),
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
		public OperatingPoint ComputeAcceleration(Meter ds, MeterPerSecond targetVelocity)
		{
			var currentSpeed = DataBus.VehicleSpeed();
			var retVal = new OperatingPoint() { SimulationDistance = ds };

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

			retVal.Acceleration = requiredAcceleration;
			var operatingPoint = ComputeTimeInterval(retVal.Acceleration, ds);

			if (ds.IsEqual(operatingPoint.SimulationDistance)) {
				return retVal;
			}

			// this case should not happen, acceleration has been computed such that the target speed
			// can be reached within ds.
			Log.Error(
				"Unexpected Condition: Distance has been adjusted from {0} to {1}, currentVelocity: {2} acceleration: {3}, targetVelocity: {4}",
				operatingPoint.SimulationDistance, ds, currentSpeed, CurrentState.Acceleration, targetVelocity);
			throw new VectoSimulationException("Simulation distance unexpectedly adjusted!");
		}


		/// <summary>
		/// computes the distance required to decelerate from the current velocity to the given target velocity considering
		/// the drivers acceleration/deceleration curve.
		/// </summary>
		/// <param name="targetSpeed"></param>
		/// <returns></returns>
		public Meter ComputeDecelerationDistance(MeterPerSecond targetSpeed)
		{
			return DriverData.AccelerationCurve.ComputeAccelerationDistance(DataBus.VehicleSpeed(), targetSpeed);
		}


		/// <summary>
		/// Computes the time interval for driving the given distance ds with the vehicle's current speed and the given acceleration.
		/// If the distance ds can not be reached (i.e., the vehicle would halt before ds is reached) then the distance parameter is adjusted.
		/// The computed time interval is returned via the out parameter dt
		/// </summary>
		/// <param name="acceleration"></param>
		/// <param name="ds"></param>
		private OperatingPoint ComputeTimeInterval(MeterPerSquareSecond acceleration, Meter ds)
		{
			if (!(ds > 0)) {
				throw new VectoSimulationException("distance has to be greater than 0!");
			}
			var currentSpeed = DataBus.VehicleSpeed();
			var retVal = new OperatingPoint() { Acceleration = acceleration, SimulationDistance = ds };
			if (acceleration.IsEqual(0)) {
				if (currentSpeed > 0) {
					retVal.SimulationInterval = ds / currentSpeed;
					return retVal;
				}
				Log.Error("vehicle speed is {0}, acceleration is {1}", currentSpeed, acceleration);
				throw new VectoSimulationException("vehicle speed has to be > 0 if acceleration = 0");
			}

			// we need to accelerate / decelerate. solve quadratic equation...
			// ds = acceleration / 2 * dt^2 + currentSpeed * dt   => solve for dt
			var solutions = VectoMath.QuadraticEquationSolver(acceleration.Value() / 2.0, currentSpeed.Value(),
				-ds.Value());

			if (solutions.Count == 0) {
				// no real-valued solutions, required distance can not be reached (vehicle stopped), adapt ds...
				retVal.SimulationInterval = -currentSpeed / acceleration;
				var stopDistance = currentSpeed * retVal.SimulationInterval +
									acceleration / 2 * retVal.SimulationInterval * retVal.SimulationInterval;
				if (stopDistance > ds) {
					// just to cover everything - does not happen...
					Log.Warn(
						"Could not find solution for computing required time interval to drive distance {0}. currentSpeed: {1}, acceleration: {2}",
						ds, currentSpeed, acceleration);
					throw new VectoSimulationException("Could not find solution");
				}
				Log.Info(
					"Adjusted distance when computing time interval: currentSpeed: {0}, acceleration: {1}, distance: {2} -> {3}, timeInterval: {4}",
					currentSpeed, acceleration, retVal.SimulationDistance, stopDistance, retVal.SimulationInterval);
				retVal.SimulationDistance = stopDistance;
				return retVal;
			}
			solutions = solutions.Where(x => x >= 0).ToList();
			// if there are 2 positive solutions (i.e. when decelerating), take the smaller time interval
			// (the second solution means that you reach negative speed 
			retVal.SimulationInterval = solutions.Min().SI<Second>();
			return retVal;
		}

		/// <summary>
		/// simulate a certain time interval where the vehicle is stopped.
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="dt"></param>
		/// <param name="targetVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		public IResponse DrivingActionHalt(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			if (!targetVelocity.IsEqual(0) || !DataBus.VehicleSpeed().IsEqual(0)) {
				throw new NotImplementedException("TargetVelocity or VehicleVelocity is not zero!");
			}
			var oldGear = DataBus.Gear;
			DataBus.Gear = 0;
			DataBus.BreakPower = double.PositiveInfinity.SI<Watt>();
			var retVal = Next.Request(absTime, dt, 0.SI<MeterPerSquareSecond>(), gradient);
			retVal.SimulationInterval = dt;
			DataBus.Gear = oldGear;
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
			//CurrentState.RetryCount = 0;
			CurrentState.Response = null;

			//if (CurrentState.DrivingAction.NextTargetSpeed != null &&
			//	DataBus.VehicleSpeed().IsEqual(CurrentState.DrivingAction.NextTargetSpeed)) {
			//	Log.Debug("reached target Speed {0} - set Driving action to {1}", CurrentState.DrivingAction.NextTargetSpeed,
			//		DefaultDriverStrategy.DrivingBehavior.Drive);
			//	CurrentState.DrivingAction.Action = DefaultDriverStrategy.DrivingBehavior.Drive;
			//}
			//if (DataBus.VehicleSpeed().IsEqual(0)) {
			//	Log.Debug("vehicle stopped {0}", DataBus.VehicleSpeed());
			//	CurrentState.DrivingAction.Action = DefaultDriverStrategy.DrivingBehavior.Stopped;
			//}
		}


		public class DriverState
		{
			public Second dt;
			public MeterPerSquareSecond Acceleration;
			public IResponse Response;
			//public int RetryCount;
		}

		public struct OperatingPoint
		{
			public MeterPerSquareSecond Acceleration;
			public Meter SimulationDistance;
			public Second SimulationInterval;
		}
	}
}