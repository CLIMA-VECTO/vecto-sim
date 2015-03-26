using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    /// <summary>
    /// Class representing one Distance Based Driving Cycle
    /// </summary>
    public class DistanceBasedDrivingCycle : VectoSimulationComponent, IDrivingCycle, IDriverDemandInPort
    {
        protected TimeSpan AbsTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
        protected TimeSpan Dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
        protected double Distance = 0;

        protected DrivingCycleData Data;

        private IDriverDemandOutPort OutPort { get; set; }

        private int CurrentStep { get; set; }

        public DistanceBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle)
        {
            Data = cycle;
            container.AddComponent(this);
        }

        #region IDrivingCycle
        public bool DoSimulationStep()
        {
            //todo: Distance calculation and comparison!!!
            throw new NotImplementedException("Distance based Cycle is not yet implemented.");
        }
        #endregion

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            // AbsTime gets increased before doing the CommitSimulationStep, 
            // therefore it has to be decreased again for commit. The needed time
            // for the moddata is between the last AbsTime and the current AbsTime,
            // therefore dt/2 has to be subtracted from the current AbsTime.
            // todo: document this in a jira ticket!
            var halfDt = new TimeSpan(Dt.Ticks / 2);
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
