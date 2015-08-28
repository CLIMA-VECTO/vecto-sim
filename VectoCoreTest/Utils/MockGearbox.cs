using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockGearbox : VectoSimulationComponent, IGearbox, ITnInPort, ITnOutPort
	{
		private ITnOutPort _outPort;

		public MockGearbox(IVehicleContainer cockpit) : base(cockpit) {}

		public ITnInPort InPort()
		{
			return this;
		}

		public ITnOutPort OutPort()
		{
			return this;
		}

		public uint Gear { get; set; }


		public void Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun = false)
		{
			throw new NotImplementedException();
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			throw new NotImplementedException();
		}


		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			// noting to write
		}

		protected override void DoCommitSimulationStep() {}
	}
}