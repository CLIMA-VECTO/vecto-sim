using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one Time Based Driving Cycle
	/// </summary>
	public class TimeBasedDrivingCycle : VectoSimulationComponent, IDriverDemandDrivingCycle, IDriverDemandInPort
	{
		protected DrivingCycleData Data;

		public TimeBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
		}

		private IDriverDemandOutPort OutPort { get; set; }

		public IDriverDemandInPort InPort()
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

			return OutPort.Request(absTime, dt, Data.Entries[index].VehicleSpeed, Data.Entries[index].RoadGradient);
		}

		public void Connect(IDriverDemandOutPort other)
		{
			OutPort = other;
		}

		public override void CommitSimulationStep(IModalDataWriter writer) {}
	}
}