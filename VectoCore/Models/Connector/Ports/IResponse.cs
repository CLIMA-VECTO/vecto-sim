using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines an interface for a Response.
	/// </summary>
	public interface IResponse
	{
		Second SimulationInterval { get; set; }

		MeterPerSquareSecond Acceleration { get; set; }

		Meter SimulationDistance { get; set; }

		Watt EnginePowerRequest { get; set; }

		Watt ClutchPowerRequest { get; set; }

		Watt GearboxPowerRequest { get; set; }

		Watt AxlegearPowerRequest { get; set; }

		Watt WheelsPowerRequest { get; set; }

		Watt VehiclePowerRequest { get; set; }

		Watt BrakePower { get; set; }
	}
}