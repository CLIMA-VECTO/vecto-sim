using System;
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
		internal EngineState CurrentState = new EngineState();

		internal EngineState PreviousState = new EngineState();

		private readonly CombustionEngineData _data;

		public CombustionEngine(IVehicleContainer cockpit, CombustionEngineData data)
			: base(cockpit)
		{
			_data = data;

			PreviousState.OperationMode = EngineOperationMode.Idle;
			PreviousState.EnginePower = 0.SI<Watt>();
			PreviousState.EngineSpeed = _data.IdleSpeed;
			PreviousState.dt = 1.SI<Second>();

			StationaryIdleFullLoadPower = Formulas.TorqueToPower(_data.FullLoadCurve.FullLoadStationaryTorque(_data.IdleSpeed),
				_data.IdleSpeed);
		}

		#region IEngineCockpit

		PerSecond IEngineInfo.EngineSpeed
		{
			get { return PreviousState.EngineSpeed; }
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

			if (engineSpeed == null) {
				// TODO: clarify what to do if engine speed is undefined (clutch open)
				engineSpeed = PreviousState.EngineSpeed;
			}
			ComputeRequestedEnginePower(absTime, dt, torque, engineSpeed, out requestedPower, out requestedEnginePower);

			ComputeFullLoadPower(engineSpeed, dt);

			ValidatePowerDemand(requestedEnginePower);

			CurrentState.EnginePower = LimitEnginePower(requestedEnginePower);

			if (dryRun) {
				return new ResponseDryRun {
					DeltaFullLoad = (requestedEnginePower - CurrentState.DynamicFullLoadPower),
					DeltaDragLoad = (requestedEnginePower - CurrentState.FullDragPower),
					EnginePowerRequest = requestedEnginePower
				};
			}

			if (!CurrentState.EnginePower.IsEqual(requestedEnginePower, Constants.SimulationSettings.EnginePowerSearchTolerance)) {
				var delta = (requestedEnginePower - CurrentState.EnginePower);
				return delta > 0
					? new ResponseOverload { Delta = delta, EnginePowerRequest = requestedEnginePower, Source = this }
					: new ResponseUnderload { Delta = delta, EnginePowerRequest = requestedEnginePower, Source = this };
			}

			UpdateEngineState(CurrentState.EnginePower);

			// = requestedEnginePower; //todo + _currentState.EnginePowerLoss;
			CurrentState.EngineTorque = Formulas.PowerToTorque(CurrentState.EnginePower,
				CurrentState.EngineSpeed);

			return new ResponseSuccess { EnginePowerRequest = requestedEnginePower };
		}

		protected void ComputeRequestedEnginePower(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed,
			out Watt requestedPower, out Watt requestedEnginePower)
		{
			CurrentState.dt = dt;
			CurrentState.EngineSpeed = engineSpeed;
			CurrentState.AbsTime = absTime;

			requestedPower = Formulas.TorqueToPower(torque, engineSpeed);
			CurrentState.EnginePowerLoss = Formulas.InertiaPower(engineSpeed, PreviousState.EngineSpeed, _data.Inertia, dt);
			requestedEnginePower = requestedPower + CurrentState.EnginePowerLoss;

			if (engineSpeed < _data.IdleSpeed.Value() - EngineIdleSpeedStopThreshold) {
				CurrentState.OperationMode = EngineOperationMode.Stopped;
				//todo: _currentState.EnginePowerLoss = enginePowerLoss;
			}

			CurrentState.FullDragTorque = _data.FullLoadCurve.DragLoadStationaryTorque(engineSpeed);
			CurrentState.FullDragPower = Formulas.TorqueToPower(CurrentState.FullDragTorque, engineSpeed);
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			PreviousState = new EngineState {
				EngineSpeed = engineSpeed,
				dt = 1.SI<Second>(),
				EnginePowerLoss = 0.SI<Watt>(),
				StationaryFullLoadTorque =
					_data.FullLoadCurve.FullLoadStationaryTorque(engineSpeed),
				FullDragTorque = _data.FullLoadCurve.DragLoadStationaryTorque(engineSpeed),
				EngineTorque = torque,
				EnginePower = Formulas.TorqueToPower(torque, engineSpeed)
			};
			PreviousState.StationaryFullLoadPower = Formulas.TorqueToPower(PreviousState.StationaryFullLoadTorque,
				engineSpeed);
			PreviousState.DynamicFullLoadTorque = PreviousState.StationaryFullLoadTorque;
			PreviousState.DynamicFullLoadPower = PreviousState.StationaryFullLoadPower;
			PreviousState.FullDragPower = Formulas.TorqueToPower(PreviousState.FullDragTorque, engineSpeed);

			return new ResponseSuccess();
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.PaEng] = CurrentState.EnginePowerLoss;
			writer[ModalResultField.Pe_drag] = CurrentState.FullDragPower;
			writer[ModalResultField.Pe_full] = CurrentState.DynamicFullLoadPower;
			writer[ModalResultField.Pe_eng] = CurrentState.EnginePower;

			writer[ModalResultField.Tq_drag] = CurrentState.FullDragTorque;
			writer[ModalResultField.Tq_full] = CurrentState.DynamicFullLoadTorque;
			writer[ModalResultField.Tq_eng] = CurrentState.EngineTorque;
			writer[ModalResultField.n] = CurrentState.EngineSpeed;

			try {
				writer[ModalResultField.FCMap] =
					_data.ConsumptionMap.GetFuelConsumption(CurrentState.EngineTorque, CurrentState.EngineSpeed)
						.ConvertTo()
						.Gramm.Per.Hour;
			} catch (VectoException ex) {
				Log.Warn("t: {0} - {1} n: {2} Tq: {3}", CurrentState.AbsTime, ex.Message, CurrentState.EngineSpeed,
					CurrentState.EngineTorque);
				writer[ModalResultField.FCMap] = double.NaN.SI();
			}
		}

		protected override void DoCommitSimulationStep()
		{
			PreviousState = CurrentState;
			CurrentState = new EngineState();
		}

		#endregion

		/// <summary>
		///     Validates the requested power demand [W].
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		protected virtual void ValidatePowerDemand(Watt requestedEnginePower)
		{
			if (CurrentState.FullDragPower >= 0 && requestedEnginePower < 0) {
				throw new VectoSimulationException("P_engine_drag > 0! n: {0} [1/min] ",
					CurrentState.EngineSpeed.ConvertTo().Rounds.Per.Minute);
			}
			if (CurrentState.DynamicFullLoadPower <= 0 && requestedEnginePower > 0) {
				throw new VectoSimulationException("P_engine_full < 0! n: {0} [1/min] ",
					CurrentState.EngineSpeed.ConvertTo().Rounds.Per.Minute);
			}
		}

		/// <summary>
		/// Limits the engine power to either DynamicFullLoadPower (upper bound) or FullDragPower (lower bound)
		/// </summary>
		protected virtual Watt LimitEnginePower(Watt requestedEnginePower)
		{
			return VectoMath.Limit(requestedEnginePower, CurrentState.FullDragPower, CurrentState.DynamicFullLoadPower);
		}

		/// <summary>
		///     Updates the engine state dependend on the requested power [W].
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		protected virtual void UpdateEngineState(Watt requestedEnginePower)
		{
			if (requestedEnginePower < -ZeroThreshold) {
				CurrentState.OperationMode = IsFullLoad(requestedEnginePower, CurrentState.DynamicFullLoadPower)
					? EngineOperationMode.FullLoad
					: EngineOperationMode.Load;
			} else if (requestedEnginePower > ZeroThreshold) {
				CurrentState.OperationMode = IsFullLoad(requestedEnginePower, CurrentState.FullDragPower)
					? EngineOperationMode.FullDrag
					: EngineOperationMode.Drag;
			} else {
				// -ZeroThreshold <= requestedEnginePower <= ZeroThreshold
				CurrentState.OperationMode = EngineOperationMode.Idle;
			}
		}

		/// <summary>
		///     computes full load power from gear [-], angularVelocity [rad/s] and dt [s].
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <param name="dt">[s]</param>
		protected void ComputeFullLoadPower(PerSecond angularVelocity, Second dt)
		{
			if (dt <= 0) {
				throw new VectoException("ComputeFullLoadPower cannot compute for simulation interval length 0.");
			}

			//_currentState.StationaryFullLoadPower = _data.GetFullLoadCurve(gear).FullLoadStationaryPower(rpm);
			CurrentState.StationaryFullLoadTorque =
				_data.FullLoadCurve.FullLoadStationaryTorque(angularVelocity);
			CurrentState.StationaryFullLoadPower = Formulas.TorqueToPower(CurrentState.StationaryFullLoadTorque,
				angularVelocity);

			double pt1 = _data.FullLoadCurve.PT1(angularVelocity).Value();

//			var dynFullPowerCalculated = (1 / (pt1 + 1)) *
//										(_currentState.StationaryFullLoadPower + pt1 * _previousState.EnginePower);
			var tStarPrev = pt1 *
							Math.Log(1 / (1 - (PreviousState.EnginePower / CurrentState.StationaryFullLoadPower).Value()), Math.E)
								.SI<Second>();
			var tStar = tStarPrev + PreviousState.dt;
			var dynFullPowerCalculated = CurrentState.StationaryFullLoadPower * (1 - Math.Exp((-tStar / pt1).Value()));
			CurrentState.DynamicFullLoadPower = (dynFullPowerCalculated < CurrentState.StationaryFullLoadPower)
				? dynFullPowerCalculated
				: CurrentState.StationaryFullLoadPower;

			if (CurrentState.DynamicFullLoadPower < StationaryIdleFullLoadPower) {
				CurrentState.DynamicFullLoadPower = StationaryIdleFullLoadPower;
			}

			CurrentState.DynamicFullLoadTorque = Formulas.PowerToTorque(CurrentState.DynamicFullLoadPower,
				angularVelocity);
		}

		protected bool IsFullLoad(Watt requestedPower, Watt maxPower)
		{
			var testValue = (requestedPower / maxPower).Cast<Scalar>() - 1.0;
			return testValue.Abs() < FullLoadMargin;
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
					&& Equals(PreviousState, other.PreviousState)
					&& Equals(CurrentState, other.CurrentState);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (_data != null ? _data.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (PreviousState != null ? PreviousState.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CurrentState != null ? CurrentState.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion
	}
}