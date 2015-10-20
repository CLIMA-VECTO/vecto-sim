using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	public enum DrivingBehavior
	{
		Halted,
		Accelerating,
		Driving,
		Coasting,
		Braking,
	}

	public interface IDriverInfo
	{
		bool VehicleStopped { get; }

		DrivingBehavior DrivingBehavior { get; }
	}
}