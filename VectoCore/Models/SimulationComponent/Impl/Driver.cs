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
						SearchOperatingPoint(absTime, ds, targetVelocity, gradient, retVal);
						break;
				}
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);

			return new ResponseDrivingCycleDistanceExceeded() { SimulationInterval = CurrentState.dt };
		}

		private void SearchOperatingPoint(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient,
			IResponse response)
		{
			var exceeded = new List<double>();
			var acceleration = new List<double>();
			var searchInterval = CurrentState.Acceleration.Value() / 2.0;

			do {
				var delta = 0.0;
				switch (response.ResponseType) {
					case ResponseType.FailOverload:
						delta = ((ResponseFailOverload)response).Delta;
						break;
					case ResponseType.DryRun:
						delta = ((ResponseDryRun)response).DeltaFullLoad;
						break;
					default:
						throw new VectoSimulationException("Unknown response type");
				}

				exceeded.Add(delta);
				acceleration.Add(CurrentState.Acceleration.Value());
				if (delta.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
					return;
				}
				if (delta > 0) {
					CurrentState.Acceleration -= searchInterval;
				} else {
					CurrentState.Acceleration += searchInterval;
				}
				searchInterval /= 2.0;
				CurrentState.dt = ComputeTimeInterval(ds, targetVelocity, CurrentState.Acceleration);

				response = Next.Request(absTime, CurrentState.dt, CurrentState.Acceleration, gradient, true);
				//factor *= 2;
			} while (CurrentState.RetryCount++ < Constants.SimulationSettings.DriverSearchLoopThreshold);
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
			CurrentState.dt = ComputeTimeInterval(ds, targetVelocity, CurrentState.Acceleration);
		}

		private Second ComputeTimeInterval(Meter ds, MeterPerSecond targetVelocity, MeterPerSquareSecond acceleration)
		{
			var currentSpeed = DataBus.VehicleSpeed();

			if (acceleration.IsEqual(0)) {
				return (ds / currentSpeed).Cast<Second>();
			}

			// we need to accelerate / decelerate. solve quadratic equation...
			// ds = acceleration / 2 * dt^2 + currentSpeed * dt   => solve for dt
			var solutions = VectoMath.QuadraticEquationSolver(acceleration.Value() / 2.0, currentSpeed.Value(),
				-ds.Value());
			solutions = solutions.Where(x => x >= 0).ToList();

			if (solutions.Count == 0) {
				Log.WarnFormat(
					"Could not find solution for computing required time interval to drive distance {0}. currentSpeed: {1}, targetSpeed: {2}, acceleration: {3}",
					ds, currentSpeed, targetVelocity, acceleration);
				return 0.SI<Second>();
			}
			// if there are 2 positive solutions (i.e. when decelerating), take the smaller time interval
			// (the second solution means that you reach negative speed 
			solutions.Sort();
			return solutions.First().SI<Second>();
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