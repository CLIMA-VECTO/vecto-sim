using System;
using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	class Wheels : VectoSimulationComponent
	{
		public override IInPort InPort()
		{
			throw new NotImplementedException();
		}

		public override IOutPort OutPort()
		{
			throw new NotImplementedException();
		}

	    public override void CommitSimulationStep(IDataWriter writer)
	    {
	        throw new NotImplementedException();
	    }
	}
}
