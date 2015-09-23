using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Integration.DriverStrategy
{
	[TestClass]
	public class DriverStrategyTestCoach
	{
		#region Accelerate

		[TestInitialize]
		public void DisableLogging()
		{
			LogManager.DisableLogging();
		}

		[TestMethod]
		public void Accelerate_20_60_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_Level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_level.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphilll_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_uphill_5.vmod").Run();
		}


		[TestMethod]
		public void Accelerate_20_60_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_downhill_5.vmod").Run();
		}


		[TestMethod, Ignore]
		public void Accelerate_20_60_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_level.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_1()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_1);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_1.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_2()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_2);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_2.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_10()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_10);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_10.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_stop_0_85_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_stop_0_85_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_stop_0_85_level.vmod").Run();
		}


		[TestMethod]
		public void Accelerate_20_22_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_22_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_22_uphill_5.vmod").Run();
		}

		#endregion

		#region Decelerate

		[TestMethod]
		public void Decelerate_22_20_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_22_20_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_22_20_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_level.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_45_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_45_0_level.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_45_0_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_45_0_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_45_0_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_45_0_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_downhill_5.vmod").Run();
		}

		[TestMethod, Ignore]
		public void Decelerate_60_20_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_downhill_25.vmod").Run();
		}

		[TestMethod, Ignore]
		public void Decelerate_60_20_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, -15,     0",
				" 800, 20, -15,     0",
				"1000, 20, -15,     0",
			});
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_level.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_downhill_5.vmod").Run();
		}

		[TestMethod, Ignore]
		public void Decelerate_80_0_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_downhill_25.vmod").Run();
		}

		[TestMethod, Ignore]
		public void Decelerate_80_0_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_downhill_15.vmod").Run();
		}

		#endregion

		#region Drive

		[TestMethod]
		public void Drive_80_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_level.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_20_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_20_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_20_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_30_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_50_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_downhill_15.vmod").Run();
		}

		[TestMethod, Ignore]
		public void Drive_80_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_level.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  10, 5,    0",
				"1000,  10, 5,    0",
			});
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_5);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_25);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_15);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_uphill_15.vmod").Run();
		}

		#endregion

		#region Slope

		[TestMethod]
		public void Drive_80_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Increasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_slope_inc.vmod").Run();
		}

		[TestMethod]
		public void Drive_50_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Increasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_slope_inc.vmod").Run();
		}

		[TestMethod]
		public void Drive_30_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Increasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_slope_inc.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_Decreasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Decreasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_slope_dec.vmod").Run();
		}

		[TestMethod]
		public void Drive_50_Decreasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Decreasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_slope_dec.vmod").Run();
		}

		[TestMethod]
		public void Drive_30_Decreasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Decreasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_slope_dec.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_Dec_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Dec_Increasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_slope_dec-inc.vmod").Run();
		}

		[TestMethod]
		public void Drive_50_Dec_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Dec_Increasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_slope_dec-inc.vmod").Run();
		}


		[TestMethod]
		public void Drive_30_Dec_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Dec_Increasing_Slope);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_slope_dec-inc.vmod").Run();
		}

		#endregion

		#region Misc

		[TestMethod]
		public void DecelerateWhileBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerateWhileBrake_80_0_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod").Run();
		}

		[TestMethod]
		public void AccelerateWhileBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateWhileBrake_80_0_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod").Run();
		}

		[TestMethod]
		public void AccelerateAtBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateAtBrake_80_0_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_AccelerateAtBrake_80_0_level.vmod").Run();
		}

		[TestMethod]
		public void AccelerateBeforeBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateBeforeBrake_80_0_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod").Run();
		}

		[TestMethod]
		public void Drive_stop_85_stop_85_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_stop_85_stop_85_level);
			CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_stop_85_stop_85_level.vmod").Run();
		}

		#endregion
	}
}