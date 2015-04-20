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
		private ITnOutPort _nextComponent;

		private readonly RetarderLossMap _lossMap;

		private Watt _retarderLoss;

		public Retarder(IVehicleContainer cockpit, RetarderLossMap lossMap) : base(cockpit)
		{
			_retarderLoss = 0.SI<Watt>();
			_lossMap = lossMap;
		}

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			writer[ModalResultField.PlossRetarder] = _lossMap;
		}

		public ITnInPort InShaft()
		{
			return this;
		}

		public ITnOutPort OutShaft()
		{
			return this;
		}

		public void Connect(ITnOutPort other)
		{
			_nextComponent = other;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond angularVelocity)
		{
			var retarderTorqueLoss = _lossMap.RetarderLoss(angularVelocity);
			//_retarderLoss = Formulas.TorqueToPower(torqueLoss, angularVelocity);
			//var requestedPower = Formulas.TorqueToPower(torque, angularVelocity);
			//requestedPower += _retarderLoss;

			return _nextComponent.Request(absTime, dt, torque + retarderTorqueLoss, angularVelocity);
		}
	}
}