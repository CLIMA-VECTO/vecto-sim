using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface IDriverDemandOutPort
    {
        IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, double gradient);
    }
}
