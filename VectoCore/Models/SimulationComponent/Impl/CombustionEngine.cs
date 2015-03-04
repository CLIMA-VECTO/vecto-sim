using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class CombustionEngine : VectoSimulationComponent, ICombustionEngine
	{
		public CombustionEngine(CombustionEngineData data)
		{

		}

		public override InPort InPort()
		{
			throw new NotImplementedException();
		}

		public override OutPort OutPort()
		{
			throw new NotImplementedException();
		}

	    public override void CommitSimulationStep(IDataWriter writer)
	    {
	        writer[ModalResultFields.FC] = 1;
	        writer[ModalResultFields.FC_AUXc] = 2;
            writer[ModalResultFields.FC_WHTCc] = 3;
	    }
	}
}
