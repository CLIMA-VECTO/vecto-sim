using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    /// <summary>
    /// Class representing one Time Based Driving Cycle
    /// </summary>
    public class TimeBasedDrivingCycle : VectoSimulationComponent, IDriverDemandDrivingCycle, IDriverDemandInPort
    {
        protected DrivingCycleData Data;

        private IDriverDemandOutPort OutPort { get; set; }

        public TimeBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle)
        {
            Data = cycle;
            _nextIterator = Data.Entries.AsEnumerable().GetEnumerator();
            _current = _nextIterator.Current;
            container.AddComponent(this);
        }

        private DrivingCycleData.DrivingCycleEntry _current;

        private readonly IEnumerator<DrivingCycleData.DrivingCycleEntry> _nextIterator;


        public override void CommitSimulationStep(IModalDataWriter writer)
        {
        }

        public IDriverDemandInPort InPort()
        {
            return this;
        }

        public IResponse Request(TimeSpan absTime, TimeSpan dt)
        {
            //todo: change to variable time steps
            var index = (int)Math.Floor(absTime.TotalSeconds);
            if (index >= Data.Entries.Count)
                return new ResponseCycleFinished();

            return OutPort.Request(absTime, dt, Data.Entries[index].VehicleSpeed, Data.Entries[index].RoadGradient);
        }

        public void Connect(IDriverDemandOutPort other)
        {
            OutPort = other;
        }
    }
}
