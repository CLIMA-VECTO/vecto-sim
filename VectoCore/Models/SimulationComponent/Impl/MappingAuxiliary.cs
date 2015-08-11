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

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun)
		{
			if (_outPort == null) {
				Log.ErrorFormat("{0} cannot handle incoming request - no outport available", absTime);
				throw new VectoSimulationException(
					string.Format("{0} cannot handle incoming request - no outport available",
						absTime));
			}

			var torqueAux = GetPowerDemand(absTime, dt, angularVelocity);

			return _outPort.Request(absTime, dt, torque + torqueAux, angularVelocity);
		}

		private NewtonMeter GetPowerDemand(Second absTime, Second dt, PerSecond angularVelocity)
		{
			var powerSupply = _demand.GetPowerDemand(absTime, dt);
			var powerAuxOut = powerSupply / _data.EfficiencyToSupply;

			var nAuxiliary = angularVelocity * _data.TransitionRatio;

			var powerAuxIn = _data.GetPowerDemand(nAuxiliary, powerAuxOut);
			var powerAux = powerAuxIn / _data.EfficiencyToEngine;

			_powerDemand = powerAux;

			var torqueAux = Formulas.PowerToTorque(powerAux, angularVelocity);
			return torqueAux;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			var torqueAux = GetPowerDemand(0.SI<Second>(), 0.SI<Second>(), angularVelocity);

			return _outPort.Initialize(torque + torqueAux, angularVelocity);
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.Paux_xxx] = _powerDemand;
		}

		protected override void DoCommitSimulationStep() {}

		#endregion
	}
}