using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            public EngineOperationMode OperationMode { get; set; }

            /// <summary>
            /// [s]
            /// </summary>
            public TimeSpan AbsTime { get; set; }

            /// <summary>
            /// [W]
            /// </summary>
            public double EnginePower { get; set; }

            /// <summary>
            /// [rad/s]
            /// </summary>
            public double EngineSpeed { get; set; }

            /// <summary>
            /// [W]
            /// </summary>
            public double EnginePowerLoss { get; set; }

            /// <summary>
            /// [W]
            /// </summary>
            public double StationaryFullLoadPower { get; set; }

            /// <summary>
            /// [W]
            /// </summary>
            public double DynamicFullLoadPower { get; set; }

            /// <summary>
            /// [Nm]
            /// </summary>
            public double StationaryFullLoadTorque { get; set; }

            /// <summary>
            /// [Nm]
            /// </summary>
            public double DynamicFullLoadTorque { get; set; }

            /// <summary>
            /// [W]
            /// </summary>
            public double FullDragPower { get; set; }

            /// <summary>
            /// [Nm]
            /// </summary>
            public double FullDragTorque { get; set; }

            /// <summary>
            /// [Nm]
            /// </summary>
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
        /// <summary>
        /// [rad/s]
        /// </summary>
        public double EngineSpeed()
        {
            return _previousState.EngineSpeed;
        }
        #endregion

        private CombustionEngineData _data = new CombustionEngineData();
        private EngineState _previousState = new EngineState();

        /// <summary>
        /// Current state is computed in request method
        /// </summary>
        private EngineState _currentState = new EngineState();

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
            writer[ModalResultField.Pe_full] = _currentState.DynamicFullLoadPower;
            writer[ModalResultField.Pe_eng] = _currentState.EnginePower;

            writer[ModalResultField.Tq_drag] = _currentState.FullDragTorque;
            writer[ModalResultField.Tq_full] = _currentState.DynamicFullLoadTorque;
            writer[ModalResultField.Tq_eng] = _currentState.EngineTorque;
            writer[ModalResultField.n] = _currentState.EngineSpeed.
                                         SI().Radian.Per.Second.
                                         To().Rounds.Per.Minute.ScalarValue();

            try
            {
                writer[ModalResultField.FC] = _data.ConsumptionMap.GetFuelConsumption(_currentState.EngineSpeed, _currentState.EngineTorque).
                                              SI().Kilo.Gramm.Per.Second.
                                              To().Gramm.Per.Hour.ScalarValue();
            }
            catch (VectoException ex)
            {
                Log.WarnFormat("t: {0} - {1} n: {2} Tq: {3}", _currentState.AbsTime.TotalSeconds, ex.Message, _currentState.EngineSpeed, _currentState.EngineTorque);
                writer[ModalResultField.FC] = Double.NaN;
            }

            _previousState = _currentState;
            _currentState = new EngineState();
        }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            _currentState.EngineSpeed = engineSpeed;
            _currentState.AbsTime = absTime;

            var requestedPower = Formulas.TorqueToPower(torque.SI().Newton.Meter, engineSpeed.SI().Radian.Per.Second);
            _currentState.EnginePowerLoss = InertiaPowerLoss(torque.SI().Newton.Meter, engineSpeed.SI().Radian.Per.Second);
            var requestedEnginePower = requestedPower + _currentState.EnginePowerLoss.SI().Watt;

            if (engineSpeed < _data.IdleSpeed - EngineIdleSpeedStopThreshold)
            {
                _currentState.OperationMode = EngineOperationMode.Stopped;
                //todo: _currentState.EnginePowerLoss = enginePowerLoss;
            }

            var currentGear = Cockpit.Gear();

            _currentState.FullDragTorque = _data.GetFullLoadCurve(currentGear).DragLoadStationaryTorque(engineSpeed.SI().Radian.Per.Second);
            _currentState.FullDragPower = Formulas.TorqueToPower(_currentState.FullDragTorque.SI().Newton.Meter,
                                                                 engineSpeed.SI().Radian.Per.Second);

            ComputeFullLoadPower(currentGear, engineSpeed.SI().Radian.Per.Second, dt);

            ValidatePowerDemand(requestedEnginePower);

            requestedEnginePower = LimitEnginePower(requestedEnginePower);

            UpdateEngineState(requestedEnginePower);

            _currentState.EnginePower = requestedEnginePower; //todo + _currentState.EnginePowerLoss;
            _currentState.EngineTorque = Formulas.PowerToTorque(_currentState.EnginePower.SI().Watt,
                                                                _currentState.EngineSpeed.SI().Radian.Per.Second);
        }

        /// <summary>
        /// Validates the requested power demand [W].
        /// </summary>
        /// <param name="requestedEnginePower">[W]</param>
        protected void ValidatePowerDemand(SI requestedEnginePower)
        {
            Contract.Requires(requestedEnginePower.HasEqualUnit(new SI().Watt));

            if (_currentState.FullDragPower >= 0 && requestedEnginePower < 0)
            {
                throw new VectoSimulationException(String.Format("t: {0}  P_engine_drag > 0! n: {1} [1/min] ",
                    _currentState.AbsTime, _currentState.EngineSpeed.SI().To().Rounds.Per.Minute));
            }
            if (_currentState.DynamicFullLoadPower <= 0 && requestedEnginePower > 0)
            {
                throw new VectoSimulationException(String.Format("t: {0}  P_engine_full < 0! n: {1} [1/min] ",
                    _currentState.AbsTime, _currentState.EngineSpeed.SI().To().Rounds.Per.Minute));
            }
        }

        /// <summary>
        /// [W] => [W]
        /// </summary>
        /// <param name="requestedEnginePower">[W]</param>
        /// <returns>[W]</returns>
        protected SI LimitEnginePower(SI requestedEnginePower)
        {
            Contract.Requires(requestedEnginePower.HasEqualUnit(new SI().Watt));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

            if (requestedEnginePower > _currentState.DynamicFullLoadPower)
            {
                if (requestedEnginePower / _currentState.DynamicFullLoadPower > MaxPowerExceededThreshold)
                {
                    _enginePowerCorrections.Add(_currentState.AbsTime);
                    Log.WarnFormat("t: {0}  requested power > P_engine_full * 1.05 - corrected. P_request: {1}  P_engine_full: {2}", _currentState.AbsTime, requestedEnginePower, _currentState.DynamicFullLoadPower);
                    return _currentState.DynamicFullLoadPower.SI().Watt;
                }
            }
            else if (requestedEnginePower < _currentState.FullDragPower)
            {
                if (requestedEnginePower / _currentState.FullDragPower > MaxPowerExceededThreshold && requestedEnginePower > -99999)
                {
                    _enginePowerCorrections.Add(_currentState.AbsTime);
                    Log.WarnFormat("t: {0}  requested power < P_engine_drag * 1.05 - corrected. P_request: {1}  P_engine_drag: {2}", _currentState.AbsTime, requestedEnginePower, _currentState.FullDragPower);
                    return _currentState.FullDragPower.SI().Watt;
                }
            }
            return requestedEnginePower;
        }

        /// <summary>
        /// Updates the engine state dependend on the requested power [W].
        /// </summary>
        /// <param name="requestedEnginePower">[W]</param>
        protected void UpdateEngineState(SI requestedEnginePower)
        {
            Contract.Requires(requestedEnginePower.HasEqualUnit(new SI().Watt));

            if (requestedEnginePower < -ZeroThreshold)
            {
                _currentState.OperationMode = IsFullLoad(requestedEnginePower, _currentState.DynamicFullLoadPower.SI().Watt)
                    ? EngineOperationMode.FullLoad
                    : EngineOperationMode.Load;
            }
            else if (requestedEnginePower > ZeroThreshold)
            {
                _currentState.OperationMode = IsFullLoad(requestedEnginePower, _currentState.FullDragPower.SI().Watt)
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

        /// <summary>
        /// computes full load power from gear [-], angularFrequency [rad/s] and dt [s].
        /// </summary>
        /// <param name="gear"></param>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <param name="dt">[s]</param>
        protected void ComputeFullLoadPower(uint gear, SI angularFrequency, TimeSpan dt)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Requires(dt.Ticks != 0);
            // TODO @@@quam: handle dynamic timesteps
            Contract.Requires(dt.TotalSeconds.IsSmallerOrEqual(1), "simulation steps other than 1s can not be handled ATM");
            Contract.Ensures(_currentState.DynamicFullLoadPower <= _currentState.StationaryFullLoadPower);

            //_currentState.StationaryFullLoadPower = _data.GetFullLoadCurve(gear).FullLoadStationaryPower(rpm);
            _currentState.StationaryFullLoadTorque = _data.GetFullLoadCurve(gear).FullLoadStationaryTorque(angularFrequency);
            _currentState.StationaryFullLoadPower = Formulas.TorqueToPower(_currentState.StationaryFullLoadTorque.SI().Newton.Meter,
                                                                           angularFrequency);

            var pt1 = _data.GetFullLoadCurve(gear).PT1(angularFrequency);

            _currentState.DynamicFullLoadPower = Math.Min((1 / (pt1 + 1)) * (_currentState.StationaryFullLoadPower + pt1 * _previousState.EnginePower),
                                                          _currentState.StationaryFullLoadPower);
            _currentState.DynamicFullLoadTorque = Formulas.PowerToTorque(_currentState.DynamicFullLoadPower.SI().Watt, angularFrequency);
        }

        protected bool IsFullLoad(SI requestedPower, SI maxPower)
        {
            Contract.Requires(requestedPower.HasEqualUnit(new SI().Watt));
            Contract.Requires(maxPower.HasEqualUnit(new SI().Watt));

            return Math.Abs(requestedPower / maxPower - 1.0) < FullLoadMargin;
        }

        /// <summary>
        /// Calculates power loss. [W]
        /// </summary>
        /// <param name="torque">[Nm]</param>
        /// <param name="engineSpeed">[rad/s]</param>
        /// <returns>[W]</returns>
        protected SI InertiaPowerLoss(SI torque, SI engineSpeed)
        {
            Contract.Requires(torque.HasEqualUnit(new SI().Newton.Meter));
            Contract.Requires(engineSpeed.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

            var deltaEngineSpeed = engineSpeed - _previousState.EngineSpeed.SI().Radian.Per.Second;
            var avgEngineSpeed = (_previousState.EngineSpeed.SI().Radian.Per.Second + engineSpeed) / new SI(2).Second;
            var result = _data.Inertia * deltaEngineSpeed * avgEngineSpeed;
            return result;
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
