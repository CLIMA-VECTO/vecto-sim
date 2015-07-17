using System;
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
		protected IDriverDemandOutPort _other;

		public Driver(VehicleContainer container, DriverData driverData) : base(container)
		{
			//throw new NotImplementedException();
		}

		public IDriverDemandInPort InPort()
		{
			return this;
		}

		public void Connect(IDriverDemandOutPort other)
		{
			_other = other;
		}

		public IResponse Request(TimeSpan absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			throw new NotImplementedException();
		}


		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond accelleration, Radian gradient)
		{
			throw new NotImplementedException();
		}

		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		protected override void DoCommitSimulationStep() {}
	}
}