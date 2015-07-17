using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class AuxiliaryData
	{
		public double EfficiencyToSupply { get; set; }
		public double TransitionRatio { get; set; }
		public double EfficiencyToEngine { get; set; }

		public Watt GetPowerDemand(PerSecond nAuxiliary, Watt powerAuxOut)
		{
			throw new NotImplementedException();
		}

		public static AuxiliaryData ReadFromFile(string filePath)
		{
			throw new NotImplementedException();
		}
	}

	public class MappingAuxiliary : VectoSimulationComponent, IAuxiliary, ITnInPort, ITnOutPort
	{
		private readonly IAuxiliaryCycleData _demand;
		private AuxiliaryData _data;
		private ITnOutPort _outPort;
		private Watt _powerDemand;

		public MappingAuxiliary(IVehicleContainer container, IAuxiliaryCycleData demand, AuxiliaryData data)
			: base(container)
		{
			_demand = demand;
			_data = data;
		}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond angularVelocity)
		{
			if (_outPort == null) {
				Log.ErrorFormat("{0} cannot handle incoming request - no outport available", absTime);
				throw new VectoSimulationException(
					string.Format("{0} cannot handle incoming request - no outport available",
						absTime.TotalSeconds));
			}

			var power_supply = _demand.GetPowerDemand(absTime, dt);
			var power_aux_out = power_supply / _data.EfficiencyToSupply;

			var n_auxiliary = angularVelocity * _data.TransitionRatio;

			var power_aux_in = _data.GetPowerDemand(n_auxiliary, power_aux_out);
			var power_aux = power_aux_in / _data.EfficiencyToEngine;

			_powerDemand = power_aux;

			var torque_aux = Formulas.PowerToTorque(power_aux, angularVelocity);
			return _outPort.Request(absTime, dt, torque + torque_aux, angularVelocity);
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			writer[ModalResultField.Paux_xxx] = (double)_powerDemand;
		}

		#endregion
	}
}