using System;
using System.CodeDom;
using System.Linq;
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

		public IResponse Request(TimeSpan absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			var retVal = DoHandleRequest(absTime, ds, targetVelocity, gradient);

			CurrentState.Response = retVal;

			//switch (retVal.ResponseType) {}
			return retVal;
		}


		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			var retVal = DoHandleRequest(absTime, dt, targetVelocity, gradient);

			CurrentState.Response = retVal;

			//switch (retVal.ResponseType) {}
			return retVal;
		}


		protected IResponse DoHandleRequest(TimeSpan absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			var currentSpeed = Cockpit.VehicleSpeed();

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

			var solutions = VectoMath.QuadraticEquationSolver(requiredAcceleration.Value() / 2.0, currentSpeed.Value(),
				-ds.Value());
			solutions = solutions.Where(x => x >= 0).ToList();

			if (solutions.Count == 0) {
				Log.WarnFormat(
					"Could not find solution for computing required time interval to drive distance {0}. currentSpeed: {1}, targetSpeed: {2}, acceleration: {3}",
					ds, currentSpeed, targetVelocity, requiredAcceleration);
				return new ResponseFailTimeInterval();
			}
			var dt = TimeSpan.FromSeconds(solutions.First());
			var retVal = Next.Request(absTime, dt, requiredAcceleration, gradient);
			retVal.SimulationInterval = dt;
			return retVal;
		}


		protected IResponse DoHandleRequest(TimeSpan absTime, TimeSpan dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			if (!targetVelocity.IsEqual(0) || !Cockpit.VehicleSpeed().IsEqual(0)) {
				throw new NotImplementedException("TargetVelocity or VehicleVelocity is not zero!");
			}
			return Next.Request(absTime, dt, 0.SI<MeterPerSquareSecond>(), gradient);
		}


		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			// todo??
		}

		protected override void DoCommitSimulationStep()
		{
			if (CurrentState.Response.ResponseType != ResponseType.Success) {
				throw new VectoSimulationException("Previois request did not succeed!");
			}
		}


		public class DriverState
		{
			public IResponse Response;
		}
	}
}