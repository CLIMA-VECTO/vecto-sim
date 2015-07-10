using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockDriver : VectoSimulationComponent, IDriver, IDrivingCycleDemandOutPort, IDriverDemandInPort
	{
		private IDriverDemandOutPort _next;

		public MockDriver(IVehicleContainer container) : base(container) {}

		public override void CommitSimulationStep(IModalDataWriter writer) {}


		public IDrivingCycleDemandOutPort OutShaft()
		{
			return this;
		}

		public IDriverDemandInPort InShaft()
		{
			return this;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, Radian gradient)
		{
			var acc = 0.SI<MeterPerSquareSecond>();
			return _next.Request(absTime, dt, acc, 0.SI<Radian>());
		}

		public void Connect(IDriverDemandOutPort other)
		{
			_next = other;
		}
	}
}