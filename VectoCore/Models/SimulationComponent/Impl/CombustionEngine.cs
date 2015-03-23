using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{

    public class CombustionEngine : VectoSimulationComponent, ICombustionEngine, ITnOutPort, IMemento
    {
        private const int EngineIdleSpeedStopThreshold = 100;
        private const double MaxPowerExceededThreshold = 1.05;
        private const double ZeroThreshold = 0.0001;
        private const double FullLoadMargin = 0.01;

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

        public class EngineState
        {
			public TimeSpan AbsTime { get; set; }
			public EngineOperationMode OperationMode { get; set; }
            public double EnginePower { get; set; }
            public double EngineSpeed { get; set; }
            public double EnginePowerLoss { get; set; }
			public double FullLoadPower { get; set; }
			public double FullLoadTorque { get; set; }
			public double FullDragPower { get; set; }
			public double FullDragTorque { get; set; }
			public double EngineTorque { get; set; }

            #region Equality members

            protected bool Equals(EngineState other)
            {
                return OperationMode == other.OperationMode
                    && EnginePower.Equals(other.EnginePower)
                    && EngineSpeed.Equals(other.EngineSpeed)
                    && EnginePowerLoss.Equals(other.EnginePowerLoss)
                    && AbsTime.Equals(other.AbsTime);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((EngineState)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (int)OperationMode;
                    hashCode = (hashCode * 397) ^ EnginePower.GetHashCode();
                    hashCode = (hashCode * 397) ^ EngineSpeed.GetHashCode();
                    hashCode = (hashCode * 397) ^ EnginePowerLoss.GetHashCode();
                    hashCode = (hashCode * 397) ^ AbsTime.GetHashCode();
                    return hashCode;
                }
            }

            #endregion
        }

        #region IEngineCockpit
        public double EngineSpeed()
        {
            return _previousState.EngineSpeed;
        }
        #endregion




        private CombustionEngineData _data = new CombustionEngineData();
        private EngineState _previousState = new EngineState();
        private EngineState _currentState = new EngineState();	// current state is computed in request method

        [NonSerialized]
        private List<TimeSpan> _enginePowerCorrections = new List<TimeSpan>();

        public CombustionEngine()
        {

        }

        public CombustionEngine(IVehicleContainer cockpit, CombustionEngineData data)
            : base(cockpit)
        {
            _data = data;

            _previousState.OperationMode = EngineOperationMode.Idle;
            _previousState.EnginePower = 0;
            _previousState.EngineSpeed = _data.IdleSpeed;
        }

        public ITnOutPort OutShaft()
        {
            return this;
        }

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            writer[ModalResultField.PaEng] = _currentState.EnginePowerLoss;
	        writer[ModalResultField.Pe_drag] = _currentState.FullDragPower;
	        writer[ModalResultField.Pe_full] = _currentState.FullLoadPower;
	        writer[ModalResultField.Pe_eng] = _currentState.EnginePower;
			
			writer[ModalResultField.Tq_drag] = _currentState.FullDragTorque;
			writer[ModalResultField.Tq_full] = _currentState.FullLoadTorque;
			writer[ModalResultField.n] = _currentState.EngineSpeed;
            _previousState = _currentState;
            _currentState = new EngineState();
        }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            _currentState.EngineSpeed = engineSpeed;
            _currentState.AbsTime = absTime;

            var requestedPower = VectoMath.ConvertRpmTorqueToPower(engineSpeed, torque);
            _currentState.EnginePowerLoss = InertiaPowerLoss(torque, engineSpeed);
            var requestedEnginePower = requestedPower + _currentState.EnginePowerLoss;

            if (engineSpeed < _data.IdleSpeed - EngineIdleSpeedStopThreshold)
            {
                _currentState.OperationMode = EngineOperationMode.Stopped;
                //_currentState.EnginePowerLoss = enginePowerLoss;
            }

            var currentGear = Cockpit.Gear();

			_currentState.FullDragTorque = _data.GetFullLoadCurve(currentGear).DragLoadStationaryTorque(engineSpeed);
			_currentState.FullDragPower = VectoMath.ConvertRpmTorqueToPower(engineSpeed, _currentState.FullDragTorque);

			//_currentState.FullLoadPower = FullLoadPowerDyamic(currentGear, engineSpeed);
			//_currentState.FullDragPower = _data.GetFullLoadCurve(currentGear).DragLoadStationaryPower(engineSpeed);

            _currentState.FullLoadPower = FullLoadPowerDyamic(currentGear, engineSpeed);

            ValidatePowerDemand(requestedEnginePower);

            requestedEnginePower = LimitEnginePower(requestedEnginePower);

            UpdateEngineState(requestedEnginePower);

	        _currentState.EnginePower = requestedEnginePower + _currentState.EnginePowerLoss;
        }

        protected void ValidatePowerDemand(double requestedEnginePower)
        {
			if (_currentState.FullDragPower >= 0 && requestedEnginePower < 0)
            {
                throw new VectoSimulationException(String.Format("t: {0}  P_engine_drag > 0! n: {1} [1/min] ", _currentState.AbsTime, _currentState.EngineSpeed));
            }
			if (_currentState.FullLoadPower <= 0 && requestedEnginePower > 0)
            {
                throw new VectoSimulationException(String.Format("t: {0}  P_engine_full < 0! n: {1} [1/min] ", _currentState.AbsTime, _currentState.EngineSpeed));
            }
        }

        protected double LimitEnginePower(double requestedEnginePower)
        {
			if (requestedEnginePower > _currentState.FullLoadPower)
            {
				if (requestedEnginePower / _currentState.FullLoadPower > MaxPowerExceededThreshold)
                {
                    _enginePowerCorrections.Add(_currentState.AbsTime);
					Log.WarnFormat("t: {0}  requested power > P_engine_full * 1.05 - corrected. P_request: {1}  P_engine_full: {2}", _currentState.AbsTime, requestedEnginePower, _currentState.FullLoadPower);
					return _currentState.FullLoadPower;
                }
            }
			else if (requestedEnginePower < _currentState.FullDragPower)
            {
				if (requestedEnginePower / _currentState.FullDragPower > MaxPowerExceededThreshold && requestedEnginePower > -99999)
                {
                    _enginePowerCorrections.Add(_currentState.AbsTime);
					Log.WarnFormat("t: {0}  requested power < P_engine_drag * 1.05 - corrected. P_request: {1}  P_engine_drag: {2}", _currentState.AbsTime, requestedEnginePower, _currentState.FullDragPower);
					return _currentState.FullDragPower;
                }
            }
            return requestedEnginePower;

        }

        protected void UpdateEngineState(double requestedEnginePower)
        {
            if (requestedEnginePower < -ZeroThreshold)
            {
				_currentState.OperationMode = IsFullLoad(requestedEnginePower, _currentState.FullLoadPower)
                    ? EngineOperationMode.FullLoad
                    : EngineOperationMode.Load;
            }
            else if (requestedEnginePower > ZeroThreshold)
            {
				_currentState.OperationMode = IsFullLoad(requestedEnginePower, _currentState.FullDragPower)
                    ? EngineOperationMode.FullDrag
                    : EngineOperationMode.Drag;
            }
            else
            {
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

        protected double FullLoadPowerDyamic(uint gear, double rpm)
        {
            var staticFullLoadPower = _data.GetFullLoadCurve(gear).FullLoadStationaryPower(rpm);
            var pt1 = _data.GetFullLoadCurve(gear).PT1(rpm);

            return Math.Min((1 / (pt1 + 1)) * (staticFullLoadPower + pt1 * _previousState.EnginePower),
                staticFullLoadPower);
        }

        protected bool IsFullLoad(double requestedPower, double maxPower)
        {
            return Math.Abs(requestedPower / maxPower - 1.0) < FullLoadMargin;
        }

        protected double InertiaPowerLoss(double torque, double engineSpeed)
        {
            var deltaEngineSpeed = engineSpeed - _previousState.EngineSpeed;
            var avgEngineSpeed = (_previousState.EngineSpeed + engineSpeed) / 2.0;
            return _data.Inertia * VectoMath.RpmTpAngularVelocity(deltaEngineSpeed) * VectoMath.RpmTpAngularVelocity(avgEngineSpeed);
        }

        #region Equality members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
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
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_data != null ? _data.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_previousState != null ? _previousState.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_currentState != null ? _currentState.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

        #region IMemento
        public string Serialize()
        {
            var mem = new { Data = _data, PreviousState = _previousState };
            return Memento.Serialize(mem);
        }

        public void Deserialize(string data)
        {
            var mem = new { Data = _data, PreviousState = _previousState };
            mem = Memento.Deserialize(data, mem);

            _data = mem.Data;
            _previousState = mem.PreviousState;
        }
        #endregion
    }
}
