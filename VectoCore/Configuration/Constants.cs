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

			/// <summary>
			/// threshold for changes in the road gradient. changes below this threshold will be considered to be equal for filtering out the driving cycle.
			/// altitude computation is done before filtering! 
			/// </summary>
			public static readonly double DrivingCycleRoadGradientTolerance = 0;

			//VectoMath.InclinationToAngle(0.25 / 100.0).Value();

			public const int DriverSearchLoopThreshold = 100;

			public const double EngineFLDPowerTolerance = 0.50; // Watt
		}
	}
}