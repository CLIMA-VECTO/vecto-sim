using System;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface IDriverDemandOutPort
    {
        void Request(TimeSpan absTime, TimeSpan dt, double velocity, double gradient);
    }
}
