﻿using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Clutch : VectoSimulationComponent, IClutch, ITnOutPort, ITnInPort
	{
		private readonly RadianPerSecond _idleSpeed;
		private readonly RadianPerSecond _ratedSpeed;
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
			_ratedSpeed = engineData.GetFullLoadCurve(0).RatedSpeed();
		}

		public ClutchState State()
		{
			return _clutchState;
		}

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		public ITnInPort InShaft()
		{
			return this;
		}


		public ITnOutPort OutShaft()
		{
			return this;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond angularVelocity)
		{
			var torqueIn = torque;
			var engineSpeedIn = angularVelocity;

			if (Cockpit.Gear() == 0) {
				_clutchState = ClutchState.ClutchOpened;
				engineSpeedIn = _idleSpeed;
				torqueIn = 0.0.SI<NewtonMeter>();
			} else {
				var engineSpeedNorm = (angularVelocity - _idleSpeed) /
									(_ratedSpeed - _idleSpeed);
				if (engineSpeedNorm < CluchNormSpeed) {
					_clutchState = ClutchState.ClutchSlipping;

					var engineSpeed0 = new RadianPerSecond(Math.Max((double) _idleSpeed, (double) angularVelocity));
					var clutchSpeedNorm = CluchNormSpeed /
										((_idleSpeed + CluchNormSpeed * (_ratedSpeed - _idleSpeed)) / _ratedSpeed);
					engineSpeedIn =
						((clutchSpeedNorm * engineSpeed0 / _ratedSpeed) * (_ratedSpeed - _idleSpeed) + _idleSpeed).Radian
							.To<RadianPerSecond>();

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