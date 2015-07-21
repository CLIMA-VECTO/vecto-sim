using System;
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
		private ITnOutPort _nextComponent;
		private const double ClutchEff = 1;
		private const double CluchNormSpeed = 0.03;
		private ClutchState _clutchState = ClutchState.ClutchClosed;

		public enum ClutchState
		{
			ClutchClosed,
			ClutchOpened,
			ClutchSlipping
		}

		public Clutch(IVehicleContainer cockpit, CombustionEngineData engineData)
			: base(cockpit)
		{
			_idleSpeed = engineData.IdleSpeed;
			_ratedSpeed = engineData.GetFullLoadCurve(0).RatedSpeed;
		}

		public ClutchState State()
		{
			return _clutchState;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		protected override void DoCommitSimulationStep()
		{
			//todo: implement!
			throw new NotImplementedException();
		}

		public ITnInPort InPort()
		{
			return this;
		}


		public ITnOutPort OutPort()
		{
			return this;
		}

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity)
		{
			var torqueIn = torque;
			var engineSpeedIn = angularVelocity;

			if (Cockpit.Gear() == 0) {
				_clutchState = ClutchState.ClutchOpened;
				engineSpeedIn = _idleSpeed;
				torqueIn = 0.SI<NewtonMeter>();
			} else {
				var engineSpeedNorm = (angularVelocity - _idleSpeed) /
									(_ratedSpeed - _idleSpeed);
				if (engineSpeedNorm < CluchNormSpeed) {
					_clutchState = ClutchState.ClutchSlipping;

					var engineSpeed0 = VectoMath.Max(_idleSpeed, angularVelocity);
					var clutchSpeedNorm = CluchNormSpeed /
										((_idleSpeed + CluchNormSpeed * (_ratedSpeed - _idleSpeed)) / _ratedSpeed);
					engineSpeedIn =
						((clutchSpeedNorm * engineSpeed0 / _ratedSpeed) * (_ratedSpeed - _idleSpeed) + _idleSpeed).Radian
							.Cast<PerSecond>();

					torqueIn = Formulas.PowerToTorque(Formulas.TorqueToPower(torque, angularVelocity) / ClutchEff, engineSpeedIn);
				} else {
					_clutchState = ClutchState.ClutchClosed;
				}
			}

			return _nextComponent.Request(absTime, dt, torqueIn, engineSpeedIn);
		}

		public void Connect(ITnOutPort other)
		{
			_nextComponent = other;
		}
	}
}