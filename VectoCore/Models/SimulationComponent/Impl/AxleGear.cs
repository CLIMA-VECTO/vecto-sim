using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class AxleGear : VectoSimulationComponent, IPowerTrainComponent, ITnInPort, ITnOutPort
	{
		protected ITnOutPort NextComponent;
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
			NextComponent = other;
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			bool dryRun = false)
		{
			Log.Debug("request: torque: {0}, angularVelocity: {1}", outTorque, outAngularVelocity);

			var inAngularVelocity = outAngularVelocity * _gearData.Ratio;
			var inTorque = outAngularVelocity.IsEqual(0)
				? 0.SI<NewtonMeter>()
				: _gearData.LossMap.GearboxInTorque(inAngularVelocity, outTorque);

			var retVal = NextComponent.Request(absTime, dt, inTorque, inAngularVelocity, dryRun);

			retVal.AxlegearPowerRequest = outTorque * outAngularVelocity;
			return retVal;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			var inAngularVelocity = angularVelocity * _gearData.Ratio;
			var inTorque = _gearData.LossMap.GearboxInTorque(inAngularVelocity, torque);

			return NextComponent.Initialize(inTorque, inAngularVelocity);
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