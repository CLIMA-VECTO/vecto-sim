using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Exceptions;
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
			_nextEntry = cycle.Entries.GetEnumerator();
			CurrentEntry = _nextEntry.Current;
			_nextEntry.MoveNext();
		}

		private ITnOutPort OutPort { get; set; }
		private int CurrentStep { get; set; }

		protected IEnumerator<DrivingCycleData.DrivingCycleEntry> _nextEntry;
		protected DrivingCycleData.DrivingCycleEntry NextEntry { get { return _nextEntry.Current; } }
		protected DrivingCycleData.DrivingCycleEntry CurrentEntry { get; set; }

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
			if (absTime.TotalSeconds < CurrentEntry.Time.Double()) {
				Log.ErrorFormat(("cannot go back in time! current: {0}  requested: {1}", CurrentEntry.Time, absTime.TotalSeconds));
				throw new VectoSimulationException(String.Format("cannot go back in time! current: {0}  requested: {1}", CurrentEntry.Time, absTime.TotalSeconds));
			}
			while (NextEntry.Time <= absTime.TotalSeconds) {
				CurrentEntry = NextEntry;
				if (!_nextEntry.MoveNext()) {
					return new ResponseCycleFinished();
				}
			}
			if (dt.TotalSeconds > (NextEntry.Time - CurrentEntry.Time).Double()) {
				return new ResponseFailTimeInterval(TimeSpan.FromSeconds((NextEntry.Time - CurrentEntry.Time).Double()));
			}

			return OutPort.Request(absTime, dt, CurrentEntry.EngineTorque, CurrentEntry.EngineSpeed);
		}

		#endregion
	}
}