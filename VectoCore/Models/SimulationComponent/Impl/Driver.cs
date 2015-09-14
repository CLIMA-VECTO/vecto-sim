using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NLog;
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

		protected IDriverDemandOutPort NextComponent;

		protected DriverData DriverData;

		protected IDriverStrategy DriverStrategy;

		public MeterPerSquareSecond LookaheadDeceleration { get; protected set; }

		public Driver(VehicleContainer container, DriverData driverData, IDriverStrategy strategy) : base(container)
		{
			DriverData = driverData;
			DriverStrategy = strategy;
			strategy.Driver = this;

			LookaheadDeceleration = DeclarationData.Driver.LookAhead.Deceleration;
		}

		public IDriverDemandInPort InPort()
		{
			return this;
		}

		public void Connect(IDriverDemandOutPort other)
		{
			NextComponent = other;
		}


		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			LookaheadDeceleration = DriverData.AccelerationCurve.MinDeceleration();
			return NextComponent.Initialize(vehicleSpeed, roadGradient);
		}


		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			Log.Debug("==== DRIVER Request ====");
			Log.Debug(
				"Request: absTime: {0},  ds: {1}, targetVelocity: {2}, gradient: {3} | distance: {4}, velocity: {5}", absTime, ds,
				targetVelocity, gradient, DataBus.Distance, DataBus.VehicleSpeed);

			var retVal = DriverStrategy.Request(absTime, ds, targetVelocity, gradient);
			//DoHandleRequest(absTime, ds, targetVelocity, gradient);

			CurrentState.Response = retVal;
			retVal.SimulationInterval = CurrentState.dt;

			return retVal;
		}


		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			Log.Debug("==== DRIVER Request ====");
			Log.Debug("Request: absTime: {0},  dt: {1}, targetVelocity: {2}, gradient: {3} | distance: {4}, velocity: {5}",
				absTime, dt, targetVelocity, gradient, DataBus.Distance, DataBus.VehicleSpeed);

			var retVal = DriverStrategy.Request(absTime, dt, targetVelocity, gradient);

			CurrentState.Response = retVal;
			retVal.SimulationInterval = CurrentState.dt;

			return retVal;
		}

		IDataBus IDriverActions.DataBus
		{
			get { return DataBus; }
		}

		/// <summary>
		/// see documentation of IDriverActions
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="targetVelocity"></param>
		/// <param name="gradient"></param>
		/// <param name="previousResponse"></param>
		/// <returns></returns>
		public IResponse DrivingActionAccelerate(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient,
			IResponse previousResponse = null)
		{
			Log.Debug("DrivingAction Accelerate");
			var operatingPoint = ComputeAcceleration(ds, targetVelocity);

			IResponse retVal = null;
			var response = previousResponse ??
							NextComponent.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);

			response.Switch().
				Case<ResponseSuccess>(r => {
					retVal = r; // => return
				}).
				Case<ResponseOverload>(). // do nothing, searchOperatingPoint is called later on
				Case<ResponseUnderload>(r => {
					// Delta is negative we are already below the Drag-load curve. activate breaks
					retVal = r; // => return, strategy should brake
				}).
				Case<ResponseGearShift>(r => {
					retVal = r;
				}).
				Default(r => {
					throw new UnexpectedResponseException("DrivingAction Accelerate.", r);
				});

			if (retVal == null) {
				// unhandled response (overload, delta > 0) - we need to search for a valid operating point..				
				operatingPoint = SearchOperatingPoint(absTime, ds, gradient, operatingPoint.Acceleration, response);

				operatingPoint = LimitAccelerationByDriverModel(operatingPoint, LimitationMode.LimitDecelerationDriver);
				Log.Debug("Found operating point for Drive/Accelerate. dt: {0}, acceleration: {1}",
					CurrentState.dt, CurrentState.Acceleration);

				retVal = NextComponent.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);
				retVal.Switch().
					Case<ResponseUnderload>().
					Case<ResponseGearShift>().
					Case<ResponseSuccess>().
					Default(r => {
						throw new UnexpectedResponseException("DrivingAction Accelerate after SearchOperatingPoint.", r);
					});
			}
			CurrentState.Acceleration = operatingPoint.Acceleration;
			CurrentState.dt = operatingPoint.SimulationInterval;
			CurrentState.Response = retVal;

			return retVal;
		}

		/// <summary>
		/// see documentation of IDriverActions
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="maxVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		public IResponse DrivingActionCoast(Second absTime, Meter ds, MeterPerSecond maxVelocity, Radian gradient)
		{
			Log.Debug("DrivingAction Coast");

			return CoastOrRollAction(absTime, ds, maxVelocity, gradient, false);
		}

		/// <summary>
		/// see documentation of IDriverActions
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="maxVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		public IResponse DrivingActionRoll(Second absTime, Meter ds, MeterPerSecond maxVelocity, Radian gradient)
		{
			Log.Debug("DrivingAction Roll");

			var retVal = CoastOrRollAction(absTime, ds, maxVelocity, gradient, true);
			retVal.Switch().
				Case<ResponseGearShift>(() => {
					throw new UnexpectedResponseException("DrivingAction Roll: Gearshift during Roll action.", retVal);
				});
			return retVal;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="maxVelocity"></param>
		/// <param name="gradient"></param>
		/// <param name="rollAction"></param>
		/// <returns>
		/// * ResponseSuccess
		/// * ResponseDrivingCycleDistanceExceeded: vehicle is at low speed, coasting would lead to stop before ds is reached.
		/// * ResponseSpeedLimitExceeded: vehicle accelerates during coasting which would lead to exceeding the given maxVelocity (e.g., driving downhill, engine's drag load is not sufficient)
		/// * ResponseUnderload: engine's operating point is below drag curve (vehicle accelerates more than driver model allows; engine's drag load is not sufficient for limited acceleration
		/// * ResponseGearShift: gearbox needs to shift gears, vehicle can not accelerate (traction interruption)
		/// * ResponseFailTimeInterval: 
		/// </returns>
		protected IResponse CoastOrRollAction(Second absTime, Meter ds, MeterPerSecond maxVelocity, Radian gradient,
			bool rollAction)
		{
			var operatingPoint = ComputeAcceleration(ds, DataBus.VehicleSpeed);

			var response = NextComponent.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration,
				gradient, true);

			//if (response is ResponseFailTimeInterval) {
			//	return response;
			//}

			operatingPoint = SearchOperatingPoint(absTime, operatingPoint.SimulationDistance, gradient,
				operatingPoint.Acceleration, response, coasting: true);

			if (!ds.IsEqual(operatingPoint.SimulationDistance)) {
				// vehicle is at low speed, coasting would lead to stop before ds is reached.
				Log.Debug("SearchOperatingPoint reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!",
					operatingPoint.SimulationDistance, ds);
				return new ResponseDrivingCycleDistanceExceeded {
					Source = this,
					MaxDistance = operatingPoint.SimulationDistance,
				};
			}

			Log.Debug("Found operating point for coasting. dt: {0}, acceleration: {1}", operatingPoint.SimulationInterval,
				operatingPoint.Acceleration);

			operatingPoint = LimitAccelerationByDriverModel(operatingPoint,
				rollAction ? LimitationMode.NoLimitation : LimitationMode.LimitDecelerationLookahead);

			CurrentState.Acceleration = operatingPoint.Acceleration;
			CurrentState.dt = operatingPoint.SimulationInterval;

			// compute speed at the end of the simulation interval. if it exceeds the limit -> return
			var v2 = DataBus.VehicleSpeed + operatingPoint.Acceleration * operatingPoint.SimulationInterval;
			if (v2 > maxVelocity) {
				return new ResponseSpeedLimitExceeded() { Source = this };
			}

			var retVal = NextComponent.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;
			retVal.SimulationInterval = CurrentState.dt;

			retVal.Switch().
				Case<ResponseSuccess>().
				Case<ResponseUnderload>(). // driver limits acceleration, operating point may be below engine's 
				//drag load resp. below 0
				//Case<ResponseOverload>(). // driver limits acceleration, operating point may be above 0 (GBX), use brakes
				Case<ResponseGearShift>().
				Case<ResponseFailTimeInterval>(r => {
					retVal = new ResponseDrivingCycleDistanceExceeded() {
						Source = this,
						MaxDistance = DataBus.VehicleSpeed * r.DeltaT + CurrentState.Acceleration / 2 * r.DeltaT * r.DeltaT
					};
				}).
				Default(() => {
					throw new UnexpectedResponseException("CoastOrRoll Action: unhandled response from powertrain.", retVal);
				});
			return retVal;
		}


		public IResponse DrivingActionBrake(Second absTime, Meter ds, MeterPerSecond nextTargetSpeed, Radian gradient,
			IResponse previousResponse = null)
		{
			Log.Debug("DrivingAction Brake");

			IResponse retVal = null;

			var operatingPoint = ComputeAcceleration(ds, nextTargetSpeed);

			if (operatingPoint.Acceleration < 0) {
				var v2 = DataBus.VehicleSpeed + operatingPoint.Acceleration * operatingPoint.SimulationInterval;
				var nextAcceleration = DriverData.AccelerationCurve.Lookup(v2).Deceleration;
				var tmp = ComputeTimeInterval(VectoMath.Min(operatingPoint.Acceleration, nextAcceleration),
					operatingPoint.SimulationDistance);
				if (operatingPoint.SimulationDistance.Equals(tmp.SimulationDistance)) {
					// only decelerate more of the simulation interval is not modified
					// i.e., braking to the next sample point
					Log.Debug("adjusting acceleration from {0} to {1}", operatingPoint.Acceleration, tmp.Acceleration);
					operatingPoint = tmp;
					// ComputeTimeInterval((operatingPoint.Acceleration + tmp.Acceleration) / 2, operatingPoint.SimulationDistance);
				}
			}


			var response = previousResponse ??
							NextComponent.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);

			response.Switch().
				Case<ResponseSuccess>(r => retVal = r).
				Case<ResponseOverload>(r => retVal = r).
				Case<ResponseUnderload>(). // will be handled in SearchBrakingPower
				Case<ResponseGearShift>(). // will be handled in SearchBrakingPower
				Case<ResponseFailTimeInterval>(r =>
					retVal = new ResponseDrivingCycleDistanceExceeded() {
						Source = this,
						MaxDistance = DataBus.VehicleSpeed * r.DeltaT + operatingPoint.Acceleration / 2 * r.DeltaT * r.DeltaT
					}).
				Default(r => {
					throw new UnexpectedResponseException("DrivingAction Brake: first request.", r);
				});

			if (retVal != null) {
				return retVal;
			}

			operatingPoint = SearchBrakingPower(absTime, operatingPoint.SimulationDistance, gradient,
				operatingPoint.Acceleration, response);

			if (!ds.IsEqual(operatingPoint.SimulationDistance, 1E-15)) {
				Log.Info(
					"SearchOperatingPoint Breaking reduced the max. distance: {0} -> {1}. Issue new request from driving cycle!",
					operatingPoint.SimulationDistance, ds);
				return new ResponseDrivingCycleDistanceExceeded {
					Source = this,
					MaxDistance = operatingPoint.SimulationDistance
				};
			}

			Log.Debug("Found operating point for breaking. dt: {0}, acceleration: {1}", operatingPoint.SimulationInterval,
				operatingPoint.Acceleration);

			retVal = NextComponent.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient);

			retVal.Switch().
				Case<ResponseSuccess>().
				Default(r => {
					throw new UnexpectedResponseException("DrivingAction Brake: request failed after braking power was found.", r);
				});
			CurrentState.Acceleration = operatingPoint.Acceleration;
			CurrentState.dt = operatingPoint.SimulationInterval;
			CurrentState.Response = retVal;

			return retVal;
		}


		// ================================================

		/// <summary>
		/// 
		/// </summary>
		/// <param name="operatingPoint"></param>
		/// <param name="limits"></param>
		/// <returns></returns>
		private OperatingPoint LimitAccelerationByDriverModel(OperatingPoint operatingPoint,
			LimitationMode limits)
		{
			var limitApplied = false;
			var originalAcceleration = operatingPoint.Acceleration;
			if (((limits & LimitationMode.LimitDecelerationLookahead) != 0) &&
				operatingPoint.Acceleration < LookaheadDeceleration) {
				operatingPoint.Acceleration = LookaheadDeceleration;
				limitApplied = true;
			}
			var accelerationLimits = DriverData.AccelerationCurve.Lookup(DataBus.VehicleSpeed);
			if (operatingPoint.Acceleration > accelerationLimits.Acceleration) {
				operatingPoint.Acceleration = accelerationLimits.Acceleration;
				limitApplied = true;
			}
			if (((limits & LimitationMode.LimitDecelerationDriver) != 0) &&
				operatingPoint.Acceleration < accelerationLimits.Deceleration) {
				operatingPoint.Acceleration = accelerationLimits.Deceleration;
				limitApplied = true;
			}
			if (limitApplied) {
				operatingPoint.SimulationInterval =
					ComputeTimeInterval(operatingPoint.Acceleration, operatingPoint.SimulationDistance).SimulationInterval;
				Log.Debug("Limiting acceleration from {0} to {1}, dt: {2}", originalAcceleration,
					operatingPoint.Acceleration, operatingPoint.SimulationInterval);
			}
			return operatingPoint;
		}

		/// <summary>
		/// Performs a search for the required braking power such that the vehicle accelerates with the given acceleration.
		/// Returns a new operating point (a, ds, dt) where ds may be shorter due to vehicle stopping
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="gradient"></param>
		/// <param name="acceleration"></param>
		/// <param name="initialResponse"></param>
		/// <returns>operating point (a, ds, dt) such that the vehicle accelerates with the given acceleration.</returns>
		private OperatingPoint SearchBrakingPower(Second absTime, Meter ds, Radian gradient,
			MeterPerSquareSecond acceleration, IResponse initialResponse)
		{
			Log.Info("Disabling logging during search iterations");
			LogManager.DisableLogging();

			var debug = new List<dynamic>(); // only used while testing

			var searchInterval = Constants.SimulationSettings.BreakingPowerInitialSearchInterval;

			var operatingPoint = new OperatingPoint() { SimulationDistance = ds, Acceleration = acceleration };
			Watt origDelta = null;
			initialResponse.Switch().
				Case<ResponseGearShift>(r => origDelta = r.GearboxPowerRequest).
				Case<ResponseUnderload>(r => origDelta = DataBus.ClutchClosed(absTime)
					? r.Delta
					: r.GearboxPowerRequest).
				Default(r => {
					throw new UnexpectedResponseException("cannot use response for searching braking power!", r);
				});
			var delta = origDelta;

			debug.Add(new { brakingPower = 0.SI<Watt>(), searchInterval, delta });

			var breakingPower = origDelta.Abs();

			// double the searchInterval until a good interval was found
			var intervalFactor = 2.0;
			var retryCount = 0;
			//ResponseDryRun response;
			do {
				operatingPoint = ComputeTimeInterval(operatingPoint.Acceleration, ds);
				DataBus.BreakPower = breakingPower;
				var response =
					(ResponseDryRun)
						NextComponent.Request(absTime, operatingPoint.SimulationInterval, operatingPoint.Acceleration, gradient, true);
				delta = DataBus.ClutchClosed(absTime) ? response.DeltaDragLoad : response.GearboxPowerRequest;

				if (delta.IsEqual(0, Constants.SimulationSettings.EnginePowerSearchTolerance)) {
					LogManager.EnableLogging();
					Log.Debug("found operating point in {0} iterations, delta: {1}", debug.Count, delta);
					return operatingPoint;
				}

				debug.Add(new { breakingPower, searchInterval, delta });

				// check if a correct searchInterval was found (when the delta changed signs, we stepped through the 0-point)
				// from then on the searchInterval can be bisected.
				if (origDelta.Sign() != delta.Sign()) {
					intervalFactor = 0.5;
				}

				searchInterval *= intervalFactor;
				breakingPower += searchInterval * -delta.Sign();
			} while (retryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			LogManager.EnableLogging();
			Log.Warn("Exceeded max iterations when searching for operating point!");
			Log.Warn("exceeded: {0} ... {1}", ", ".Join(debug.Take(5)), ", ".Join(debug.Slice(-6)));
			Log.Error("Failed to find operating point for breaking!");
			throw new VectoSimulationException("Failed to find operating point for breaking!exceeded: {0} ... {1}",
				", ".Join(debug.Take(5)), ", ".Join(debug.Slice(-6)));
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
		/// <param name="initialResponse"></param>
		/// <param name="coasting">if true approach the drag-load curve, otherwise full-load curve</param>
		/// <returns></returns>
		protected OperatingPoint SearchOperatingPoint(Second absTime, Meter ds, Radian gradient,
			MeterPerSquareSecond acceleration, IResponse initialResponse, bool coasting = false)
		{
			Log.Info("Disabling logging during search iterations");
			LogManager.DisableLogging();

			var debug = new List<dynamic>();

			var retVal = new OperatingPoint { Acceleration = acceleration, SimulationDistance = ds };

			var actionRoll = !DataBus.ClutchClosed(absTime);

			var searchInterval = Constants.SimulationSettings.OperatingPointInitialSearchIntervalAccelerating;

			var curve = DriverData.AccelerationCurve.Lookup(DataBus.VehicleSpeed);
			var intervalFactor = 1.0;

			Watt origDelta = null;
			if (actionRoll) {
				initialResponse.Switch().
					Case<ResponseDryRun>(r => origDelta = r.GearboxPowerRequest).
					Case<ResponseFailTimeInterval>(r => origDelta = r.GearboxPowerRequest).
					Default(r => {
						throw new UnexpectedResponseException("Unknown response type.", r);
					});
			} else {
				initialResponse.Switch().
					Case<ResponseOverload>(r => origDelta = r.Delta). // search operating point in drive action after overload
					Case<ResponseDryRun>(r => origDelta = coasting ? r.DeltaDragLoad : r.DeltaFullLoad).
					Default(r => {
						throw new UnexpectedResponseException("Unknown response type.", r);
					});
			}
			var delta = origDelta;

			var retryCount = 0;
			do {
				debug.Add(new { delta, acceleration = retVal.Acceleration, searchInterval });

				// check if a correct searchInterval was found (when the delta changed signs, we stepped through the 0-point)
				// from then on the searchInterval can be bisected.
				if (origDelta.Sign() != delta.Sign()) {
					intervalFactor = 0.5;
				}

				searchInterval *= intervalFactor;
				retVal.Acceleration += searchInterval * -delta.Sign();

				if (retVal.Acceleration < (10 * DriverData.AccelerationCurve.MaxDeceleration() - searchInterval)
					|| retVal.Acceleration > (DriverData.AccelerationCurve.MaxAcceleration() + searchInterval)) {
					LogManager.EnableLogging();
					Log.Warn("Operating Point outside driver acceleration limits: a: {0}", retVal.Acceleration);
					LogManager.DisableLogging();
					//throw new VectoSimulationException(
					//	"Could not find an operating point: operating point outside of driver acceleration limits. a: {0}",
					//	retVal.Acceleration);
				}

				// TODO: move to driving mode
				// check for minimum acceleration, add some safety margin due to search
				//if (!coasting && retVal.Acceleration.Abs() < Constants.SimulationSettings.MinimumAcceleration / 5.0
				//	&& searchInterval.Abs() < Constants.SimulationSettings.MinimumAcceleration / 20.0) {
				//	throw new VectoSimulationException("Could not achieve minimum acceleration");
				//}

				var tmp = ComputeTimeInterval(retVal.Acceleration, ds);
				retVal.SimulationInterval = tmp.SimulationInterval;
				retVal.SimulationDistance = tmp.SimulationDistance;

				var response =
					(ResponseDryRun)NextComponent.Request(absTime, retVal.SimulationInterval, retVal.Acceleration, gradient, true);
				delta = actionRoll ? response.GearboxPowerRequest : (coasting ? response.DeltaDragLoad : response.DeltaFullLoad);

				if (delta.IsEqual(0, Constants.SimulationSettings.EnginePowerSearchTolerance)) {
					LogManager.EnableLogging();
					Log.Debug("found operating point in {0} iterations. Engine Power req: {2}, Gearbox Power req: {3} delta: {1}",
						debug.Count, delta, response.EnginePowerRequest, response.GearboxPowerRequest);
					return retVal;
				}
			} while (retryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			LogManager.EnableLogging();
			Log.Warn("Exceeded max iterations when searching for operating point!");
			Log.Warn("acceleration: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.acceleration)),
				", ".Join(debug.Slice(-6).Select(x => x.acceleration)));
			Log.Warn("exceeded: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.delta)),
				", ".Join(debug.Slice(-6).Select(x => x.delta)));
			Log.Error("Failed to find operating point! absTime: {0}", absTime);
			throw new VectoSimulationException("Failed to find operating point!  exceeded: {0} ... {1}",
				", ".Join(debug.Take(5).Select(x => x.delta)),
				", ".Join(debug.Slice(-6).Select(x => x.delta)));
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
			var currentSpeed = DataBus.VehicleSpeed;
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
			retVal = ComputeTimeInterval(retVal.Acceleration, ds);

			if (ds.IsEqual(retVal.SimulationDistance)) {
				return retVal;
			}

			// this case should not happen, acceleration has been computed such that the target speed
			// can be reached within ds.
			Log.Error(
				"Unexpected Condition: Distance has been adjusted from {0} to {1}, currentVelocity: {2} acceleration: {3}, targetVelocity: {4}",
				retVal.SimulationDistance, ds, currentSpeed, CurrentState.Acceleration, targetVelocity);
			throw new VectoSimulationException("Simulation distance unexpectedly adjusted! {0} -> {1}", ds,
				retVal.SimulationDistance);
		}


		/// <summary>
		/// computes the distance required to decelerate from the current velocity to the given target velocity considering
		/// the drivers acceleration/deceleration curve.
		/// </summary>
		/// <param name="targetSpeed"></param>
		/// <returns></returns>
		public Meter ComputeDecelerationDistance(MeterPerSecond targetSpeed)
		{
			return DriverData.AccelerationCurve.ComputeAccelerationDistance(DataBus.VehicleSpeed, targetSpeed);
		}


		/// <summary>
		/// Computes the time interval for driving the given distance ds with the vehicle's current speed and the given acceleration.
		/// If the distance ds can not be reached (i.e., the vehicle would halt before ds is reached) then the distance parameter is adjusted.
		/// Returns a new operating point (a, ds, dt)
		/// </summary>
		/// <param name="acceleration"></param>
		/// <param name="ds"></param>
		/// <returns>Operating point (a, ds, dt)</returns>
		private OperatingPoint ComputeTimeInterval(MeterPerSquareSecond acceleration, Meter ds)
		{
			if (!(ds > 0)) {
				throw new VectoSimulationException("ds has to be greater than 0! ds: {0}", ds);
			}
			var currentSpeed = DataBus.VehicleSpeed;
			var retVal = new OperatingPoint() { Acceleration = acceleration, SimulationDistance = ds };
			if (acceleration.IsEqual(0)) {
				if (currentSpeed > 0) {
					retVal.SimulationInterval = ds / currentSpeed;
					return retVal;
				}
				Log.Error("vehicle speed is {0}, acceleration is {1}", currentSpeed, acceleration);
				throw new VectoSimulationException(
					"vehicle speed has to be > 0 if acceleration = 0!  v: {0}", currentSpeed);
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
					Log.Error(
						"Could not find solution for computing required time interval to drive distance ds: {0}. currentSpeed: {1}, acceleration: {2}, stopDistance: {3}, distance: {4}",
						ds, currentSpeed, acceleration, stopDistance, DataBus.Distance);
					throw new VectoSimulationException(
						"Could not find solution for time-interval!  ds: {0}, stopDistance: {1}", ds, stopDistance);
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
			if (!targetVelocity.IsEqual(0) || !DataBus.VehicleSpeed.IsEqual(0)) {
				throw new NotImplementedException("TargetVelocity or VehicleVelocity is not zero!");
			}
			DataBus.BreakPower = double.PositiveInfinity.SI<Watt>();
			var retVal = NextComponent.Request(absTime, dt, 0.SI<MeterPerSquareSecond>(), gradient);
			CurrentState.dt = dt;
			CurrentState.Acceleration = 0.SI<MeterPerSquareSecond>();
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
			CurrentState.Response = null;
		}


		public class DriverState
		{
			public Second dt;
			public MeterPerSquareSecond Acceleration;
			public IResponse Response;
		}

		[DebuggerDisplay("a: {Acceleration}, dt: {SimulationInterval}, ds: {SimulationDistance}")]
		public struct OperatingPoint
		{
			public MeterPerSquareSecond Acceleration;
			public Meter SimulationDistance;
			public Second SimulationInterval;
		}

		[Flags]
		protected enum LimitationMode
		{
			NoLimitation = 0x0,
			//LimitAccelerationDriver = 0x1,
			LimitDecelerationDriver = 0x2,
			LimitDecelerationLookahead = 0x4
		}
	}
}