using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Vehicle : IVehicle, IDriverDemandOutPort, IFvInPort
	{
		public Vehicle(VehicleContainer container, VehicleData vehicleData)
		{
			throw new NotImplementedException();
		}

		public IDriverDemandOutPort OutShaft()
		{
			throw new NotImplementedException();
		}

		public IFvInPort InShaft()
		{
			throw new NotImplementedException();
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, Radian gradient)
		{
			throw new NotImplementedException();
		}

		public void Connect(IFvOutPort other)
		{
			throw new NotImplementedException();
		}
	}
}