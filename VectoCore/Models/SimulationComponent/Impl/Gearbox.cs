using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    public class Gearbox : VectoSimulationComponent, IGearbox, ITnOutPort
    {
        public class Gear
        {

        }

        public Gear AxleGear { get; set; }

        public IEnumerable<Gear> Gears { get; set; }

        public ITnInPort InShaft()
        {
            throw new NotImplementedException();
        }

        public ITnOutPort OutShaft()
        {
            throw new NotImplementedException();
        }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            throw new NotImplementedException();
        }

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}

