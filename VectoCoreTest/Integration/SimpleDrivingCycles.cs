using System.Diagnostics.CodeAnalysis;
using System.IO;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Tests.Integration
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class SimpleDrivingCycles
	{
		#region Accelerate

		public static readonly string[] CycleAccelerate_20_60_Level = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, 0,     0",
			" 100, 60, 0,     0",
			"1000, 60, 0,     0",
		};


		public static readonly string[] CycleAccelerate_20_60_uphilll_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, 5,     0",
			" 100, 60, 5,     0",
			"1000, 60, 5,     0",
		};

		public static readonly string[] CycleAccelerate_20_60_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, -5,     0",
			" 100, 60, -5,     0",
			"1000, 60, -5,     0",
		};

		public static readonly string[] CycleAccelerate_20_60_uphill_25 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, 25,     0",
			" 100, 60, 25,     0",
			"1000, 60, 25,     0",
		};

		public static readonly string[] CycleAccelerate_20_60_downhill_25 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, -25,     0",
			" 100, 60, -25,     0",
			"1000, 60, -25,     0",
		};

		public static readonly string[] CycleAccelerate_20_60_uphill_15 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, 15,     0",
			" 100, 60, 15,     0",
			"1000, 60, 15,     0",
		};

		public static readonly string[] CycleAccelerate_20_60_downhill_15 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  20, -15,     0",
			" 100,  60, -15,     0",
			"1000,  60, -15,     0",
		};

		public static readonly string[] CycleAccelerate_0_85_level = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  0,     2",
			"1000, 85, 0,     0",
		};

		public static readonly string[] CycleAccelerate_0_85_uphill_1 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  1,     2",
			"1000, 85, 1,     0",
		};

		public static readonly string[] CycleAccelerate_0_85_uphill_2 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  2,     2",
			"1000, 85, 2,     0",
		};

		public static readonly string[] CycleAccelerate_0_85_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  5,     2",
			"1000, 85, 5,     0",
		};

		public static readonly string[] CycleAccelerate_0_85_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  0,     2",
			"  0,  85, -5,    0",
			"1000, 85, -5,    0",
		};

		public static readonly string[] CycleAccelerate_0_85_uphill_25 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  0,     2",
			"  0,  85, 25,    0",
			"1000, 85, 25,    0",
		};

		public static readonly string[] CycleAccelerate_0_85_downhill_25 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  0,     2",
			"1000, 85, -25,    0",
		};

		public static readonly string[] CycleAccelerate_0_85_uphill_10 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  10,     2",
			"1000, 85, 10,    0",
		};

		public static readonly string[] CycleAccelerate_0_85_downhill_15 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  -15,     2",
			"1000, 85, -15,    0",
		};

		public static readonly string[] CycleAccelerate_stop_0_85_level = {
			// <s>,<v>,<grad>,<stop>
			"  0,  0,  0,     2",
			"1000, 85, 0,     0",
		};

		public static readonly string[] CycleAccelerate_20_22_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  20, 5,     0",
			" 100, 22, 5,     0",
			" 200, 22, 5,     0",
		};

		#endregion

		#region Decelerate

		public static readonly string[] CycleDecelerate_22_20_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  22, -5,     0",
			" 100, 20, -5,     0",
			" 300, 20, -5,     0",
		};

		public static readonly string[] CycleDecelerate_60_20_level = {
			// <s>,<v>,<grad>,<stop>
			"  0,  60, 0,     0",
			"1000, 20, 0,     0",
			"1100, 20, 0,     0",
		};

		public static readonly string[] CycleDecelerate_45_0_level = {
			// <s>,<v>,<grad>,<stop>
			"  0,  45, 0,     0",
			" 200,  0, 0,     2",
		};

		public static readonly string[] CycleDecelerate_45_0_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  45, 5,     0",
			" 200,  0, 5,     2",
		};

		public static readonly string[] CycleDecelerate_45_0_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  45, -5,     0",
			" 200,  0, -5,     2",
		};

		public static readonly string[] CycleDecelerate_60_20_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  60, 5,     0",
			"1000, 20, 5,     0",
			"1100, 20, 5,     0"
		};

		public static readonly string[] CycleDecelerate_60_20_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  60, -5,     0",
			"1000, 20, -5,     0",
			"1100, 20, -5,     0",
		};

		public static readonly string[] CycleDecelerate_60_20_uphill_25 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  60, 25,     0",
			"1000, 20, 25,     0",
		};

		public static readonly string[] CycleDecelerate_60_20_downhill_25 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  60, -25,     0",
			"1000, 20, -25,     0",
			"1100, 20, -25,     0",
		};

		public static readonly string[] CycleDecelerate_60_20_uphill_15 = {
			// <s>,<v>,<grad>,<stop>
			"  0,  60, 15,     0",
			"1000,  0, 15,     0",
			"1100,  0, 0,     0"
		};

		public static readonly string[] CycleDecelerate_60_20_downhill_15 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,     0",
			"1000,  0,  0,     2",
		};

		public static readonly string[] CycleDecelerate_80_0_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 5,    0",
			"1000,  0,  5,    2",
