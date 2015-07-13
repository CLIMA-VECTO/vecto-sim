using System;
using System.Linq;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one Distance Based Driving Cycle
	/// </summary>
	public class DistanceBasedSimulation : VectoSimulationComponent, IDrivingCycle,
		ISimulationOutPort,
		IDrivingCycleInPort
	{
		protected DrivingCycleData Data;

		private DrivingCycleState _previousState = null;
		private DrivingCycleState _currentState = new DrivingCycleState();

		private IDrivingCycleOutPort _outPort;

		public DistanceBasedSimulation(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
		}

		#region IDrivingCycleInProvider

		public IDrivingCycleInPort InPort()
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

		#region IDrivingCycleInPort

		void IDrivingCycleInPort.Connect(IDrivingCycleOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region ISimulationOutPort

		IResponse ISimulationOutPort.Request(TimeSpan absTime, Meter ds)
		{
			//todo: Distance calculation and comparison!!!

			//Data.

			throw new NotImplementedException("Distance based Cycle is not yet implemented.");
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt)
		{
			throw new NotImplementedException();
		}

		public IResponse Initialize()
		{
			var first = Data.Entries.First();
			_previousState = new DrivingCycleState() {
				AbsTime = TimeSpan.FromSeconds(0),
				Distance = first.Distance.SI<Meter>(),
				Altitude = first.Altitude.SI<Meter>(),
				CycleIndex = 0,
			};
			return new ResponseSuccess();
			//TODO: return _outPort.Initialize();
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			_previousState = _currentState;
			_currentState = new DrivingCycleState();
			throw new NotImplementedException("Distance based Cycle is not yet implemented.");
		}

		#endregion

		protected void LookupCycle(Meter ds) {}


		public class DrivingCycleState
		{
			public TimeSpan AbsTime;

			public Meter Distance;

			public Meter Altitude;

			public int CycleIndex;
		}
	}
}