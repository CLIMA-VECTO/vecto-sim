using System;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    /// <summary>
    /// Defines a method to request the outport.
    /// </summary>
    public interface IDrivingCycleOutPort
    {
        /// <summary>
        /// Requests a demand for a specific absolute time and a time interval dt.
        /// </summary>
        /// <param name="absTime">The absolute time of the simulation.</param>
        /// <param name="dt">The current time interval.</param>
        /// <returns></returns>
        IResponse Request(TimeSpan absTime, TimeSpan dt);
    }
}