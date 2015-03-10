using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{

	public class CombustionEngine : VectoSimulationComponent, ICombustionEngine, ITnOutPort
	{
		public enum EngineState
		{
			Idle,
			Drag,
			FullDrag,
			Load,
			FullLoad,
			Stopped,
			Undef
		}

		private CombustionEngineData _data;

		private EngineState _state;
		private double _currentEnginePower;
		private double _currentEngineSpeed;

		public CombustionEngine(CombustionEngineData data)
		{
			_data = data;
			_state = EngineState.Idle;
			_currentEnginePower = 0;
			_currentEngineSpeed = _data.IdleSpeed;
		}


        public ITnOutPort OutShaft()
		{
			return this;
		}

	    public override void CommitSimulationStep(IModalDataWriter writer)
	    {
	        writer[ModalResultField.FC] = 1;
	        writer[ModalResultField.FCAUXc] = 2;
            writer[ModalResultField.FCWHTCc] = 3;
	    }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {

            //throw new NotImplementedException();
        }

		// accelleration los rotation engine
		//Return (ENG.I_mot * (nU - nUBefore) * 0.01096 * ((nU + nUBefore) / 2)) * 0.001
	}
}
