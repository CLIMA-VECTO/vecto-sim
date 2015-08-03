using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
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

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
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
			var retVal = DoHandleRequest(absTime, dt, targetVelocity, gradient);

			CurrentState.Response = retVal;

			//switch (retVal.ResponseType) {}
			return retVal;
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			return Next.Initialize(vehicleSpeed, roadGradient);
		}

		internal IResponse DoCoast(Second absTime, Meter ds, Radian gradient)
		{
			ComputeAcceleration(ds, 0.SI<MeterPerSecond>());

			if (CurrentState.dt.IsEqual(0)) {
				return new ResponseFailTimeInterval();
			}

			var response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);

			var distance = SearchOperatingPoint(absTime, ds, gradient, response, true);
			if (!ds.IsEqual(distance)) {
				return new ResponseDrivingCycleDistanceExceeded() { MaxDistance = distance, SimulationInterval = CurrentState.dt };
			}

			var retVal = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient);
			CurrentState.Response = retVal;
			switch (retVal.ResponseType) {
				case ResponseType.Success:
					retVal.SimulationInterval = CurrentState.dt;
					return retVal;
			}

			return new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}


		protected IResponse DoHandleRequest(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			ComputeAcceleration(ds, targetVelocity);
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
						SearchOperatingPoint(absTime, ds, gradient, retVal);
						break;
				}
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			return new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		private Meter SearchOperatingPoint(Second absTime, Meter ds, Radian gradient,
			IResponse response, bool coasting = false)
		{
			var exceeded = new List<double>(); // only used while testing
			var acceleration = new List<double>(); // only used while testing
			var searchInterval = CurrentState.Acceleration.Value() / 2.0;
			Meter computedDs;

			do {
				var delta = 0.0;
				computedDs = ds;
				switch (response.ResponseType) {
					case ResponseType.FailOverload:
						delta = ((ResponseFailOverload)response).Delta;
						break;
					case ResponseType.DryRun:
						delta = coasting ? -((ResponseDryRun)response).DeltaDragLoad : ((ResponseDryRun)response).DeltaFullLoad;
						break;
					default:
						throw new VectoSimulationException("Unknown response type");
				}

				exceeded.Add(delta);
				acceleration.Add(CurrentState.Acceleration.Value());
				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					return computedDs;
				}
				if (delta > 0) {
					CurrentState.Acceleration -= searchInterval;
				} else {
					CurrentState.Acceleration += searchInterval;
				}
				searchInterval /= 2.0;
				ComputeTimeInterval(CurrentState.Acceleration, ref computedDs, out CurrentState.dt);
				response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);
			return computedDs;
		}

		private void ComputeAcceleration(Meter ds, MeterPerSecond targetVelocity)
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
			ComputeTimeInterval(CurrentState.Acceleration, ref ds, out CurrentState.dt);
		}

		private void ComputeTimeInterval(MeterPerSquareSecond acceleration, ref Meter ds, out Second dt)
		{
			var currentSpeed = DataBus.VehicleSpeed();

			if (acceleration.IsEqual(0)) {
				dt = (ds / currentSpeed).Cast<Second>();
				return;
			}

			// we need to accelerate / decelerate. solve quadratic equation...
			// ds = acceleration / 2 * dt^2 + currentSpeed * dt   => solve for dt
			var solutions = VectoMath.QuadraticEquationSolver(acceleration.Value() / 2.0, currentSpeed.Value(),
				-ds.Value());

			if (solutions.Count == 0) {
				// no real-valued solutions, required distance can not be reached (vehicle stopped?), adapt ds...
				dt = -(currentSpeed / acceleration).Cast<Second>();
				var stopDistance = (currentSpeed * dt + acceleration / 2 * dt * dt).Cast<Meter>();
				if (stopDistance > ds) {
					Log.WarnFormat(
						"Could not find solution for computing required time interval to drive distance {0}. currentSpeed: {1}, acceleration: {2}",
						ds, currentSpeed, acceleration);
					dt = 0.SI<Second>();
					return;
				}
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
		}


		public class DriverState
		{
			public Second dt;
			public MeterPerSquareSecond Acceleration;
			public IResponse Response;
			public int RetryCount;
		}
	}
}