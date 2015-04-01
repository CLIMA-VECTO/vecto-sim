using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface ITnOutPort : ITnPort, IOutPort
    {
        /// <summary>
        /// Requests the Outport with the given torque [Nm] and angularVelocity [rad/s].
        /// </summary>
        /// <param name="absTime">[s]</param>
        /// <param name="dt">[s]</param>
        /// <param name="torque">[Nm]</param>
        /// <param name="angularVelocity">[rad/s]</param>
        IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond angularVelocity);
    }
}