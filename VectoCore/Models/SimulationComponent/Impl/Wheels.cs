using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Wheels : VectoSimulationComponent, IWheels, IFvOutPort, ITnInPort
	{
		private ITnOutPort _outPort;

		public Wheels(IVehicleContainer cockpit)
			: base(cockpit) {}

		#region IRoadPortOutProvider

		IFvOutPort IRoadPortOutProvider.OutShaft()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IInShaft

		ITnInPort IInShaft.InShaft()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IFvOutPort

		IResponse IFvOutPort.Request(TimeSpan absTime, TimeSpan dt, Newton force, MeterPerSecond velocity)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}