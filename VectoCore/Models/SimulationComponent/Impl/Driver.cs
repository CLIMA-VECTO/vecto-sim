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

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			throw new NotImplementedException();
		}


		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond accelleration, Radian gradient)
		{
			throw new NotImplementedException();
		}

		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		public override void CommitSimulationStep(IModalDataWriter writer) {}
	}
}