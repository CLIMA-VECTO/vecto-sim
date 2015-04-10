using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    /// <summary>
    ///     Class representing one EngineOnly Driving Cycle
    /// </summary>
    public class EngineOnlyDrivingCycle : VectoSimulationComponent, IEngineOnlyDrivingCycle, ITnInPort,
        IDrivingCycleOutPort
    {
        protected DrivingCycleData Data;
        private ITnOutPort _outPort;

        public EngineOnlyDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
        {
            Data = cycle;
        }

        #region IInShaft

        public ITnInPort InShaft()
        {
            return this;
        }

        #endregion

        #region IDrivingCycleOutProvider

        public IDrivingCycleOutPort OutPort()
        {
            return this;
        }

        #endregion

        #region IDrivingCycleOutPort

        IResponse IDrivingCycleOutPort.Request(TimeSpan absTime, TimeSpan dt)
        {
            //todo: change to variable time steps
            var index = (int) Math.Floor(absTime.TotalSeconds);
            if (index >= Data.Entries.Count) {
                return new ResponseCycleFinished();
            }

            return _outPort.Request(absTime, dt, Data.Entries[index].EngineTorque, Data.Entries[index].EngineSpeed);
        }

        #endregion

        #region ITnInPort

        void ITnInPort.Connect(ITnOutPort other)
        {
            _outPort = other;
        }

        #endregion

        #region VectoSimulationComponent

        public override void CommitSimulationStep(IModalDataWriter writer) {}

        #endregion
    }
}