using System;
using System.Security.Cryptography.X509Certificates;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines an interface for a Response.
	/// </summary>
	public interface IResponse
	{
		TimeSpan SimulationInterval { get; set; }
	}
}