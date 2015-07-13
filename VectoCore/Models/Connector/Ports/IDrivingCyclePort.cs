using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an DriverDemand in port.
	/// </summary>
	public interface IDrivingCycleInProvider
	{
		/// <summary>
		/// Returns the inport to connect it to another outport.
		/// </summary>
		/// <returns></returns>
		IDrivingCycleInPort InPort();
	}

	/// <summary>
	/// Defines a method to acquire an DriverDemand out port.
	/// </summary>
	public interface IDrivingCycleOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		IDrivingCycleOutPort OutPort();
	}


	//=============================================================


	/// <summary>
	/// Defines a connect method to connect the inport to an outport.
	/// </summary>
	public interface IDrivingCycleInPort
	{
		/// <summary>
		/// Connects the inport to another outport.
		/// </summary>
		void Connect(IDrivingCycleOutPort other);
	}

	/// <summary>
	/// Defines a request method for a DriverDemand-Out-Port.
	/// </summary>
	public interface IDrivingCycleOutPort
	{
		/// <summary>
		/// Requests the Outport with the given velocity [m/s] and road gradient [rad].
		/// </summary>
		/// <param name="absTime">[s]</param>
		/// <param name="dt">[s]</param>
		/// <param name="velocity">[m/s]</param>
		/// <param name="gradient">[rad]</param>
		IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, Radian gradient);
	}
}