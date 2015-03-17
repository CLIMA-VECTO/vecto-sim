using System;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{

    public class CombustionEngine : VectoSimulationComponent, ICombustionEngine, ITnOutPort
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
            public EngineOperationMode OperationMode { get; set; }
            public double EnginePower { get; set; }
            public double EngineSpeed { get; set; }
            public double EnginePowerLoss { get; set; }
            public TimeSpan AbsTime { get; set; }

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

        public CombustionEngine()
        {
            
        }

        public CombustionEngine(IVehicleContainer cockpit, CombustionEngineData data) : base(cockpit)
        {
            _data = data;

            _previousState.OperationMode = EngineOperationMode.Idle;
            _previousState.EnginePower = 0;
            _previousState.EngineSpeed = _data.IdleSpeed;
        }

        private CombustionEngineData _data;
        private EngineState _previousState = new EngineState();
        private EngineState _currentState = new EngineState();	// current state is computed in request method

        [NonSerialized]
        private List<TimeSpan> _enginePowerCorrections = new List<TimeSpan>();

        public ITnOutPort OutShaft()
        {
            return this;
        }

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            writer[ModalResultField.PaEng] = _currentState.EnginePowerLoss;
            _previousState = _currentState;
            _currentState = new EngineState();
        }

        public double EngineSpeed()
        {
            return _previousState.EngineSpeed;
        }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            _currentState.EngineSpeed = engineSpeed;
            _currentState.AbsTime = absTime;

            var requestedPower = VectoMath.ConvertRpmToPower(engineSpeed, torque);
            var enginePowerLoss = InertiaPowerLoss(torque, engineSpeed);
            var requestedEnginePower = requestedPower + enginePowerLoss;

            if (engineSpeed < _data.IdleSpeed - EngineIdleSpeedStopThreshold)
            {
                _currentState.OperationMode = EngineOperationMode.Stopped;
                _currentState.EnginePowerLoss = enginePowerLoss;
            }

            var currentGear = Cockpit.Gear();

            var minEnginePower = _data.GetFullLoadCurve(currentGear).DragLoadStationaryPower(engineSpeed);
            var maxEnginePower = FullLoadPowerDyamic(currentGear, engineSpeed);

            ValidatePowerDemand(requestedEnginePower, maxEnginePower, minEnginePower);

            requestedEnginePower = LimitEnginePower(requestedEnginePower, maxEnginePower, minEnginePower);

            UpdateEngineState(requestedEnginePower, maxEnginePower, minEnginePower);
        }

        protected void ValidatePowerDemand(double requestedEnginePower, double maxEnginePower, double minEnginePower)
        {
            if (minEnginePower >= 0 && requestedEnginePower < 0)
            {
                throw new VectoSimulationException(String.Format("t: {0}  P_engine_drag > 0! n: {1} [1/min] ", _currentState.AbsTime, _currentState.EngineSpeed));
            }
            if (maxEnginePower <= 0 && requestedEnginePower > 0)
            {
                throw new VectoSimulationException(String.Format("t: {0}  P_engine_full < 0! n: {1} [1/min] ", _currentState.AbsTime, _currentState.EngineSpeed));
            }
        }

        protected double LimitEnginePower(double requestedEnginePower, double maxEnginePower, double minEnginePower)
        {
            if (requestedEnginePower > maxEnginePower)
            {
                if (requestedEnginePower / maxEnginePower > MaxPowerExceededThreshold)
                {
                    _enginePowerCorrections.Add(_currentState.AbsTime);
                    Log.WarnFormat("t: {0}  requested power > P_engine_full * 1.05 - corrected. P_request: {1}  P_engine_full: {2}", _currentState.AbsTime, requestedEnginePower, maxEnginePower);
                    return maxEnginePower;
                }
            }
            else if (requestedEnginePower < minEnginePower)
            {
                if (requestedEnginePower / minEnginePower > MaxPowerExceededThreshold && requestedEnginePower > -99999)
                {
                    _enginePowerCorrections.Add(_currentState.AbsTime);
                    Log.WarnFormat("t: {0}  requested power < P_engine_drag * 1.05 - corrected. P_request: {1}  P_engine_drag: {2}", _currentState.AbsTime, requestedEnginePower, minEnginePower);
                    return minEnginePower;
                }
            }
            return requestedEnginePower;

        }

        protected void UpdateEngineState(double requestedEnginePower, double maxEnginePower, double minEnginePower)
        {
            if (requestedEnginePower < -ZeroThreshold)
            {
                _currentState.OperationMode = IsFullLoad(requestedEnginePower, maxEnginePower)
                    ? EngineOperationMode.FullLoad
                    : EngineOperationMode.Load;
            }
            else if (requestedEnginePower > ZeroThreshold)
            {
                _currentState.OperationMode = IsFullLoad(requestedEnginePower, minEnginePower)
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
    }
}
