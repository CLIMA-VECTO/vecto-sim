using System;
using System.Collections.Generic;
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
	public class EngineOnlySimulation : VectoSimulationComponent, IDrivingCycleInfo, IEngineOnlySimulation, ITnInPort,
		ISimulationOutPort
	{
		protected DrivingCycleData Data;
		private ITnOutPort _outPort;
		private IEnumerator<DrivingCycleData.DrivingCycleEntry> RightSample { get; set; }
		private IEnumerator<DrivingCycleData.DrivingCycleEntry> LeftSample { get; set; }


		public EngineOnlySimulation(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
			LeftSample = Data.Entries.GetEnumerator();
			LeftSample.MoveNext();

			RightSample = Data.Entries.GetEnumerator();
			RightSample.MoveNext();
			RightSample.MoveNext();
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
			var index = 0;
			return _outPort.Initialize(Data.Entries[index].EngineTorque, Data.Entries[index].EngineSpeed);
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

		protected override void DoCommitSimulationStep()
		{
			LeftSample.MoveNext();
			RightSample.MoveNext();
		}

		#endregion

		public CycleData CycleData()
		{
			return new CycleData {
				AbsTime = LeftSample.Current.Time,
				AbsDistance = null,
				LeftSample = LeftSample.Current,
				RightSample = RightSample.Current,
			};
		}
	}
}