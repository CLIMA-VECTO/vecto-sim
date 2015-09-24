using System;
using System.Diagnostics;
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

		protected readonly CombustionEngineData Data;

		public CombustionEngine(IVehicleContainer cockpit, CombustionEngineData data)
			: base(cockpit)
		{
			Data = data;

			PreviousState.OperationMode = EngineOperationMode.Idle;
			PreviousState.EnginePower = 0.SI<Watt>();
			PreviousState.EngineSpeed = Data.IdleSpeed;
			PreviousState.dt = 1.SI<Second>();

			StationaryIdleFullLoadPower = Data.FullLoadCurve.FullLoadStationaryTorque(Data.IdleSpeed) * Data.IdleSpeed;
		}

		#region IEngineCockpit

		PerSecond IEngineInfo.EngineSpeed
		{
			get { return PreviousState.EngineSpeed; }
		}

		public Watt EngineStationaryFullPower(PerSecond angularSpeed)
		{
			return Data.FullLoadCurve.FullLoadStationaryTorque(angularSpeed) * angularSpeed;
		}

		public PerSecond EngineIdleSpeed
		{
			get { return Data.IdleSpeed; }
		}

		public PerSecond EngineRatedSpeed
		{
			get { return Data.FullLoadCurve.RatedSpeed; }
		}

		public ICombustionEngineIdleController GetIdleController()
		{
			return IdleController ?? new CombustionEngineIdleController(this);
		}

		protected CombustionEngineIdleController IdleController { get; set; }

		#endregion

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region ITnOutPort

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun)
		{
			return DoHandleRequest(absTime, dt, torque, angularVelocity, dryRun);
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
			CurrentState.EngineTorque = CurrentState.EnginePower / CurrentState.EngineSpeed;

			return new ResponseSuccess { EnginePowerRequest = requestedEnginePower };
		}

		protected void ComputeRequestedEnginePower(Second absTime, Second dt, NewtonMeter torque, PerSecond angularSpeed,
			out Watt requestedPower, out Watt requestedEnginePower)
		{
			CurrentState.dt = dt;
			CurrentState.EngineSpeed = angularSpeed;
			CurrentState.AbsTime = absTime;

			requestedPower = torque * angularSpeed;
			//CurrentState.EnginePowerLoss = Formulas.InertiaPower(engineSpeed, PreviousState.EngineSpeed, Data.Inertia, dt);
			CurrentState.EnginePowerLoss = InertiaPowerLoss(torque, angularSpeed);
			requestedEnginePower = requestedPower + CurrentState.EnginePowerLoss;

			if (angularSpeed < Data.IdleSpeed.Value() - EngineIdleSpeedStopThreshold) {
				CurrentState.OperationMode = EngineOperationMode.Stopped;
				//todo: _currentState.EnginePowerLoss = enginePowerLoss;
			}

			CurrentState.FullDragTorque = Data.FullLoadCurve.DragLoadStationaryTorque(angularSpeed);
			CurrentState.FullDragPower = CurrentState.FullDragTorque * angularSpeed;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularSpeed)
		{
			PreviousState = new EngineState {
				EngineSpeed = angularSpeed,
				dt = 1.SI<Second>(),
				EnginePowerLoss = 0.SI<Watt>(),
				StationaryFullLoadTorque =
					Data.FullLoadCurve.FullLoadStationaryTorque(angularSpeed),
				FullDragTorque = Data.FullLoadCurve.DragLoadStationaryTorque(angularSpeed),
				EngineTorque = torque,
				EnginePower = torque * angularSpeed,
			};
			PreviousState.StationaryFullLoadPower = PreviousState.StationaryFullLoadTorque * angularSpeed;
			PreviousState.DynamicFullLoadTorque = PreviousState.StationaryFullLoadTorque;
			PreviousState.DynamicFullLoadPower = PreviousState.StationaryFullLoadPower;
			PreviousState.FullDragPower = PreviousState.FullDragTorque * angularSpeed;

			return new ResponseSuccess { Source = this, EnginePowerRequest = PreviousState.EnginePower };
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
					Data.ConsumptionMap.GetFuelConsumption(CurrentState.EngineTorque, CurrentState.EngineSpeed)
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
				Data.FullLoadCurve.FullLoadStationaryTorque(angularVelocity);
			CurrentState.StationaryFullLoadPower = CurrentState.StationaryFullLoadTorque * angularVelocity;

			double pt1 = Data.FullLoadCurve.PT1(angularVelocity).Value();

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

			// new check in vecto 3.x (according to Martin Rexeis)
			if (CurrentState.DynamicFullLoadPower < StationaryIdleFullLoadPower) {
				CurrentState.DynamicFullLoadPower = StationaryIdleFullLoadPower;
			}

			CurrentState.DynamicFullLoadTorque = CurrentState.DynamicFullLoadPower / angularVelocity;
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
			var deltaEngineSpeed = angularVelocity - PreviousState.EngineSpeed;
			// TODO: consider simulation interval! (not simply divide by 1 Second but the current dt)
			var avgEngineSpeed = (PreviousState.EngineSpeed + angularVelocity) / 2.SI<Second>();

			var result = Data.Inertia * deltaEngineSpeed * avgEngineSpeed;
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
			return Equals(Data, other.Data)
					&& Equals(PreviousState, other.PreviousState)
					&& Equals(CurrentState, other.CurrentState);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (Data != null ? Data.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (PreviousState != null ? PreviousState.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CurrentState != null ? CurrentState.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion

		protected class CombustionEngineIdleController : ICombustionEngineIdleController
		{
			protected CombustionEngine Engine;

			protected ITnOutPort OutPort;

			public CombustionEngineIdleController(CombustionEngine combustionEngine)
			{
				Engine = combustionEngine;
			}

			public void SetRequestPort(ITnOutPort tnOutPort)
			{
				OutPort = tnOutPort;
			}

			public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity,
				bool dryRun = false)
			{
				throw new NotImplementedException();
			}

			public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
			{
				return new ResponseSuccess() { Source = this };
			}
		}
	}
}