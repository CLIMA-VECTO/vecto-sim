using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockGearbox : VectoSimulationComponent, IGearbox, ITnInPort, ITnOutPort, IClutchInfo
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

		public MeterPerSecond StartSpeed
		{
			get { return 2.SI<MeterPerSecond>(); }
		}

		public MeterPerSquareSecond StartAcceleration
		{
			get { return 0.6.SI<MeterPerSquareSecond>(); }
		}


		public void Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			if (_outPort != null) {
				if (Gear > 0) {
					return _outPort.Request(absTime, dt, torque, angularVelocity, dryRun);
				}
				return _outPort.Request(absTime, dt, 0.SI<NewtonMeter>(), null, dryRun);
			}
			throw new NotImplementedException();
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			if (_outPort != null) {
				return _outPort.Initialize(torque, angularVelocity);
			}
			throw new NotImplementedException();
		}


		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			// nothing to write
		}

		protected override void DoCommitSimulationStep() {}

		public bool ClutchClosed(Second absTime)
		{
			return true;
		}
	}
}