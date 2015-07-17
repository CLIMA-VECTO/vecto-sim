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
	public class TimeBasedDrivingCycle : VectoSimulationComponent, IDrivingCycle,
		IDrivingCycleInPort,
		ISimulationOutPort
	{
		protected DrivingCycleData Data;
		private IDrivingCycleOutPort _outPort;

		public TimeBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
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

		IResponse ISimulationOutPort.Request(TimeSpan absTime, Meter ds)
		{
			throw new NotImplementedException();
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt)
		{
			//todo: change to variable time steps
			var index = (int)Math.Floor(absTime.TotalSeconds);
			if (index >= Data.Entries.Count) {
				return new ResponseCycleFinished();
			}

			// TODO!!
			var dx = 0.SI<Meter>();
			return _outPort.Request(absTime, dt, Data.Entries[index].VehicleTargetSpeed,
				Data.Entries[index].RoadGradient);
		}

		public IResponse Initialize()
		{
			// nothing to initialize here...
			// TODO: _outPort.initialize();
			throw new NotImplementedException();
		}

		#endregion

		#region IDrivingCycleInPort

		void IDrivingCycleInPort.
			Connect(IDrivingCycleOutPort
				other)
		{
			_outPort = other;
		}

		#endregion

		#region VectoSimulationComponent

		public override
			void CommitSimulationStep
			(IModalDataWriter
				writer) {}

		#endregion
	}
}