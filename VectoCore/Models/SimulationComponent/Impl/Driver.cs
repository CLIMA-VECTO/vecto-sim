using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
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
			return null;
		}


		protected IResponse DoHandleRequest(TimeSpan absTime, TimeSpan dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			if (!targetVelocity.IsEqual(0) || !Cockpit.VehicleSpeed().IsEqual(0)) {
				throw new VectoException("TargetVelocity or VehicleVelocity is not zero!");
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