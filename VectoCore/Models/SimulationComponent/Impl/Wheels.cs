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

		#region IRoadPortOutProvider

		IFvOutPort IRoadPortOutProvider.OutPort()
		{
			return this;
		}

		#endregion

		#region IInShaft

		ITnInPort IInShaft.InShaft()
		{
			return this;
		}

		#endregion

		#region IFvOutPort

		IResponse IFvOutPort.Request(TimeSpan absTime, TimeSpan dt, Newton force, MeterPerSecond velocity)
		{
			NewtonMeter torque = (force * _dynamicWheelRadius).Cast<NewtonMeter>();
			var angularVelocity = (velocity / _dynamicWheelRadius).Cast<PerSecond>();
			return _outPort.Request(absTime, dt, torque, angularVelocity);
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region VectoSimulationComponent

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}