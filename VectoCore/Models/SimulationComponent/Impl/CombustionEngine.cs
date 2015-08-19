using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	/// Component for a combustion engine.
	/// </summary>
	public class CombustionEngine : VectoSimulationComponent, ICombustionEngine, ITnOutPort
	{
		public enum EngineOperationMode
		{
			Idle,
			Drag,
			FullDrag,
			Load,
			FullLoad,
			Stopped,
			Undef
		}

		protected const int EngineIdleSpeedStopThreshold = 100;
		protected const double MaxPowerExceededThreshold = 1.05;
		protected const double ZeroThreshold = 0.0001;
		protected const double FullLoadMargin = 0.01;

		protected readonly Watt StationaryIdleFullLoadPower;

		/// <summary>
		///     Current state is computed in request method
		/// </summary>
		internal EngineState _currentState = new EngineState();

		private CombustionEngineData _data;
		internal EngineState _previousState = new EngineState();

		public CombustionEngine(IVehicleContainer cockpit, CombustionEngineData data)
			: base(cockpit)
		{
			_data = data;

			_previousState.OperationMode = EngineOperationMode.Idle;
			_previousState.EnginePower = 0.SI<Watt>();
			_previousState.EngineSpeed = _data.IdleSpeed;
			_previousState.dt = 1.SI<Second>();

			StationaryIdleFullLoadPower = Formulas.TorqueToPower(_data.FullLoadCurve.FullLoadStationaryTorque(_data.IdleSpeed),
				_data.IdleSpeed);
		}

		#region IEngineCockpit

		PerSecond IEngineInfo.EngineSpeed()
		{
			return _previousState.EngineSpeed;
		}

		#endregion

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun)
		{
			return DoHandleRequest(absTime, dt, torque, engineSpeed, dryRun);
		}

		protected virtual IResponse DoHandleRequest(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed,
			bool dryRun)
		{
			Watt requestedPower;
			Watt requestedEnginePower;
			ComputeRequestedEnginePower(absTime, dt, torque, engineSpeed, out requestedPower, out requestedEnginePower);

			ComputeFullLoadPower(engineSpeed, dt);

			ValidatePowerDemand(requestedEnginePower);

			_currentState.EnginePower = LimitEnginePower(requestedEnginePower);

			if (dryRun) {
				return new ResponseDryRun {
					EngineDeltaFullLoad = (requestedEnginePower - _currentState.DynamicFullLoadPower),
					EngineDeltaDragLoad = (requestedEnginePower - _currentState.FullDragPower),
					EnginePowerRequest = requestedEnginePower
				};
			}

			if (!_currentState.EnginePower.IsEqual(requestedEnginePower, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
				return new ResponseFailOverload {
					Delta = (requestedEnginePower - _currentState.EnginePower),
					EnginePowerRequest = requestedEnginePower
				};
			}

			UpdateEngineState(_currentState.EnginePower);

			// = requestedEnginePower; //todo + _currentState.EnginePowerLoss;
			_currentState.EngineTorque = Formulas.PowerToTorque(_currentState.EnginePower,
				_currentState.EngineSpeed);

			return new ResponseSuccess { EnginePowerRequest = requestedEnginePower };
		}

		protected void ComputeRequestedEnginePower(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed,
			out Watt requestedPower, out Watt requestedEnginePower)
		{
			_currentState.dt = dt;
			_currentState.EngineSpeed = engineSpeed;
			_currentState.AbsTime = absTime;

			requestedPower = Formulas.TorqueToPower(torque, engineSpeed);
			_currentState.EnginePowerLoss = InertiaPowerLoss(torque, engineSpeed);
			requestedEnginePower = requestedPower + _currentState.EnginePowerLoss;

			if (engineSpeed < _data.IdleSpeed.Value() - EngineIdleSpeedStopThreshold) {
				_currentState.OperationMode = EngineOperationMode.Stopped;
				//todo: _currentState.EnginePowerLoss = enginePowerLoss;
			}

			_currentState.FullDragTorque = _data.FullLoadCurve.DragLoadStationaryTorque(engineSpeed);
			_currentState.FullDragPower = Formulas.TorqueToPower(_currentState.FullDragTorque, engineSpeed);
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			_previousState = new EngineState {
				EngineSpeed = engineSpeed,
				dt = 1.SI<Second>(),
				EnginePowerLoss = 0.SI<Watt>(),
				StationaryFullLoadTorque =
					_data.FullLoadCurve.FullLoadStationaryTorque(engineSpeed),
				FullDragTorque = _data.FullLoadCurve.DragLoadStationaryTorque(engineSpeed),
				EngineTorque = torque,
				EnginePower = Formulas.TorqueToPower(torque, engineSpeed)
			};
			_previousState.StationaryFullLoadPower = Formulas.TorqueToPower(_previousState.StationaryFullLoadTorque,
				engineSpeed);
			_previousState.DynamicFullLoadTorque = _previousState.StationaryFullLoadTorque;
			_previousState.DynamicFullLoadPower = _previousState.StationaryFullLoadPower;
			_previousState.FullDragPower = Formulas.TorqueToPower(_previousState.FullDragTorque, engineSpeed);

			return new ResponseSuccess();
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.PaEng] = _currentState.EnginePowerLoss;
			writer[ModalResultField.Pe_drag] = _currentState.FullDragPower;
			writer[ModalResultField.Pe_full] = _currentState.DynamicFullLoadPower;
			writer[ModalResultField.Pe_eng] = _currentState.EnginePower;

			writer[ModalResultField.Tq_drag] = _currentState.FullDragTorque;
			writer[ModalResultField.Tq_full] = _currentState.DynamicFullLoadTorque;
			writer[ModalResultField.Tq_eng] = _currentState.EngineTorque;
			writer[ModalResultField.n] = _currentState.EngineSpeed;

			try {
				writer[ModalResultField.FCMap] =
					_data.ConsumptionMap.GetFuelConsumption(_currentState.EngineTorque, _currentState.EngineSpeed)
						.ConvertTo()
						.Gramm.Per.Hour;
			} catch (VectoException ex) {
				Log.WarnFormat("t: {0} - {1} n: {2} Tq: {3}", _currentState.AbsTime, ex.Message,
					_currentState.EngineSpeed, _currentState.EngineTorque);
				writer[ModalResultField.FCMap] = double.NaN.SI();
			}
		}

		protected override void DoCommitSimulationStep()
		{
			_previousState = _currentState;
			_currentState = new EngineState();
		}

		#endregion

		/// <summary>
		///     Validates the requested power demand [W].
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		protected virtual void ValidatePowerDemand(Watt requestedEnginePower)
		{
			if (_currentState.FullDragPower >= 0 && requestedEnginePower < 0) {
				throw new VectoSimulationException(string.Format("t: {0}  P_engine_drag > 0! n: {1} [1/min] ",
					_currentState.AbsTime, _currentState.EngineSpeed.ConvertTo().Rounds.Per.Minute));
			}
			if (_currentState.DynamicFullLoadPower <= 0 && requestedEnginePower > 0) {
				throw new VectoSimulationException(string.Format("t: {0}  P_engine_full < 0! n: {1} [1/min] ",
					_currentState.AbsTime, _currentState.EngineSpeed.ConvertTo().Rounds.Per.Minute));
			}
		}

		/// <summary>
		/// Limits the engine power to either DynamicFullLoadPower (upper bound) or FullDragPower (lower bound)
		/// </summary>
		protected virtual Watt LimitEnginePower(Watt requestedEnginePower)
		{
			return VectoMath.Limit(requestedEnginePower, _currentState.FullDragPower, _currentState.DynamicFullLoadPower);
		}

		/// <summary>
		///     Updates the engine state dependend on the requested power [W].
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		protected virtual void UpdateEngineState(Watt requestedEnginePower)
		{
			if (requestedEnginePower < -ZeroThreshold) {
				_currentState.OperationMode = IsFullLoad(requestedEnginePower, _currentState.DynamicFullLoadPower)
					? EngineOperationMode.FullLoad
					: EngineOperationMode.Load;
			} else if (requestedEnginePower > ZeroThreshold) {
				_currentState.OperationMode = IsFullLoad(requestedEnginePower, _currentState.FullDragPower)
					? EngineOperationMode.FullDrag
					: EngineOperationMode.Drag;
			} else {
				// -ZeroThreshold <= requestedEnginePower <= ZeroThreshold
				_currentState.OperationMode = EngineOperationMode.Idle;
			}
		}

		/// <summary>
		///     computes full load power from gear [-], angularVelocity [rad/s] and dt [s].
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <param name="dt">[s]</param>
		protected void ComputeFullLoadPower(PerSecond angularVelocity, Second dt)
		{
			if (dt.IsEqual(0)) {
				throw new VectoException("ComputeFullLoadPower cannot compute for simulation interval length 0.");
			}

			//_currentState.StationaryFullLoadPower = _data.GetFullLoadCurve(gear).FullLoadStationaryPower(rpm);
			_currentState.StationaryFullLoadTorque =
				_data.FullLoadCurve.FullLoadStationaryTorque(angularVelocity);
			_currentState.StationaryFullLoadPower = Formulas.TorqueToPower(_currentState.StationaryFullLoadTorque,
				angularVelocity);

			double pt1 = _data.FullLoadCurve.PT1(angularVelocity).Value();

//			var dynFullPowerCalculated = (1 / (pt1 + 1)) *
//										(_currentState.StationaryFullLoadPower + pt1 * _previousState.EnginePower);
			var tStarPrev = pt1 *
							Math.Log(1 / (1 - (_previousState.EnginePower / _currentState.StationaryFullLoadPower).Value()), Math.E)
								.SI<Second>();
			var tStar = tStarPrev + _previousState.dt;
			var dynFullPowerCalculated = _currentState.StationaryFullLoadPower * (1 - Math.Exp((-tStar / pt1).Value()));
			_currentState.DynamicFullLoadPower = (dynFullPowerCalculated < _currentState.StationaryFullLoadPower)
				? dynFullPowerCalculated
				: _currentState.StationaryFullLoadPower;

			if (_currentState.DynamicFullLoadPower < StationaryIdleFullLoadPower) {
				_currentState.DynamicFullLoadPower = StationaryIdleFullLoadPower;
			}

			_currentState.DynamicFullLoadTorque = Formulas.PowerToTorque(_currentState.DynamicFullLoadPower,
				angularVelocity);
		}

		protected bool IsFullLoad(Watt requestedPower, Watt maxPower)
		{
			var testValue = (requestedPower / maxPower).Cast<Scalar>() - 1.0;
			return testValue.Abs() < FullLoadMargin;
		}

		/// <summary>
		/// Calculates power loss. [W]
		/// </summary>
		/// <param name="torque">[Nm]</param>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[W]</returns>
		protected Watt InertiaPowerLoss(NewtonMeter torque, PerSecond angularVelocity)
		{
			var deltaEngineSpeed = angularVelocity - _previousState.EngineSpeed;
			var avgEngineSpeed = (_previousState.EngineSpeed + angularVelocity) / 2.SI<Second>();
			var result = _data.Inertia * deltaEngineSpeed * avgEngineSpeed;
			return result.Cast<Watt>();
		}

		public class EngineState
		{
			public EngineOperationMode OperationMode { get; set; }

			/// <summary>
			///     [s]
			/// </summary>
			public Second AbsTime { get; set; }

			/// <summary>
			///     [W]
			/// </summary>
			public Watt EnginePower { get; set; }

			/// <summary>
			///     [rad/s]
			/// </summary>
			public PerSecond EngineSpeed { get; set; }

			/// <summary>
			///     [W]
			/// </summary>
			public Watt EnginePowerLoss { get; set; }

			/// <summary>
			///     [W]
			/// </summary>
			public Watt StationaryFullLoadPower { get; set; }

			/// <summary>
			///     [W]
			/// </summary>
			public Watt DynamicFullLoadPower { get; set; }

			/// <summary>
			///     [Nm]
			/// </summary>
			public NewtonMeter StationaryFullLoadTorque { get; set; }

			/// <summary>
			///     [Nm]
			/// </summary>
			public NewtonMeter DynamicFullLoadTorque { get; set; }

			/// <summary>
			///     [W]
			/// </summary>
			public Watt FullDragPower { get; set; }

			/// <summary>
			///     [Nm]
			/// </summary>
			public NewtonMeter FullDragTorque { get; set; }

			/// <summary>
			///     [Nm]
			/// </summary>
			public NewtonMeter EngineTorque { get; set; }

			public Second dt { get; set; }

			#region Equality members

			protected bool Equals(EngineState other)
			{
				return OperationMode == other.OperationMode
						&& Equals(EnginePower, other.EnginePower)
						&& Equals(EngineSpeed, other.EngineSpeed)
						&& Equals(EnginePowerLoss, other.EnginePowerLoss)
						&& AbsTime.Equals(other.AbsTime);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				if (ReferenceEquals(this, obj)) {
					return true;
				}
				var other = obj as EngineState;
				return other != null && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = (int)OperationMode;
					hashCode = (hashCode * 397) ^ (EnginePower != null ? EnginePower.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (EngineSpeed != null ? EngineSpeed.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (EnginePowerLoss != null ? EnginePowerLoss.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ AbsTime.GetHashCode();
					return hashCode;
				}
			}

			#endregion
		}

		#region Equality members

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			return obj.GetType() == GetType() && Equals((CombustionEngine)obj);
		}

		protected bool Equals(CombustionEngine other)
		{
			return Equals(_data, other._data)
					&& Equals(_previousState, other._previousState)
					&& Equals(_currentState, other._currentState);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (_data != null ? _data.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_previousState != null ? _previousState.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (_currentState != null ? _currentState.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion
	}
}