using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	class Wheels : VectoSimulationComponent
	{
		public override Connector.Ports.Impl.InPort InPort()
		{
			throw new NotImplementedException();
		}

		public override Connector.Ports.Impl.OutPort OutPort()
		{
			throw new NotImplementedException();
		}

	    public override void CommitSimulationStep(IDataWriter writer)
	    {
	        throw new NotImplementedException();
	    }
	}
}
