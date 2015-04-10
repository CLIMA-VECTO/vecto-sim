using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class DummyGearbox : VectoSimulationComponent, IGearbox, ITnInPort, ITnOutPort
	{
		private ITnOutPort _outPort;
		public uint CurrentGear { get; set; }

		 
		public DummyGearbox(IVehicleContainer cockpit) : base(cockpit) {}

		public ITnInPort InShaft()
		{
			return this;
		}

		public ITnOutPort OutShaft()
		{
			return this;
		}

		public uint Gear()
		{
			return CurrentGear;
		}

		public void Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond engineSpeed)
		{
			throw new NotImplementedException();
		}

		public void Connect(IOutPort other)
		{
			throw new NotImplementedException();
		}

		public override void CommitSimulationStep(IModalDataWriter writer) {}
	}
}