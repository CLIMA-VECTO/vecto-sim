using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an DriverCycle Demand out port.
	/// </summary>
	public interface ISimulationOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		ISimulationOutPort OutPort();
	}

	//========================================================================

	/// <summary>
	/// Defines a method to request the outport.
	/// </summary>
	public interface ISimulationOutPort
	{
		/// <summary>
		/// Requests a demand for a specific absolute time and a time interval dt.
		/// </summary>
		/// <param name="absTime">The absolute time of the simulation.</param>
		/// <param name="ds"></param>
		/// <returns></returns>
		IResponse Request(TimeSpan absTime, Meter ds);

		IResponse Request(TimeSpan absTime, TimeSpan dt);

		IResponse Initialize();
	}
}