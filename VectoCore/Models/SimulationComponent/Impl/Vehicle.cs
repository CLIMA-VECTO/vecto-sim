using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Vehicle : IVehicle
	{
		private IFvOutPort _nextInstance;


		public IFvInPort InPort()
		{
			throw new NotImplementedException();
		}

		public IDriverDemandOutPort OutPort()
		{
			throw new NotImplementedException();
		}

		public void Connect(IFvOutPort other)
		{
			_nextInstance = other;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond accelleration, Radian gradient)
		{
			var force = 0.SI<Newton>();
			var velocity = 0.SI<MeterPerSecond>();
			return _nextInstance.Request(absTime, dt, force, velocity);
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, Radian gradient)
		{
			throw new NotImplementedException();
		}
	}
}