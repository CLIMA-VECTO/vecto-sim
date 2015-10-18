using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Configuration
{
	public static class Constants
	{
		public static class Auxiliaries
		{
			public static class IDs
			{
				public const string Fan = "FAN";
				public const string SteeringPump = "STP";
				public const string ElectricSystem = "ES";
				public const string HeatingVentilationAirCondition = "AC";
				public const string PneumaticSystem = "PS";
			}

			public static class Names
			{
				public const string Fan = "Fan";
				public const string SteeringPump = "Steering pump";
				public const string ElectricSystem = "Electric System";
				public const string HeatingVentilationAirCondition = "HVAC";
				public const string PneumaticSystem = "Pneumatic System";
			}
		}

		public static class FileExtensions
		{
			public const string ModDataFile = ".vmod";

			public const string SumFile = ".vsum";

			public const string VectoJobFile = ".vecto";

			public const string EngineDataFile = ".veng";

			public const string CycleFile = ".vdri";
		}

		public static class SimulationSettings
		{
			/// <summary>
			/// base time interval for the simulation. the distance is estimated to reach this time interval as good as possible
			/// </summary>
			public static readonly Second TargetTimeInterval = 0.5.SI<Second>();

			/// <summary>
			/// simulation interval if the vehicle stands still
			/// </summary>
			public static readonly Meter DriveOffDistance = 1.SI<Meter>();

			public static readonly Meter BrakeNextTargetDistance = 5.SI<Meter>();

			public static readonly MeterPerSecond MinVelocityForCoast = 5.KMPHtoMeterPerSecond();

			/// <summary>
			/// threshold for changes in the road gradient. changes below this threshold will be considered to be equal for filtering out the driving cycle.
			/// altitude computation is done before filtering! 
			/// </summary>
			public static readonly Radian DrivingCycleRoadGradientTolerance = 1E-12.SI<Radian>();

			//VectoMath.InclinationToAngle(0.25 / 100.0).Value();

			public const int DriverSearchLoopThreshold = 100;

			public static readonly Watt EnginePowerSearchTolerance = 0.50.SI<Watt>(); // Watt

			public const double CluchNormSpeed = 0.03;

			public static readonly MeterPerSquareSecond MinimumAcceleration = 0.1.SI<MeterPerSquareSecond>();

			public static Meter DriverActionDistanceTolerance = 0.25.SI<Meter>();

			/// <summary>
			/// The initial search interval for the operating point search in the driver.
			/// </summary>
			public static readonly MeterPerSquareSecond OperatingPointInitialSearchIntervalAccelerating =
				0.1.SI<MeterPerSquareSecond>();

			public static readonly PerSecond EngineIdlingSearchInterval = 10.SI<PerSecond>();

			public const int EngineSearchLoopThreshold = 100;

			public const int MaximumIterationCountForSimulationStep = 30;

			public static readonly MeterPerSecond VehicleStopClutchDisengageSpeed = 10.KMPHtoMeterPerSecond();
		}
	}
}