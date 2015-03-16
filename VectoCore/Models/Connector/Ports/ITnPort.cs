using System;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface ITnPort
    {
    }

    public interface ITnInPort : ITnPort, IInPort
    {

    }

    public interface ITnOutPort : ITnPort, IOutPort
    {
        void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed);
    }
}
