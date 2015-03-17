using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class EngineOnlyTimeBasedVectoJob : TimeBasedVectoJob
    {
        private List<EngineOnlyDrivingCycle> _cycles;
        private IModalDataWriter _dataWriter;

        public EngineOnlyTimeBasedVectoJob(VehicleContainer container, List<EngineOnlyDrivingCycle> cycles, IModalDataWriter dataWriter)
            : base(container)
        {
            _cycles = cycles;
            _dataWriter = dataWriter;
        }

        public override void Run()
        {
            var port = Container.GetEngineOnlyStartPort();
            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            foreach (var cycle in _cycles)
            {
                port.Request(absTime, dt, cycle.Torque, cycle.EngineSpeed);
                Container.CommitSimulationStep(_dataWriter);

                absTime += dt;
            }
            Container.FinishSimulation(_dataWriter);
        }
    }
}