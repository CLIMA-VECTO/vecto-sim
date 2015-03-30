using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    public class Gearbox : VectoSimulationComponent, IGearbox, ITnOutPort
    {
        public Gearbox() { }

        public Gearbox(IVehicleContainer container): base(container)
        {

        }


        public ITnInPort InShaft()
        {
            throw new NotImplementedException();
        }

        public ITnOutPort OutShaft()
        {
            throw new NotImplementedException();
        }

        public void Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond engineSpeed)
        {
            throw new NotImplementedException();
        }

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            throw new NotImplementedException();
        }

        public uint Gear()
        {
            throw new NotImplementedException();
        }
    }
}

