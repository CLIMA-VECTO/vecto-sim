using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Defines a method to access shared data of the driving cycle.
	/// </summary>
	/// 
	public interface IDrivingCycleCockpit
	{
		/// <summary>
		/// Returns the data samples for the current position in the cycle.
		/// </summary>
		CycleData CycleData();
	}
}