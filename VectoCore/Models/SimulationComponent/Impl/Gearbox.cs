using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Gearbox : VectoSimulationComponent, IGearbox, ITnOutPort, ITnInPort
	{
		protected ITnOutPort Next;

		internal GearboxData Data;

		public Gearbox(IVehicleContainer container, GearboxData gearboxData) : base(container)
		{
			Data = gearboxData;
		}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IGearboxCockpit

		uint IGearboxInfo.Gear()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun)
		{
			throw new NotImplementedException();
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			// todo: add gearbox losses
			return Next.Initialize(torque, engineSpeed);
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			Next = other;
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		protected override void DoCommitSimulationStep()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}