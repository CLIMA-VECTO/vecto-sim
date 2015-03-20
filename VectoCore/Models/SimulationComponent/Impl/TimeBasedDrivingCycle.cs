using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    /// <summary>
    /// Class representing one Time Based Driving Cycle
    /// </summary>
    public class TimeBasedDrivingCycle : VectoSimulationComponent, IDrivingCycle, IDriverDemandInPort
    {
        protected TimeSpan AbsTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
        protected TimeSpan dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

        protected DrivingCycleData Data;

        private IDriverDemandOutPort OutPort { get; set; }

        private int CurrentStep { get; set; }

        public TimeBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle)
        {
            Data = cycle;
            container.AddComponent(this);
        }

        #region IDrivingCycle
        public bool DoSimulationStep()
        {
            if (Data.Entries.Count >= CurrentStep)
                return false;

            var entry = Data.Entries[CurrentStep];
            OutPort.Request(AbsTime, dt, entry.VehicleSpeed, entry.RoadGradient);
            AbsTime += dt;
            CurrentStep++;
            return true;
        }
        #endregion

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            // AbsTime gets increased before doing the CommitSimulationStep, 
            // therefore it has to be decreased again for commit. The needed time
            // for the moddata is between the last AbsTime and the current AbsTime,
            // therefore dt/2 has to be subtracted from the current AbsTime.
            // todo: document this in a jira ticket!
            var halfDt = new TimeSpan(dt.Ticks/2);
            writer[ModalResultField.time] = (AbsTime - halfDt).TotalSeconds;
        }

        public IDriverDemandInPort InPort()
        {
            return this;
        }

        public void Connect(IDriverDemandOutPort other)
        {
            OutPort = other;
        }
    }
}
