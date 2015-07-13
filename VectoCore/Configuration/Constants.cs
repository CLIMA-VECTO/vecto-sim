using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Configuration
{
	public class Constants
	{
		public class FileExtensions
		{
			public const string ModDataFile = ".vmod";

			public const string SumFile = ".vsum";

			public const string VectoJobFile = ".vecto";

			public const string EngineDataFile = ".veng";

			public const string CycleFile = ".vdri";
		}

		public class SimulationSettings
		{
			/// <summary>
			/// base time interval for the simulation. the distance is estimated to reach this time interval as good as possible
			/// </summary>
			public static readonly Second TargetTimeInterval = 0.5.SI<Second>();

			/// <summary>
			/// simulation interval if the vehicle stands still
			/// </summary>
			public static readonly Meter DriveOffDistance = 1.SI<Meter>();
		}
	}
}