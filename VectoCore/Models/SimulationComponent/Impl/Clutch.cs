using System;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Clutch : VectoSimulationComponent, IClutch, IClutchInfo, ITnOutPort, ITnInPort
	{
		private readonly PerSecond _idleSpeed;
		private readonly PerSecond _ratedSpeed;
		private ITnOutPort _nextComponent;
		private const double ClutchEff = 1;
		private ClutchState _clutchState = SimulationComponent.ClutchState.ClutchOpened;


		public Clutch(IVehicleContainer cockpit, CombustionEngineData engineData)
			: base(cockpit)
		{
			_idleSpeed = engineData.IdleSpeed;
			_ratedSpeed = engineData.FullLoadCurve.RatedSpeed;
		}

		public ClutchState State()
		{
			return _clutchState;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			// TODO: @@@
			writer[ModalResultField.Pe_clutch] = 0.SI<Watt>();
		}

		protected override void DoCommitSimulationStep()
		{
			//todo: implement!
			//throw new NotImplementedException();
		}

		public ITnInPort InPort()
		{
			return this;
		}


		public ITnOutPort OutPort()
		{
			return this;
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			NewtonMeter torqueIn;
			PerSecond engineSpeedIn;
			AddClutchLoss(torque, angularVelocity, out torqueIn, out engineSpeedIn);

			var retVal = _nextComponent.Request(absTime, dt, torqueIn, engineSpeedIn, dryRun);
			retVal.ClutchPowerRequest = Formulas.TorqueToPower(torque, angularVelocity);
			return retVal;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			NewtonMeter torqueIn;
			PerSecond engineSpeedIn;
			AddClutchLoss(torque, angularVelocity, out torqueIn, out engineSpeedIn);

			var retVal = _nextComponent.Initialize(torqueIn, engineSpeedIn);
			return retVal;
		}

		public void Connect(ITnOutPort other)
		{
			_nextComponent = other;
		}

		private void AddClutchLoss(NewtonMeter torque, PerSecond angularVelocity, out NewtonMeter torqueIn,
			out PerSecond engineSpeedIn)
		{
			Log.DebugFormat("from Wheels: torque: {0}, angularVelocity: {1}, power {2}", torque, angularVelocity,
				Formulas.TorqueToPower(torque, angularVelocity));
			torqueIn = torque;
			engineSpeedIn = angularVelocity;

			if (DataBus.Gear() == 0) {
				_clutchState = SimulationComponent.ClutchState.ClutchOpened;
				engineSpeedIn = _idleSpeed;
				torqueIn = 0.SI<NewtonMeter>();
			} else {
				var engineSpeedNorm = (angularVelocity - _idleSpeed) /
									(_ratedSpeed - _idleSpeed);
				if (engineSpeedNorm < Constants.SimulationSettings.CluchNormSpeed) {
					_clutchState = SimulationComponent.ClutchState.ClutchSlipping;

					var engineSpeed0 = VectoMath.Max(_idleSpeed, angularVelocity);
					var clutchSpeedNorm = Constants.SimulationSettings.CluchNormSpeed /
										((_idleSpeed + Constants.SimulationSettings.CluchNormSpeed * (_ratedSpeed - _idleSpeed)) / _ratedSpeed);
					engineSpeedIn =
						((clutchSpeedNorm * engineSpeed0 / _ratedSpeed) * (_ratedSpeed - _idleSpeed) + _idleSpeed).Radian
							.Cast<PerSecond>();

					torqueIn = Formulas.PowerToTorque(Formulas.TorqueToPower(torque, angularVelocity) / ClutchEff, engineSpeedIn);
				} else {
					_clutchState = SimulationComponent.ClutchState.ClutchClosed;
				}
			}
			Log.DebugFormat("to Engine:   torque: {0}, angularVelocity: {1}, power {2}", torqueIn, engineSpeedIn,
				Formulas.TorqueToPower(torqueIn, engineSpeedIn));
		}

		public ClutchState ClutchState()
		{
			return _clutchState;
		}
	}
}