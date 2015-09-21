using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class EngineOnlyGearbox : VectoSimulationComponent, IGearbox, ITnInPort, ITnOutPort
	{
		protected ITnOutPort NextComponent;
		public EngineOnlyGearbox(IVehicleContainer cockpit) : base(cockpit) {}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion ITnOutProvider

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IGearboxCockpit

		uint IGearboxInfo.Gear
		{
			get { return 0; }
			//set { }
		}

		public MeterPerSecond StartSpeed
		{
			get { throw new VectoSimulationException("Not Implemented: EngineOnlyGearbox has no StartSpeed value."); }
		}

		public MeterPerSquareSecond StartAcceleration
		{
			get { throw new VectoSimulationException("Not Implemented: EngineOnlyGearbox has no StartAcceleration value."); }
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			NextComponent = other;
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun)
		{
			if (NextComponent == null) {
				Log.Error("Ccannot handle incoming request - no outport available. absTime: {0}, dt: {1}", absTime, dt);
				throw new VectoSimulationException("Cannot handle incoming request - no outport available.");
			}
			return NextComponent.Request(absTime, dt, torque, engineSpeed, dryRun);
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			return NextComponent.Initialize(torque, engineSpeed);
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer) {}

		protected override void DoCommitSimulationStep() {}

		#endregion
	}
}