using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an DriverDemand in port.
	/// </summary>
	public interface IDriverDemandInProvider
	{
		/// <summary>
		/// Returns the inport to connect it to another outport.
		/// </summary>
		/// <returns></returns>
		IDriverDemandInPort InPort();
	}

	/// <summary>
	/// Defines a method to acquire an DriverDemand out port.
	/// </summary>
	public interface IDriverDemandOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		IDriverDemandOutPort OutPort();
	}

	//===============================================


	/// <summary>
	/// Defines a connect method to connect the inport to an outport.
	/// </summary>
	public interface IDriverDemandInPort
	{
		/// <summary>
		/// Connects the inport to another outport.
		/// </summary>
		void Connect(IDriverDemandOutPort other);
	}

	/// <summary>
	/// Defines a request method for a DriverDemand-Out-Port.
	/// </summary>
	public interface IDriverDemandOutPort
	{
		/// <summary>
		/// Requests the Outport with the given accelleration [m/s] and road gradient [rad].
		/// </summary>
		/// <param name="absTime">[s]</param>
		/// <param name="dt">[s]</param>
		/// <param name="acceleration">[m/s^2]</param>
		/// <param name="gradient">[rad]</param>
		IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond acceleration, Radian gradient);
	}
}