//				"1000,  0,  5,    2",
		};

		public static readonly string[] CycleDecelerate_80_0_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, -5,    0",
			" 500,  0,  -5,    2",
		};

		public static readonly string[] CycleDecelerate_80_0_uphill_25 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 25,    0",
			"1000,  0,  25,    2",
		};

		public static readonly string[] CycleDecelerate_80_0_downhill_25 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, -25,  0",
			"1000,  0,  -25,  2",
		};

		public static readonly string[] CycleDecelerate_80_0_uphill_15 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 15,    0",
			"1000,  0,  15,    0",
		};

		public static readonly string[] CycleDecelerate_80_0_downhill_15 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, -15,  0",
			"1000,  0,  -15,  2",
		};

		#endregion

		#region Drive

		public static readonly string[] CycleDrive_80_level = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,    0",
			"1000,  80, 0,    0",
		};

		public static readonly string[] CycleDrive_80_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 5,    0",
			"1000,  80, 5,    0",
		};

		public static readonly string[] CycleDrive_80_downhill_5 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, -5,    0",
			" 1000, 80,  -5,   0",
		};

		public static readonly string[] CycleDrive_20_downhill_15 = {
			// <s>, <v>, <grad>, <stop>
			"   0,  20,  -15,    0",
			" 500,  20,  -25,    0",
		};

		public static readonly string[] CycleDrive_30_downhill_15 = {
			// <s>, <v>, <grad>, <stop>
			"   0,  30,  -15,    0",
			" 500,  30,  -15,    0",
		};

		public static readonly string[] CycleDrive_50_downhill_15 = {
			// <s>, <v>, <grad>, <stop>
			"   0,  50,  -15,    0",
			" 500,  50,  -15,    0",
		};

		public static readonly string[] CycleDrive_80_uphill_25 = {
			// <s>,<v>,<grad>,<stop>
			"   0, 80, 25,    0",
			" 500, 80, 25,    0",
		};

		public static readonly string[] CycleDrive_80_downhill_15 = {
			// <s>, <v>, <grad>, <stop>
			"   0,  80,  -15,    0",
			" 500,  80,  -15,    0",
		};

		public static readonly string[] CycleDrive_80_uphill_15 = {
			// <s>,<v>,<grad>,<stop>
			"   0, 80, 15,    0",
			" 500, 80, 15,    0",
		};

		public static readonly string[] CycleDrive_10_level = {
			// <s>,<v>,<grad>,<stop>
			"   0,  10, 0,    0",
			"1000,  10, 0,    0",
		};

		public static readonly string[] CycleDrive_10_uphill_5 = {
			// <s>,<v>,<grad>,<stop>
			"   0,  10, -5,    0",
			" 1000, 10,  -5,   0",
		};

		public static readonly string[] CycleDrive_10_downhill_25 = {
			// <s>, <v>, <grad>, <stop>
			"   0,  10,  -25,    0",
			" 500,  10,  -25,    0",
		};

		public static readonly string[] CycleDrive_10_uphill_25 = {
			// <s>,<v>,<grad>,<stop>
			"   0, 10, 25,    0",
			" 500, 10, 25,    0",
		};

		public static readonly string[] CycleDrive_10_downhill_15 = {
			// <s>, <v>, <grad>, <stop>
			"   0,  10,  -15,    0",
			" 500,  10,  -15,    0",
		};


		public static readonly string[] CycleDrive_10_uphill_15 = {
			// <s>,<v>,<grad>,<stop>
			"   0, 10, 15,    0",
			" 500, 10, 15,    0",
		};

		#endregion

		#region Misc

		public static readonly string[] CycleDrive_80_Increasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,    0",
			" 100,  80, 0.25, 0",
			" 200,  80, 0.5,  0",
			" 300,  80, 0.75, 0",
			" 400,  80, 1,    0",
			" 500,  80, 1.25, 0",
			" 600,  80, 1.5,  0",
			" 700,  80, 1.75, 0",
			" 800,  80, 2,    0",
			" 900,  80, 2.25, 0",
			"1000,  80, 2.5,  0",
			"1100,  80, 0,    0",
		};

		public static readonly string[] CycleDrive_50_Increasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  50, 0,    0",
			" 100,  50, 0.25, 0",
			" 200,  50, 0.5,  0",
			" 300,  50, 0.75, 0",
			" 400,  50, 1,    0",
			" 500,  50, 1.25, 0",
			" 600,  50, 1.5,  0",
			" 700,  50, 1.75, 0",
			" 800,  50, 2,    0",
			" 900,  50, 2.25, 0",
			"1000,  50, 2.5,  0",
			"1100,  50, 0,    0",
		};

		public static readonly string[] CycleDrive_30_Increasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  30, 0,    0",
			" 100,  30, 0.25, 0",
			" 200,  30, 0.5,  0",
			" 300,  30, 0.75, 0",
			" 400,  30, 1,    0",
			" 500,  30, 1.25, 0",
			" 600,  30, 1.5,  0",
			" 700,  30, 1.75, 0",
			" 800,  30, 2,    0",
			" 900,  30, 2.25, 0",
			"1000,  30, 2.5,  0",
			"1100,  30, 0,    0",
		};

		public static readonly string[] CycleDrive_80_Decreasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80,  0,    0",
			" 100,  80, -0.25, 0",
			" 200,  80, -0.5,  0",
			" 300,  80, -0.75, 0",
			" 400,  80, -1,    0",
			" 500,  80, -1.25, 0",
			" 600,  80, -1.5,  0",
			" 700,  80, -1.75, 0",
			" 800,  80, -2,    0",
			" 900,  80, -2.25, 0",
			"1000,  80, -2.5,  0",
			"1100,  80,  0,    0",
		};

		public static readonly string[] CycleDrive_50_Decreasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  50,  0,    0",
			" 100,  50, -0.25, 0",
			" 200,  50, -0.5,  0",
			" 300,  50, -0.75, 0",
			" 400,  50, -1,    0",
			" 500,  50, -1.25, 0",
			" 600,  50, -1.5,  0",
			" 700,  50, -1.75, 0",
			" 800,  50, -2,    0",
			" 900,  50, -2.25, 0",
			"1000,  50, -2.5,  0",
			"1100,  50,  0,    0",
		};

		public static readonly string[] CycleDrive_30_Decreasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  30,  0,    0",
			" 100,  30, -0.25, 0",
			" 200,  30, -0.5,  0",
			" 300,  30, -0.75, 0",
			" 400,  30, -1,    0",
			" 500,  30, -1.25, 0",
			" 600,  30, -1.5,  0",
			" 700,  30, -1.75, 0",
			" 800,  30, -2,    0",
			" 900,  30, -2.25, 0",
			"1000,  30, -2.5,  0",
			"1100,  30,  0,    0",
		};

		public static readonly string[] CycleDrive_80_Dec_Increasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80,  0,    0",
			" 100,  80, -0.25, 0",
			" 200,  80, -0.5,  0",
			" 300,  80, -0.75, 0",
			" 400,  80, -1,    0",
			" 500,  80, -1.25, 0",
			" 600,  80, -1.5,  0",
			" 700,  80, -1.75, 0",
			" 800,  80, -2,    0",
			" 900,  80, -2.25, 0",
			"1000,  80, -2.5,  0",
			"1100,  80,  0,    0",
			"1200,  80, 0,    0",
			"1300,  80, 0.25, 0",
			"1400,  80, 0.5,  0",
			"1500,  80, 0.75, 0",
			"1600,  80, 1,    0",
			"1700,  80, 1.25, 0",
			"1800,  80, 1.5,  0",
			"1900,  80, 1.75, 0",
			"2000,  80, 2,    0",
			"2100,  80, 2.25, 0",
			"2200,  80, 2.5,  0",
			"2300,  80, 0,    0",
		};

		public static readonly string[] CycleDrive_50_Dec_Increasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  50,  0,    0",
			" 100,  50, -0.25, 0",
			" 200,  50, -0.5,  0",
			" 300,  50, -0.75, 0",
			" 400,  50, -1,    0",
			" 500,  50, -1.25, 0",
			" 600,  50, -1.5,  0",
			" 700,  50, -1.75, 0",
			" 800,  50, -2,    0",
			" 900,  50, -2.25, 0",
			"1000,  50, -2.5,  0",
			"1100,  50,  0,    0",
			"1200,  50, 0,    0",
			"1300,  50, 0.25, 0",
			"1400,  50, 0.5,  0",
			"1500,  50, 0.75, 0",
			"1600,  50, 1,    0",
			"1700,  50, 1.25, 0",
			"1800,  50, 1.5,  0",
			"1900,  50, 1.75, 0",
			"2000,  50, 2,    0",
			"2100,  50, 2.25, 0",
			"2200,  50, 2.5,  0",
			"2300,  50, 5,    0",
			"2500,  50, 0,    0",
		};

		public static readonly string[] CycleDrive_30_Dec_Increasing_Slope = {
			// <s>,<v>,<grad>,<stop>
			"   0,  30,  0,    0",
			" 100,  30, -0.25, 0",
			" 200,  30, -0.5,  0",
			" 300,  30, -0.75, 0",
			" 400,  30, -1,    0",
			" 500,  30, -1.25, 0",
			" 600,  30, -1.5,  0",
			" 700,  30, -1.75, 0",
			" 800,  30, -2,    0",
			" 900,  30, -2.25, 0",
			"1000,  30, -2.5,  0",
			"1100,  30,  0,    0",
			"1200,  30, 0,    0",
			"1300,  30, 0.25, 0",
			"1400,  30, 0.5,  0",
			"1500,  30, 0.75, 0",
			"1600,  30, 1,    0",
			"1700,  30, 1.25, 0",
			"1800,  30, 1.5,  0",
			"1900,  30, 1.75, 0",
			"2000,  30, 2,    0",
			"2100,  30, 2.25, 0",
			"2200,  30, 2.5,  0",
			"2300,  30, 0,    0",
		};

		public static readonly string[] CycleDecelerateWhileBrake_80_0_level = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,    0",
			" 990,  65, 0,    0",
			"1000,   0, 0,    2",
		};

		public static readonly string[] CycleAccelerateWhileBrake_80_0_level = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,    0",
			" 800,  90, 0,    0",
			" 950,  80, 0,    0",
			"1000,   0, 0,    2",
		};

		public static readonly string[] CycleAccelerateAtBrake_80_0_level = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,    0",
			" 505,  90, 0,    0",
			" 650,  80, 0,    0",
			"1000,   0, 0,    2",
		};

		public static readonly string[] CycleAccelerateBeforeBrake_80_0_level = {
			// <s>,<v>,<grad>,<stop>
			"   0,  80, 0,    0",
			" 450,  90, 0,    0",
			" 650,  80, 0,    0",
			"1000,   0, 0,    2",
		};

		public static readonly string[] CycleDrive_stop_85_stop_85_level = {
			// <s>,<v>,<grad>,<stop>
			"  0,   0, 0,     2",
			"1000, 85, 0,     0",
			"2000,   0, 0,     2",
			"3000, 85, 0,     0",
		};

		#endregion

		public static DrivingCycleData CreateCycleData(string[] entries)
		{
			var cycleData = new MemoryStream();
			var writer = new StreamWriter(cycleData);
			writer.WriteLine("<s>,<v>,<grad>,<stop>");
			foreach (var entry in entries) {
				writer.WriteLine(entry);
			}
			writer.Flush();
			cycleData.Seek(0, SeekOrigin.Begin);
			return DrivingCycleDataReader.ReadFromStream(cycleData, CycleType.DistanceBased);
		}
	}
}