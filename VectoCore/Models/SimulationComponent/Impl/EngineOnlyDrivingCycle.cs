using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one EngineOnly Driving Cycle
	/// </summary>
	public class EngineOnlyDrivingCycle : VectoSimulationComponent, IEngineOnlyDrivingCycle, ITnInPort
	{
		protected DrivingCycleData Data;

		public EngineOnlyDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
		}

		private ITnOutPort OutPort { get; set; }
		private int CurrentStep { get; set; }

		#region ITnInPort

		public void Connect(ITnOutPort other)
		{
			OutPort = other;
		}

		#endregion

		public override void CommitSimulationStep(IModalDataWriter writer) {}

		#region IInShaft

		public ITnInPort InShaft()
		{
			return this;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt)
		{
			//todo: change to variable time steps
			var index = (int) Math.Floor(absTime.TotalSeconds);
			if (index >= Data.Entries.Count) {
				return new ResponseCycleFinished();
			}

			return OutPort.Request(absTime, dt, Data.Entries[index].EngineTorque, Data.Entries[index].EngineSpeed);
		}

		#endregion
	}
}