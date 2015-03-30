using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class EngineOnlyGearbox : VectoSimulationComponent, IGearbox, ITnInPort, ITnOutPort
	{
		private ITnOutPort _outPort;

		public EngineOnlyGearbox(IVehicleContainer cockpit) : base(cockpit) { }

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
			return 0;
		}

		public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
		{
			_outPort.Request(absTime, dt, torque, engineSpeed);
		}

		public void Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		public void Connect(IOutPort other)
		{
			throw new NotImplementedException();
		}


		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			
		}
	}
}