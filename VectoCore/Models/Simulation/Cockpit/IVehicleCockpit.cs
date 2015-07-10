using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Cockpit
{
	/// <summary>
	/// Defines a method to access shared data of the vehicle.
	/// </summary>
	public interface IVehicleCockpit
	{
		/// <summary>
		/// Returns the current vehicle speed.
		/// </summary>
		/// <returns></returns>
		MeterPerSecond VehicleSpeed();

		Kilogram VehicleMass();

		Kilogram VehicleLoading();

		Kilogram TotalMass();
	}
}