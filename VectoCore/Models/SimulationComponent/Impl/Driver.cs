using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Driver : IDriver, IDriverDemandInPort, IDriverDemandOutPort
	{
		public Driver(DriverData driverData)
		{
			throw new NotImplementedException();
		}

		public IDriverDemandInPort InShaft()
		{
			throw new NotImplementedException();
		}

		public IDriverDemandOutPort OutShaft()
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
	}
}