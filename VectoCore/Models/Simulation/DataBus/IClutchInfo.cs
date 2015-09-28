using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{

	public interface IClutchInfo
	{
		/// <summary>
		/// Returns if the clutch is closed in the current interval.
		/// </summary>
		bool ClutchClosed(Second absTime);
	}
}