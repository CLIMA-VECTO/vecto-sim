using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;
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

		private const int EngineIdleSpeedStopThreshold = 100;
		private const double MaxPowerExceededThreshold = 1.05;
		private const double ZeroThreshold = 0.0001;
		private const double FullLoadMargin = 0.01;
		[NonSerialized] private readonly List<Second> _enginePowerCorrections = new List<Second>();

		/// <summary>
		///     Current state is computed in request method
		/// </summary>
		private EngineState _currentState = new EngineState();

		private CombustionEngineData _data;
		private EngineState _previousState = new EngineState();

		public CombustionEngine(IVehicleContainer cockpit, CombustionEngineData data)
			: base(cockpit)
		{
			_data = data;

			_previousState.OperationMode = EngineOperationMode.Idle;
			_previousState.EnginePower = 0.SI<Watt>();
			_previousState.EngineSpeed = _data.IdleSpeed;
			_previousState.dt = 1.SI<Second>();
		}

		#region IEngineCockpit

		PerSecond IEngineCockpit.EngineSpeed()
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

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed)
		{
			_currentState.dt = dt;
			_currentState.EngineSpeed = engineSpeed;
			_currentState.AbsTime = absTime;

			var requestedPower = Formulas.TorqueToPower(torque, engineSpeed);
			_currentState.EnginePowerLoss = InertiaPowerLoss(torque, engineSpeed);
			var requestedEnginePower = requestedPower + _currentState.EnginePowerLoss;

			if (engineSpeed < _data.IdleSpeed.Value() - EngineIdleSpeedStopThreshold) {
				_currentState.OperationMode = EngineOperationMode.Stopped;
				//todo: _currentState.EnginePowerLoss = enginePowerLoss;
			}

			_currentState.FullDragTorque = _data.FullLoadCurve.DragLoadStationaryTorque(engineSpeed);
			_currentState.FullDragPower = Formulas.TorqueToPower(_currentState.FullDragTorque, engineSpeed);

			ComputeFullLoadPower(engineSpeed, dt);

			ValidatePowerDemand(requestedEnginePower);

			requestedEnginePower = LimitEnginePower(requestedEnginePower);

			UpdateEngineState(requestedEnginePower);

			_currentState.EnginePower = requestedEnginePower; //todo + _currentState.EnginePowerLoss;
			_currentState.EngineTorque = Formulas.PowerToTorque(_currentState.EnginePower,
				_currentState.EngineSpeed);

			//todo: use ResponseOverloadFail in case of overload
			return new ResponseSuccess();
		}

		public IResponse Initialize()
		{
			// Todo: @@@quam
			_previousState = new EngineState() {
				EngineSpeed = _data.IdleSpeed,
				EnginePower = 0.SI<Watt>(),
				dt = 1.SI<Second>(),
			};
			//_currentState
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
			writer[ModalResultField.n] = _currentState.EngineSpeed.ConvertTo().Rounds.Per.Minute;

			try {
				writer[ModalResultField.FC] =
					_data.ConsumptionMap.GetFuelConsumption(_currentState.EngineTorque, _currentState.EngineSpeed)
						.ConvertTo()
						.Gramm.Per.Hour;
			} catch (VectoException ex) {
				Log.WarnFormat("t: {0} - {1} n: {2} Tq: {3}", _currentState.AbsTime, ex.Message,
					_currentState.EngineSpeed, _currentState.EngineTorque);
				writer[ModalResultField.FC] = double.NaN;
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
		protected void ValidatePowerDemand(SI requestedEnginePower)
		{
			Contract.Requires(requestedEnginePower.HasEqualUnit(new SI().Watt));

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
		///     [W] => [W]
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		/// <returns>[W]</returns>
		protected Watt LimitEnginePower(Watt requestedEnginePower)
		{
			if (requestedEnginePower > _currentState.DynamicFullLoadPower) {
				if (requestedEnginePower / _currentState.DynamicFullLoadPower > MaxPowerExceededThreshold) {
					_enginePowerCorrections.Add(_currentState.AbsTime);
					Log.WarnFormat(
						"t: {0}  requested power > P_engine_full * 1.05 - corrected. P_request: {1}  P_engine_full: {2}",
						_currentState.AbsTime, requestedEnginePower, _currentState.DynamicFullLoadPower);
				}
				return _currentState.DynamicFullLoadPower;
			}
			if (requestedEnginePower < _currentState.FullDragPower) {
				if (requestedEnginePower / _currentState.FullDragPower > MaxPowerExceededThreshold &&
					requestedEnginePower > -99999) {
					_enginePowerCorrections.Add(_currentState.AbsTime);
					Log.WarnFormat(
						"t: {0}  requested power < P_engine_drag * 1.05 - corrected. P_request: {1}  P_engine_drag: {2}",
						_currentState.AbsTime, requestedEnginePower, _currentState.FullDragPower);
				}
				return _currentState.FullDragPower;
			}
			return requestedEnginePower;
		}

		/// <summary>
		///     Updates the engine state dependend on the requested power [W].
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		protected void UpdateEngineState(Watt requestedEnginePower)
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

		public IList<string> Warnings()
		{
			IList<string> retVal = new List<string>();
			retVal.Add(string.Format("Engine power corrected (>5%) in {0} time steps ", _enginePowerCorrections.Count));
			return retVal;
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

			_currentState.DynamicFullLoadTorque = Formulas.PowerToTorque(_currentState.DynamicFullLoadPower,
				angularVelocity);
		}

		protected bool IsFullLoad(Watt requestedPower, Watt maxPower)
		{
			var testValue = requestedPower / maxPower - 1.0;
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