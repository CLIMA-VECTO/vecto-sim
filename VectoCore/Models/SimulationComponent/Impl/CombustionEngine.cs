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


        public ITnOutPort OutShaft()
		{
			return this;
		}

	    public override void CommitSimulationStep(IModalDataWriter writer)
	    {
	        writer[ModalResultField.FC] = 1;
	        writer[ModalResultField.FC_AUXc] = 2;
            writer[ModalResultField.FC_WHTCc] = 3;
	    }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            //throw new NotImplementedException();
        }
	}
}
