using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    public class CombustionEngine : VectoSimulationComponent, ICombustionEngine, ITnOutPort
	{
		public CombustionEngine(CombustionEngineData data)
		{

		}

		public override IInPort InPort()
		{
			throw new NotSupportedException("The Engine does not support InPorts.");
		}

        public override IOutPort OutPort()
		{
			return this;
		}

	    public override void CommitSimulationStep(IDataWriter writer)
	    {
	        writer[ModalResult.FC] = 1;
	        writer[ModalResult.FC_AUXc] = 2;
            writer[ModalResult.FC_WHTCc] = 3;
	    }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            //throw new NotImplementedException();
        }
	}
}
