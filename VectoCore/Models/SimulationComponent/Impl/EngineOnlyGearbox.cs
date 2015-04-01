using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

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

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond engineSpeed)
		{
			if (_outPort == null) {
				Log.ErrorFormat("{0} cannot handle incoming request - no outport available", absTime);
				throw new VectoSimulationException(String.Format("{0} cannot handle incoming request - no outport available", absTime.TotalSeconds));
			}
			return _outPort.Request(absTime, dt, torque, engineSpeed);
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