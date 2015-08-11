using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class AxleGear : VectoSimulationComponent, IPowerTrainComponent, ITnInPort, ITnOutPort
	{
		private ITnOutPort _nextComponent;
		private readonly GearData _gearData;

		public AxleGear(VehicleContainer container, GearData gearData) : base(container)
		{
			_gearData = gearData;
		}

		public ITnInPort InPort()
		{
			return this;
		}

		public ITnOutPort OutPort()
		{
			return this;
		}

		public void Connect(ITnOutPort other)
		{
			_nextComponent = other;
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			Log.DebugFormat("request: torque: {0}, angularVelocity: {1}", torque, angularVelocity);
			var retVal = _nextComponent.Request(absTime, dt,
				_gearData.LossMap.GearboxInTorque(angularVelocity * _gearData.Ratio, torque),
				angularVelocity * _gearData.Ratio, dryRun);
			retVal.AxlegearPowerRequest = Formulas.TorqueToPower(torque, angularVelocity);
			return retVal;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			return _nextComponent.Initialize(_gearData.LossMap.GearboxInTorque(angularVelocity * _gearData.Ratio, torque),
				angularVelocity * _gearData.Ratio);
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			// nothing to write
		}

		protected override void DoCommitSimulationStep()
		{
			// nothing to commit
		}
	}
}