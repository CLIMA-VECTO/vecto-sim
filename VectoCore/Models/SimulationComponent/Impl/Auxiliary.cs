using System;
using System.Collections.Generic;
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

		protected ITnOutPort NextComponent;

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
			NextComponent = other;
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun)
		{
			var currentEngineSpeed = engineSpeed ?? DataBus.EngineSpeed;
			var powerDemand = ComputePowerDemand(currentEngineSpeed);

			return NextComponent.Request(absTime, dt, torque + powerDemand / currentEngineSpeed, engineSpeed, dryRun);
		}

		private Watt ComputePowerDemand(PerSecond engineSpeed)
		{
			_powerDemands.Clear();
			var powerDemand = 0.SI<Watt>();

			foreach (var kv in _auxDict) {
				var demand = kv.Value(engineSpeed);
				powerDemand += demand;
				_powerDemands[kv.Key] = demand;
			}
			return powerDemand;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			var powerDemand = ComputePowerDemand(engineSpeed);
			return NextComponent.Initialize(torque + powerDemand / engineSpeed, engineSpeed);
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			var sum = 0.SI<Watt>();
			foreach (var kv in _powerDemands) {
				sum += kv.Value;
				// todo: aux write directauxiliary somewhere to moddata .... probably Padd column??
				if (!string.IsNullOrWhiteSpace(kv.Key)) {
					writer[kv.Key] = kv.Value;
				}
			}
			writer[ModalResultField.Paux] = sum;
		}

		protected override void DoCommitSimulationStep() {}

		#endregion

		public void AddConstant(string auxId, Watt powerDemand)
		{
			_auxDict[auxId] = speed => powerDemand;
		}

		public void AddDirect(IDrivingCycleInfo cycle)
		{
			_auxDict[DirectAuxiliaryId] = speed => cycle.CycleData().LeftSample.AdditionalAuxPowerDemand;
		}

		public void AddMapping(string auxId, IDrivingCycleInfo cycle, AuxiliaryData data)
		{
			if (!cycle.CycleData().LeftSample.AuxiliarySupplyPower.ContainsKey("Aux_" + auxId)) {
				var error = string.Format("driving cycle does not contain column for auxiliary: {0}", auxId);
				Log.Error(error);
				throw new VectoException(error);
			}

			_auxDict[auxId] = speed => {
				var powerSupply = cycle.CycleData().LeftSample.AuxiliarySupplyPower["Aux_" + auxId];
				var nAuxiliary = speed * data.TransitionRatio;
				var powerAuxOut = powerSupply / data.EfficiencyToSupply;
				var powerAuxIn = data.GetPowerDemand(nAuxiliary, powerAuxOut);
				return powerAuxIn / data.EfficiencyToEngine;
			};
		}
	}
}