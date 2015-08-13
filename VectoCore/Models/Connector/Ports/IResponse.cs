using System;
using System.Security.Cryptography.X509Certificates;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	public enum ResponseType
	{
		Success,
		CycleFinished,
		FailOverload,
		FailTimeInterval,
		DrivingCycleDistanceExceeded,
		DryRun,
		GearShift
	}


	/// <summary>
	/// Defines an interface for a Response.
	/// </summary>
	public interface IResponse
	{
		Second SimulationInterval { get; set; }

		ResponseType ResponseType { get; }

		Watt EnginePowerRequest { get; set; }

		Watt ClutchPowerRequest { get; set; }

		Watt GearboxPowerRequest { get; set; }

		Watt AxlegearPowerRequest { get; set; }

		Watt WheelsPowerRequest { get; set; }

		Watt VehiclePowerRequest { get; set; }
	}
}