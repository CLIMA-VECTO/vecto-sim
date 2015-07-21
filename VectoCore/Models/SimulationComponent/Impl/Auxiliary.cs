using System;
using System.Collections.Generic;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Auxiliary : VectoSimulationComponent, IAuxiliary, ITnInPort, ITnOutPort
	{
		public const string DirectAuxiliaryId = "";

		private readonly Dictionary<string, Func<PerSecond, Watt>> _auxDict = new Dictionary<string, Func<PerSecond, Watt>>();
		private readonly Dictionary<string, Watt> _powerDemands = new Dictionary<string, Watt>();

		private ITnOutPort _outPort;

		public Auxiliary(IVehicleContainer container) : base(container) {}

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

		IResponse ITnOutPort.Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond engineSpeed)
		{
			_powerDemands.Clear();
			var powerDemand = 0.SI<Watt>();

			foreach (var kv in _auxDict) {
				var demand = kv.Value(engineSpeed);
				powerDemand += demand;
				_powerDemands[kv.Key] = demand;
			}

			return _outPort.Request(absTime, dt, torque + powerDemand * engineSpeed, engineSpeed);
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			var sum = 0.SI<Watt>();
			foreach (var kv in _powerDemands) {
				sum += kv.Value;
				// todo: aux write directauxiliary somewhere to moddata .... probably Padd column??
				if (!string.IsNullOrWhiteSpace(kv.Key)) {
					writer[kv.Key] = kv.Value;
				}
			}
			writer[ModalResultField.Paux] = sum.Value();
		}

		#endregion

		public void AddConstant(string auxId, Watt powerDemand)
		{
			_auxDict[auxId] = ignored => powerDemand;
		}

		public void AddDirect(IDrivingCycleCockpit cycle)
		{
			_auxDict[DirectAuxiliaryId] = ignored => cycle.CycleData().LeftSample.AdditionalAuxPowerDemand;
		}

		public void AddMapping(string auxId, IDrivingCycleCockpit cycle, MappingAuxiliaryData data)
		{
			if (!cycle.CycleData().LeftSample.AuxiliarySupplyPower.ContainsKey(auxId)) {
				var error = string.Format("driving cycle does not contain column for auxiliary: {0}", auxId);
				LogManager.GetLogger(GetType()).ErrorFormat(error);
				throw new VectoException(error);
			}

			_auxDict[auxId] = speed => {
				var powerSupply = cycle.CycleData().LeftSample.AuxiliarySupplyPower[auxId];

				var nAuxiliary = speed * data.TransitionRatio;
				var powerAuxOut = powerSupply / data.EfficiencyToSupply;
				var powerAuxIn = data.GetPowerDemand(nAuxiliary, powerAuxOut);
				return powerAuxIn / data.EfficiencyToEngine;
			};
		}
	}
}