using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Wheels : VectoSimulationComponent, IWheels, IFvOutPort, ITnInPort
	{
		protected ITnOutPort NextComponent;
		private readonly Meter _dynamicWheelRadius;

		protected Watt WheelsPowerRequest { get; set; }

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

		public IResponse Request(Second absTime, Second dt, Newton force, MeterPerSecond velocity, bool dryRun)
		{
			Log.Debug("request: force: {0}, velocity: {1}", force, velocity);
			var torque = force * _dynamicWheelRadius;
			var angularVelocity = velocity / _dynamicWheelRadius;
			WheelsPowerRequest = torque * angularVelocity;
			var retVal = NextComponent.Request(absTime, dt, torque, angularVelocity, dryRun);
			retVal.WheelsPowerRequest = WheelsPowerRequest;
			return retVal;
		}


		public IResponse Initialize(Newton force, MeterPerSecond velocity)
		{
			var torque = force * _dynamicWheelRadius;
			var angularVelocity = velocity / _dynamicWheelRadius;

			return NextComponent.Initialize(torque, angularVelocity);
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			NextComponent = other;
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.Pwheel] = WheelsPowerRequest;
		}

		protected override void DoCommitSimulationStep()
		{
			// nothing to commit
		}

		#endregion
	}
}