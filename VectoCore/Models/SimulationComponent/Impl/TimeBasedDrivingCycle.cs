using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one Time Based Driving Cycle
	/// </summary>
	public class TimeBasedSimulation : VectoSimulationComponent, IDrivingCycle,
		IDrivingCycleInPort,
		ISimulationOutPort
	{
		protected DrivingCycleData Data;
		private IDrivingCycleOutPort _outPort;

		public TimeBasedSimulation(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
		}

		#region ISimulationOutProvider

		public ISimulationOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IDrivingCycleInProvider

		public IDrivingCycleInPort InPort()
		{
			return this;
		}

		#endregion

		#region ISimulationOutPort

		IResponse ISimulationOutPort.Request(TimeSpan absTime, TimeSpan dt)
		{
			//todo: change to variable time steps
			var index = (int)Math.Floor(absTime.TotalSeconds);
			if (index >= Data.Entries.Count) {
				return new ResponseCycleFinished();
			}

			return _outPort.Request(absTime, dt, Data.Entries[index].VehicleSpeed,
				Data.Entries[index].RoadGradient.SI().GradientPercent.Cast<Radian>());
		}

		#endregion

		#region IDrivingCycleInPort

		void IDrivingCycleInPort.Connect(IDrivingCycleOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer) {}

		#endregion
	}
}