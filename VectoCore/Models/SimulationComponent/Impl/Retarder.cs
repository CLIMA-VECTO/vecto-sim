using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Retarder : VectoSimulationComponent, IPowerTrainComponent, ITnInPort, ITnOutPort
	{
		protected ITnOutPort NextComponent;

		private readonly RetarderLossMap _lossMap;

		private Watt _retarderLoss;

		public Retarder(IVehicleContainer cockpit, RetarderLossMap lossMap) : base(cockpit)
		{
			_retarderLoss = 0.SI<Watt>();
			_lossMap = lossMap;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.PlossRetarder] = _retarderLoss;
		}

		protected override void DoCommitSimulationStep() {}

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

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			if (angularVelocity == null) {
				return NextComponent.Request(absTime, dt, torque, null, dryRun);
			}
			var retarderTorqueLoss = _lossMap.RetarderLoss(angularVelocity);
			_retarderLoss = retarderTorqueLoss * angularVelocity;

			return NextComponent.Request(absTime, dt, torque + retarderTorqueLoss, angularVelocity, dryRun);
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			var retarderTorqueLoss = _lossMap.RetarderLoss(angularVelocity);
			return NextComponent.Initialize(torque + retarderTorqueLoss, angularVelocity);
		}
	}
}