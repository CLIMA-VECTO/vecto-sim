using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Driver : VectoSimulationComponent, IDriver, IDrivingCycleDemandOutPort, IDriverDemandInPort
	{
		public Driver(VehicleContainer container, DriverData driverData) : base(container)
		{
			throw new NotImplementedException();
		}

		public IDriverDemandInPort InShaft()
		{
			throw new NotImplementedException();
		}

		public void Connect(IDriverDemandOutPort other)
		{
			throw new NotImplementedException();
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, Radian gradient)
		{
			throw new NotImplementedException();
		}


		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond accelleration, Radian gradient)
		{
			throw new NotImplementedException();
		}

		public IDrivingCycleDemandOutPort OutShaft()
		{
			throw new NotImplementedException();
		}

		public override void CommitSimulationStep(IModalDataWriter writer) {}
	}
}