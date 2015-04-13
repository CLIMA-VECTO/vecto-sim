using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    /// <summary>
    /// Defines a connect method to connect the inport to an outport.
    /// </summary>
    public interface ITnInPort
    {
        /// <summary>
        /// Connects the inport to another outport.
        /// </summary>
        void Connect(ITnOutPort other);
    }

    /// <summary>
    /// Defines a request method for a Tn-Out-Port.
    /// </summary>
    public interface ITnOutPort
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