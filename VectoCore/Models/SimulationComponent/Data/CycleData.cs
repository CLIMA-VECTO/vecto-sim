using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class CycleData
	{
		/// <summary>
		/// The left data sample of the current driving cycle position. (current start point)
		/// </summary>
		public DrivingCycleData.DrivingCycleEntry LeftSample;

		/// <summary>
		/// The right data sample of the current driving cycle position. (current end point)
		/// </summary>
		public DrivingCycleData.DrivingCycleEntry RightSample;

		/// <summary>
		/// The current absolute distance in the driving cycle.
		/// </summary>
		public Meter AbsDistance;

		/// <summary>
		/// The current absolute time in the driving cycle.
		/// </summary>
		public Second AbsTime;
	}
}