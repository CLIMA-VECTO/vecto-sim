using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Integration.DriverStrategy
{
	[TestClass]
	public class DriverStrategyTestTruck
	{
		#region Accelerate

		[TestInitialize]
		public void DisableLogging()
		{
			//LogManager.DisableLogging();
		}

		[TestMethod]
		public void Accelerate_20_60_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_Level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_level.vmod");
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphilll_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_uphill_5.vmod");
		}


		[TestMethod]
		public void Accelerate_20_60_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_downhill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_downhill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_downhill_5.vmod");
		}


		[TestMethod, Ignore]
		public void Accelerate_20_60_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_uphill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_uphill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_uphill_25.vmod");
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_downhill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_downhill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_downhill_25.vmod");
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_uphill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_uphill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_uphill_15.vmod");
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_downhill_15.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_level.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_1()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_1);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_uphill_1.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_1.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_1.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_2()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_2);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_uphill_2.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_2.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_2.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_5.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_downhill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_5.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_uphill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_25.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_downhill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_25.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_10()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_10);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_uphill_10.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_10.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_10.vmod");
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_15.vmod");
		}

		[TestMethod]
		public void Accelerate_stop_0_85_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_stop_0_85_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_stop_0_85_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_stop_0_85_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_stop_0_85_level.vmod");
		}


		[TestMethod]
		public void Accelerate_20_22_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_22_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_22_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_22_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_22_uphill_5.vmod");
		}

		#endregion

		#region Decelerate

		[TestMethod]
		public void Decelerate_22_20_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_22_20_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_22_20_downhill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_22_20_downhill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_22_20_downhill_5.vmod");
		}

		[TestMethod]
		public void Decelerate_60_20_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_level.vmod");
		}

		[TestMethod]
		public void Decelerate_45_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_45_0_level.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Decelerate_45_0_level.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_45_0_level.vmod");
		}

		[TestMethod]
		public void Decelerate_45_0_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_45_0_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_45_0_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_45_0_uphill_5.vmod");
		}

		[TestMethod]
		public void Decelerate_45_0_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_45_0_downhill_5.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Decelerate_45_0_downhill_5.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_45_0_downhill_5.vmod");
		}

		[TestMethod]
		public void Decelerate_60_20_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_uphill_5.vmod");
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_downhill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_downhill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_downhill_5.vmod");
		}

		[TestMethod, Ignore]
		public void Decelerate_60_20_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_uphill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_uphill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_uphill_25.vmod");
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_downhill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_downhill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_downhill_25.vmod");
		}

		[TestMethod, Ignore]
		public void Decelerate_60_20_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_uphill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_uphill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_uphill_15.vmod");
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
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_downhill_15.vmod");
		}

		[TestMethod]
		public void Decelerate_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_level.vmod");
		}

		[TestMethod]
		public void Decelerate_80_0_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_uphill_5.vmod");
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_downhill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_downhill_5.vmod");
		}

		[TestMethod, Ignore]
		public void Decelerate_80_0_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_steep_uphill_25.vmod");
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_downhill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_downhill_25.vmod");
		}

		[TestMethod, Ignore]
		public void Decelerate_80_0_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_steep_uphill_15.vmod");
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_downhill_15.vmod");
		}

		#endregion

		#region Drive

		[TestMethod]
		public void Drive_80_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_80_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_level.vmod");
		}

		[TestMethod]
		public void Drive_80_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_uphill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_80_uphill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_uphill_5.vmod");
		}

		[TestMethod]
		public void Drive_80_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_downhill_5.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_80_downhill_5.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_downhill_5.vmod");
		}

		[TestMethod]
		public void Drive_20_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_20_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_20_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_20_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_20_downhill_15.vmod");
		}

		[TestMethod]
		public void Drive_30_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_30_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_downhill_15.vmod");
		}

		[TestMethod]
		public void Drive_50_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_50_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_downhill_15.vmod");
		}

		[TestMethod, Ignore]
		public void Drive_80_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_uphill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_80_uphill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_uphill_25.vmod");
		}

		[TestMethod]
		public void Drive_80_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_80_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_downhill_15.vmod");
		}

		[TestMethod]
		public void Drive_80_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_uphill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_80_uphill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_uphill_15.vmod");
		}

		[TestMethod]
		public void Drive_10_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_level.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_10_level.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_level.vmod");
		}

		[TestMethod]
		public void Drive_10_uphill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  10, 5,    0",
				"1000,  10, 5,    0",
			});
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_uphill_5.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_10_uphill_5.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_uphill_5.vmod");
		}

		[TestMethod]
		public void Drive_10_downhill_5()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_downhill_5.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_10_downhill_5.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_downhill_5.vmod");
		}

		[TestMethod]
		public void Drive_10_downhill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_downhill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_10_downhill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_downhill_25.vmod");
		}

		[TestMethod]
		public void Drive_10_uphill_25()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_25);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_uphill_25.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_10_uphill_25.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_uphill_25.vmod");
		}

		[TestMethod]
		public void Drive_10_downhill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_downhill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_10_downhill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_downhill_15.vmod");
		}

		[TestMethod]
		public void Drive_10_uphill_15()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_15);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_uphill_15.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_10_uphill_15.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_uphill_15.vmod");
		}

		#endregion

		#region Slope

		[TestMethod]
		public void Drive_80_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Increasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_slope_inc.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_80_slope_inc.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_slope_inc.vmod");
		}

		[TestMethod]
		public void Drive_50_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Increasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_slope_inc.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_50_slope_inc.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_slope_inc.vmod");
		}

		[TestMethod]
		public void Drive_30_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Increasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_slope_inc.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_30_slope_inc.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_slope_inc.vmod");
		}

		[TestMethod]
		public void Drive_80_Decreasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Decreasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_slope_dec.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_80_slope_dec.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_slope_dec.vmod");
		}

		[TestMethod]
		public void Drive_50_Decreasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Decreasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_slope_dec.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_50_slope_dec.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_slope_dec.vmod");
		}

		[TestMethod]
		public void Drive_30_Decreasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Decreasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_slope_dec.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_30_slope_dec.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_slope_dec.vmod");
		}

		[TestMethod]
		public void Drive_80_Dec_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Dec_Increasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_slope_dec-inc.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_80_slope_dec-inc.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_slope_dec-inc.vmod");
		}

		[TestMethod]
		public void Drive_50_Dec_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Dec_Increasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_slope_dec-inc.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_50_slope_dec-inc.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_slope_dec-inc.vmod");
		}


		[TestMethod]
		public void Drive_30_Dec_Increasing_Slope()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Dec_Increasing_Slope);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_slope_dec-inc.vmod").Run();

			//GraphWriter.Write("Truck_DriverStrategy_Drive_30_slope_dec-inc.vmod",
			//	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_slope_dec-inc.vmod");
		}

		#endregion

		#region Misc

		[TestMethod]
		public void DecelerateWhileBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerateWhileBrake_80_0_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_DecelerateWhileBrake_80_0_level.vmod");
		}

		[TestMethod]
		public void AccelerateWhileBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateWhileBrake_80_0_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_AccelerateWhileBrake_80_0_level.vmod");
		}

		[TestMethod]
		public void AccelerateAtBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateAtBrake_80_0_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_AccelerateAtBrake_80_0_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_AccelerateAtBrake_80_0_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_AccelerateAtBrake_80_0_level.vmod");
		}

		[TestMethod]
		public void AccelerateBeforeBrake_80_0_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateBeforeBrake_80_0_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_AccelerateBeforeBrake_80_0_level.vmod");
		}

		[TestMethod]
		public void Drive_stop_85_stop_85_level()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_stop_85_stop_85_level);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_stop_85_stop_85_level.vmod").Run();

			GraphWriter.Write("Truck_DriverStrategy_Drive_stop_85_stop_85_level.vmod",
				@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_stop_85_stop_85_level.vmod");
		}

		#endregion

		#region AccelerateOverspeed

		[TestMethod]
		public void Accelerate_0_85_downhill_5_overspeed()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_downhill_5-overspeed.vmod", true)
				.Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_1_overspeed()
		{
			var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_1);
			Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_downhill_1-overspeed.vmod", true)
				.Run();
		}

		#endregion
	}
}