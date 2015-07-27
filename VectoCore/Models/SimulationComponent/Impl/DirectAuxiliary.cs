﻿using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class DirectAuxiliary : VectoSimulationComponent, IAuxiliary, ITnInPort, ITnOutPort
	{
		private readonly IAuxiliaryCycleData _demand;
		private ITnOutPort _outPort;
		private Watt _powerDemand;

		public DirectAuxiliary(IVehicleContainer container, IAuxiliaryCycleData demand)
			: base(container)
		{
			_demand = demand;
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

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed)
		{
			if (_outPort == null) {
				Log.ErrorFormat("{0} cannot handle incoming request - no outport available", absTime);
				throw new VectoSimulationException(
					string.Format("{0} cannot handle incoming request - no outport available",
						absTime));
			}

			_powerDemand = _demand.GetPowerDemand(absTime, dt);
			var tq = Formulas.PowerToTorque(_powerDemand, engineSpeed);
			return _outPort.Request(absTime, dt, torque + tq, engineSpeed);
		}

		public IResponse Initialize()
		{
			return _outPort.Initialize();
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.Paux] = _powerDemand;
		}

		protected override void DoCommitSimulationStep() {}

		#endregion
	}
}