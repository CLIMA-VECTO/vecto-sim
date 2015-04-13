using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
    public class Gearbox : VectoSimulationComponent, IGearbox, ITnOutPort, ITnInPort
    {
        public Gearbox(IVehicleContainer container) : base(container) {}

        #region IInShaft

        public ITnInPort InShaft()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IOutShaft

        public ITnOutPort OutShaft()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGearboxCockpit

        uint IGearboxCockpit.Gear()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITnOutPort

        IResponse ITnOutPort.Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond engineSpeed)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITnInPort

        void ITnInPort.Connect(ITnOutPort other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region VectoSimulationComponent

        public override void CommitSimulationStep(IModalDataWriter writer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}