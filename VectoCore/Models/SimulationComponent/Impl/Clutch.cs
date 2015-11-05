using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Clutch : VectoSimulationComponent, IClutch, ITnOutPort, ITnInPort
	{
		private readonly PerSecond _idleSpeed;
		private readonly PerSecond _ratedSpeed;
		protected ITnOutPort NextComponent;
		private const double ClutchEff = 1;
		private ClutchState _clutchState = ClutchState.ClutchSlipping;

		protected ICombustionEngineIdleController IdleController;
		private Watt _RequiredPower;
		private NewtonMeter _RequiredTorque;


		public Clutch(IVehicleContainer cockpit, CombustionEngineData engineData,
			ICombustionEngineIdleController idleController)
			: base(cockpit)
		{
			_idleSpeed = engineData.IdleSpeed;
			_ratedSpeed = engineData.FullLoadCurve.RatedSpeed;
			IdleController = idleController;
		}

		public ClutchState State()
		{
			return _clutchState;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.Pe_clutch] = _RequiredPower;
			writer[ModalResultField.Tq_clutch] = _RequiredTorque;
		}

		protected override void DoCommitSimulationStep()
		{
			_RequiredPower = 0.SI<Watt>();
			_RequiredTorque = 0.SI<NewtonMeter>();
		}

		public ITnInPort InPort()
		{
			return this;
		}


		public ITnOutPort OutPort()
		{
			return this;
		}

		public ITnOutPort IdleControlPort
		{
			get { return NextComponent; }
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			if (angularVelocity == null) {
				Log.Debug("Invoking IdleController...");

				_RequiredPower = 0.SI<Watt>();
				_RequiredTorque = 0.SI<NewtonMeter>();
				var retval = IdleController.Request(absTime, dt, torque, null, dryRun);
				retval.ClutchPowerRequest = _RequiredPower;
				return retval;
			}
			if (IdleController != null) {
				IdleController.Reset();
			}
			NewtonMeter torqueIn;
			PerSecond angularVelocityIn;
			AddClutchLoss(torque, angularVelocity, out torqueIn, out angularVelocityIn);
			_RequiredPower = torqueIn * angularVelocityIn;
			_RequiredTorque = torqueIn;

			var retVal = NextComponent.Request(absTime, dt, torqueIn, angularVelocityIn, dryRun);
			retVal.ClutchPowerRequest = torque * angularVelocity;
			return retVal;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			NewtonMeter torqueIn;
			PerSecond engineSpeedIn;
			AddClutchLoss(torque, angularVelocity, out torqueIn, out engineSpeedIn);

			var retVal = NextComponent.Initialize(torqueIn, engineSpeedIn);
			retVal.ClutchPowerRequest = torque * angularVelocity;
			return retVal;
		}

		public void Connect(ITnOutPort other)
		{
			NextComponent = other;
		}

		private void AddClutchLoss(NewtonMeter torque, PerSecond angularVelocity, out NewtonMeter torqueIn,
			out PerSecond engineSpeedIn)
		{
			Log.Debug("from Wheels: torque: {0}, angularVelocity: {1}, power {2}", torque, angularVelocity,
				Formulas.TorqueToPower(torque, angularVelocity));
			torqueIn = torque;
			engineSpeedIn = angularVelocity;

			if (DataBus.VehicleStopped) {
				_clutchState = ClutchState.ClutchOpened;
				engineSpeedIn = _idleSpeed;
				torqueIn = 0.SI<NewtonMeter>();
			} else {
				var engineSpeedNorm = (angularVelocity - _idleSpeed) /
									(_ratedSpeed - _idleSpeed);
				if (engineSpeedNorm < Constants.SimulationSettings.CluchNormSpeed) {
					_clutchState = ClutchState.ClutchSlipping;

					var engineSpeed0 = VectoMath.Max(_idleSpeed, angularVelocity);
					var clutchSpeedNorm = Constants.SimulationSettings.CluchNormSpeed /
										((_idleSpeed + Constants.SimulationSettings.CluchNormSpeed * (_ratedSpeed - _idleSpeed)) / _ratedSpeed);
					engineSpeedIn =
						((clutchSpeedNorm * engineSpeed0 / _ratedSpeed) * (_ratedSpeed - _idleSpeed) + _idleSpeed).Radian
							.Cast<PerSecond>();

					torqueIn = ((torque * angularVelocity) / ClutchEff / engineSpeedIn);
				} else {
					_clutchState = ClutchState.ClutchClosed;
				}
			}
			Log.Debug("to Engine:   torque: {0}, angularVelocity: {1}, power {2}", torqueIn, engineSpeedIn,
				Formulas.TorqueToPower(torqueIn, engineSpeedIn));
		}
	}
}