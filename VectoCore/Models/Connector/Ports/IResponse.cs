using System;
using System.Security.Cryptography.X509Certificates;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	public enum ResponseType
	{
		Success,
		CycleFinished,
		FailOverload,
		FailTimeInterval,
		DrivingCycleDistanceExceeded,
	}


	/// <summary>
	/// Defines an interface for a Response.
	/// </summary>
	public interface IResponse
	{
		TimeSpan SimulationInterval { get; set; }

		ResponseType ResponseType { get; }
	}
}