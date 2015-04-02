using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class EngineOnlyAuxiliary : VectoSimulationComponent, IAuxiliary, ITnInPort, ITnOutPort
	{
		private readonly AuxiliariesDemandAdapter _demand;
		private ITnOutPort _outPort;
		private Watt _powerDemand;

		public EngineOnlyAuxiliary(IVehicleContainer container, AuxiliariesDemandAdapter demand) : base(container)
		{
			_demand = demand;
		}

		public ITnInPort InShaft()
		{
			return this;
		}

		public ITnOutPort OutShaft()
		{
			return this;
		}

		public void Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond engineSpeed)
		{
			if (_outPort == null) {
				Log.ErrorFormat("{0} cannot handle incoming request - no outport available", absTime);
				throw new VectoSimulationException(String.Format("{0} cannot handle incoming request - no outport available",
					absTime.TotalSeconds));
			}
			_powerDemand = _demand.GetPowerDemand(absTime, dt);
			var tq = Formulas.PowerToTorque(_powerDemand, engineSpeed);
			return _outPort.Request(absTime, dt, (torque + tq).To<NewtonMeter>(), engineSpeed);
		}

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			writer[ModalResultField.Paux] = (double) _powerDemand;
		}
	}
}