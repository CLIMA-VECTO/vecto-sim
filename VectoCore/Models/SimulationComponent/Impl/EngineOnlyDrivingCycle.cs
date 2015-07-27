using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one EngineOnly Driving Cycle
	/// </summary>
	public class EngineOnlySimulation : VectoSimulationComponent, IDrivingCycleCockpit, IEngineOnlySimulation, ITnInPort,
		ISimulationOutPort
	{
		protected DrivingCycleData Data;
		private ITnOutPort _outPort;

		public EngineOnlySimulation(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
		}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion

		#region ISimulationOutProvider

		public ISimulationOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region ISimulationOutPort

		public IResponse Request(Second absTime, Meter ds)
		{
			throw new VectoSimulationException("Engine-Only Simulation can not handle distance request");
		}

		IResponse ISimulationOutPort.Request(Second absTime, Second dt)
		{
			//todo: change to variable time steps
			var index = (int)Math.Floor(absTime.Value());
			if (index >= Data.Entries.Count) {
				return new ResponseCycleFinished();
			}

			return _outPort.Request(absTime, dt, Data.Entries[index].EngineTorque, Data.Entries[index].EngineSpeed);
		}

		public IResponse Initialize()
		{
			// nothing to initialize here...
			// TODO: _outPort.initialize();
			throw new NotImplementedException();
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer) {}

		protected override void DoCommitSimulationStep() {}

		#endregion

		public CycleData CycleData()
		{
			//todo EngineOnlyDrivingCycle.CycleData
			throw new NotImplementedException();
		}
	}
}