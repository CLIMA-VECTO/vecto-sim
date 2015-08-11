using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Wheels : VectoSimulationComponent, IWheels, IFvOutPort, ITnInPort
	{
		private ITnOutPort _outPort;
		private Meter _dynamicWheelRadius;

		public Wheels(IVehicleContainer cockpit, Meter rdyn)
			: base(cockpit)
		{
			_dynamicWheelRadius = rdyn;
		}

		#region IFvOutProvider

		public IFvOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion

		#region IFvOutPort

		IResponse IFvOutPort.Request(Second absTime, Second dt, Newton force, MeterPerSecond velocity, bool dryRun)
		{
			Log.DebugFormat("request: force: {0}, velocity: {1}", force, velocity);
			var torque = force * _dynamicWheelRadius;
			var angularVelocity = velocity / _dynamicWheelRadius;
			var retVal = _outPort.Request(absTime, dt, torque, angularVelocity, dryRun);
			retVal.WheelsPowerRequest = Formulas.TorqueToPower(torque, angularVelocity);
			return retVal;
		}

		public IResponse Initialize(Newton force, MeterPerSecond velocity)
		{
			var torque = force * _dynamicWheelRadius;
			var angularVelocity = velocity / _dynamicWheelRadius;

			return _outPort.Initialize(torque, angularVelocity);
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			// noting to write...
		}

		protected override void DoCommitSimulationStep()
		{
			// nothing to commit
		}

		#endregion
	}
}