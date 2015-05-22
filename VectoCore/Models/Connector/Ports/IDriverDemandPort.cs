﻿using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
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
		/// <param name="accelleration">[m/s^2]</param>
		/// <param name="gradient">[rad]</param>
		IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond accelleration, Radian gradient);
	}
}