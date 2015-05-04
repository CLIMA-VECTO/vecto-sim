using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one Distance Based Driving Cycle
	/// </summary>
	public class DistanceBasedDrivingCycle : VectoSimulationComponent, IDrivingCycleDemandDrivingCycle,
		IDrivingCycleOutPort,
		IDrivingCycleDemandInPort
	{
		protected TimeSpan AbsTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
		protected DrivingCycleData Data;
		protected double Distance = 0;
		protected TimeSpan Dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
		private IDrivingCycleDemandOutPort _outPort;

		public DistanceBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
		}

		#region IDrivingCycleDemandInProvider

		public IDrivingCycleDemandInPort InPort()
		{
			return this;
		}

		#endregion

		#region IDrivingCycleOutProvider

		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IDrivingCycleDemandInPort

		void IDrivingCycleDemandInPort.Connect(IDrivingCycleDemandOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region IDrivingCycleOutPort

		IResponse IDrivingCycleOutPort.Request(TimeSpan absTime, TimeSpan dt)
		{
			//todo: Distance calculation and comparison!!!
			throw new NotImplementedException("Distance based Cycle is not yet implemented.");
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			throw new NotImplementedException("Distance based Cycle is not yet implemented.");
		}

		#endregion
	}
}