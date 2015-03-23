using System;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface ITnOutPort : ITnPort, IOutPort
    {
        void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed);
    }
}