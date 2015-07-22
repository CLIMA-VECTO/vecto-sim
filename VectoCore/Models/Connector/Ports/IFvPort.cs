﻿using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an Fv in port.
	/// </summary>
	public interface IFvInProvider
	{
		/// <summary>
		/// Returns the inport to connect it to another outport.
		/// </summary>
		/// <returns></returns>
		IFvInPort InPort();
	}

	/// <summary>
	/// Defines a method to acquire an Fv out port.
	/// </summary>
	public interface IFvOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		IFvOutPort OutPort();
	}

	// ==================================================


	/// <summary>
	/// Defines a connect method to connect the inport to an outport.
	/// </summary>
	public interface IFvInPort
	{
		/// <summary>
		/// Connects the inport to another outport.
		/// </summary>
		void Connect(IFvOutPort other);
	}

	/// <summary>
	/// Defines a request method for a Fv-Out-Port.
	/// </summary>
	public interface IFvOutPort
	{
		/// <summary>
		/// Requests the Outport with the given force [N] and vehicle velocity [m/s].
		/// </summary>
		/// <param name="absTime">[s]</param>
		/// <param name="dt">[s]</param>
		/// <param name="force">[N]</param>
		/// <param name="velocity">[m/s]</param>
		IResponse Request(Second absTime, Second dt, Newton force, MeterPerSecond velocity);
	}
}