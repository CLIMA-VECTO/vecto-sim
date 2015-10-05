using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
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

		public ICombustionEngineIdleController IdleController
		{
			get { return EngineIdleController ?? (EngineIdleController = new CombustionEngineIdleController(this)); }
		}

		protected CombustionEngineIdleController EngineIdleController { get; set; }

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
			Log.Debug("Engine Power Request: torque: {0}, angularVelocity: {1}, power: {2}", torque, angularVelocity,
				torque * angularVelocity);
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
				Log.Debug("requested engine power exceeds FLD: delta: {0}", delta);
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
			CurrentState.EnginePowerLoss = Formulas.InertiaPower(angularSpeed, PreviousState.EngineSpeed, Data.Inertia, dt);
			requestedEnginePower = requestedPower + CurrentState.EnginePowerLoss;

			if (angularSpeed < Data.IdleSpeed.Value() - EngineIdleSpeedStopThreshold) {
				CurrentState.OperationMode = EngineOperationMode.Stopped;
				//todo: _currentState.EnginePowerLoss = enginePowerLoss;
			}

			CurrentState.FullDragTorque = Data.FullLoadCurve.DragLoadStationaryTorque(angularSpeed);
			CurrentState.FullDragPower = CurrentState.FullDragTorque * angularSpeed;

			Log.Debug("EnginePowerLoss: {0}", CurrentState.EnginePowerLoss);
			Log.Debug("Drag Curve: torque: {0}, power: {1}", CurrentState.FullDragTorque, CurrentState.FullDragPower);
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
					Data.ConsumptionMap.GetFuelConsumption(CurrentState.EngineTorque, CurrentState.EngineSpeed);
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

			Log.Debug("FullLoad: torque: {0}, power: {1}", CurrentState.DynamicFullLoadTorque, CurrentState.DynamicFullLoadPower);
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

		protected class CombustionEngineIdleController : LoggingObject, ICombustionEngineIdleController
		{
			protected readonly double PeDropSlope = -0.75;
			protected readonly double PeDropOffset = 1.0;

			protected CombustionEngine Engine;

			protected Second IdleStart = null;
			protected Watt LastEnginePower = null;

			public CombustionEngineIdleController(CombustionEngine combustionEngine)
			{
				Engine = combustionEngine;
			}

			public ITnOutPort RequestPort { private get; set; }

			public void Reset()
			{
				IdleStart = null;
			}

			public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity,
				bool dryRun = false)
			{
				if (angularVelocity != null) {
					throw new VectoException("IdleController can only handle idle requests, i.e. angularVelocity == null!");
				}
				if (!torque.IsEqual(0)) {
					throw new VectoException("Torque has to be 0 for idle requests!");
				}
				if (IdleStart == null) {
					IdleStart = absTime;
					LastEnginePower = Engine.PreviousState.EnginePower;
				}
				IResponse retVal = null;

				var idleTime = absTime - IdleStart + dt;
				var prevEngineSpeed = Engine.PreviousState.EngineSpeed;
				var dragLoad = Engine.Data.FullLoadCurve.DragLoadStationaryPower(prevEngineSpeed);

				var nextEnginePower = (LastEnginePower - dragLoad) * VectoMath.Max(idleTime.Value() * PeDropSlope + PeDropOffset, 0) +
									dragLoad;

				var auxDemandResponse = RequestPort.Request(absTime, dt, torque, prevEngineSpeed, true);

				var deltaEnginePower = nextEnginePower - auxDemandResponse.AuxiliariesPowerDemand;
				var deltaTorque = deltaEnginePower / prevEngineSpeed;
				var deltaAngularSpeed = (deltaTorque / Engine.Data.Inertia * dt).Cast<PerSecond>();

				var nextAngularSpeed = prevEngineSpeed;
				if (deltaAngularSpeed > 0) {
					retVal = RequestPort.Request(absTime, dt, torque, nextAngularSpeed);
					return retVal;
				}


				nextAngularSpeed = prevEngineSpeed + deltaAngularSpeed;
				if (nextAngularSpeed < Engine.Data.IdleSpeed) {
					// search for EnginePower such that nextAngularSpeed == Engine.Data.IdleSpeed
					var tmp = RequestPort.Request(absTime, dt, torque, Engine.Data.IdleSpeed);
					return tmp;
					//throw new NotImplementedException("Search for PE s.t. n2 = n_idle");
				}

				retVal = RequestPort.Request(absTime, dt, torque, nextAngularSpeed);
				retVal.Switch().
					Case<ResponseSuccess>().
					Case<ResponseUnderload>(r => {
						retVal = RequestPort.Request(absTime, dt, torque, nextAngularSpeed);
						retVal = SearchIdlingSpeed(absTime, dt, torque, nextAngularSpeed, r);
					}).
					Default(r => {
						throw new UnexpectedResponseException("searching Idling point", r);
					});

				return retVal;
			}

			private IResponse SearchIdlingSpeed(Second absTime, Second dt, NewtonMeter torque, PerSecond angularSpeed,
				ResponseUnderload responseUnderload)
			{
				var prevEngineSpeed = Engine.PreviousState.EngineSpeed;

				var searchInterval = Constants.SimulationSettings.EngineIdlingSearchInterval;
				var intervalFactor = 1.0;

				var debug = new List<dynamic>();

				var origDelta = responseUnderload.Delta;
				var delta = origDelta;
				var nextAngularSpeed = angularSpeed;

				debug.Add(new { engineSpeed = angularSpeed, searchInterval, delta });
				var retryCount = 0;
				do {
					nextAngularSpeed -= searchInterval * delta.Sign();


					var response = (ResponseDryRun)RequestPort.Request(absTime, dt, torque, nextAngularSpeed, true);
					delta = response.DeltaDragLoad;
					debug.Add(new { engineSpeed = nextAngularSpeed, searchInterval, delta });
					if (delta.IsEqual(0.SI<Watt>(), Constants.SimulationSettings.EnginePowerSearchTolerance)) {
						Log.Debug("found operating point in {0} iterations. engine speed: {1}, delta: {2}", retryCount, nextAngularSpeed,
							delta);
						return RequestPort.Request(absTime, dt, torque, nextAngularSpeed);
					}

					if (origDelta.Sign() != delta.Sign()) {
						intervalFactor = 0.5;
					}
					searchInterval *= intervalFactor;
				} while (retryCount++ < Constants.SimulationSettings.EngineSearchLoopThreshold);

				Log.Warn("Exceeded max iterations when searching for idling point!");
				Log.Warn("acceleration: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.acceleration)),
					", ".Join(debug.Slice(-6).Select(x => x.acceleration)));
				Log.Warn("exceeded: {0} ... {1}", ", ".Join(debug.Take(5).Select(x => x.delta)),
					", ".Join(debug.Slice(-6).Select(x => x.delta)));
				Log.Error("Failed to find operating point! absTime: {0}", absTime);
				throw new VectoSimulationException("Failed to find operating point!  exceeded: {0} ... {1}",
					", ".Join(debug.Take(5).Select(x => x.delta)),
					", ".Join(debug.Slice(-6).Select(x => x.delta)));
			}

			public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
			{
				return new ResponseSuccess() { Source = this };
			}
		}
	}
}