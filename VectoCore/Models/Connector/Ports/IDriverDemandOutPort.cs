using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface IDriverDemandOutPort
    {
        void Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, double gradient);
    }
}
