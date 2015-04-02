using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Wheels : VectoSimulationComponent
	{
		public Wheels(IVehicleContainer cockpit)
			: base(cockpit) {}

		public IInPort InPort()
		{
			throw new NotImplementedException();
		}

		public IOutPort OutPort()
		{
			throw new NotImplementedException();
		}

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}