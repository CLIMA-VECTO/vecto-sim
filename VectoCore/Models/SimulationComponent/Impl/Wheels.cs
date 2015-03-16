using System;
using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	class Wheels : VectoSimulationComponent
	{
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
