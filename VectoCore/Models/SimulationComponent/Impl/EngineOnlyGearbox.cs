using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class EngineOnlyGearbox : VectoSimulationComponent, IGearbox, ITnInPort, ITnOutPort
	{
		private ITnOutPort _outPort;
		public EngineOnlyGearbox(IVehicleContainer cockpit) : base(cockpit) {}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion ITnOutProvider

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IGearboxCockpit

		uint IGearboxCockpit.Gear()
		{
			return 0;
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond engineSpeed)
		{
			if (_outPort == null) {
				Log.ErrorFormat("{0} cannot handle incoming request - no outport available", absTime);
				throw new VectoSimulationException(
					string.Format("{0} cannot handle incoming request - no outport available",
						absTime.TotalSeconds));
			}
			return _outPort.Request(absTime, dt, torque, engineSpeed);
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer) {}

		#endregion
	}
}