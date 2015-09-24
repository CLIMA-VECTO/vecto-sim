﻿using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	/// <summary>
	/// Defines a method to access shared data of the vehicle.
	/// </summary>
	public interface IVehicleInfo
	{
		/// <summary>
		/// Returns the current vehicle speed.
		/// </summary>
		/// <value></value>
		MeterPerSecond VehicleSpeed { get; }

		Kilogram VehicleMass { get; }

		Kilogram VehicleLoading { get; }

		Kilogram TotalMass { get; }
	}
}