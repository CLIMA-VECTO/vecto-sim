using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Integration.DriverStrategy
{
    [TestClass]
    public class DriverStrategyTestCoach
    {
        [TestInitialize]
        public void DisableLogging()
        {
            //LogManager.DisableLogging();
            //GraphWriter.Disable();
        }

        [TestMethod]
        public void TestGraph()
        {
            var imgV3 = @"..\..\..\VectoCoreTest\bin\Debug\Coach_DriverStrategy_Drive_50_slope_dec-inc.vmod";
            var imgv22 =
                @"..\..\..\VectoCoreTest\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_50_Dec_Increasing_Slope.vmod";

            GraphWriter.Write(imgV3, imgv22);
        }


        [TestMethod]
        public void TestSingleGraph()
        {
            var imgV3 = @"..\..\..\VectoCoreTest\bin\Debug\Coach_DriverStrategy_Drive_50_slope_dec-inc.vmod";

            GraphWriter.Write(imgV3);
        }

        #region Accelerate

        [TestMethod]
        public void Coach_Accelerate_20_60_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_Level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_level.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_20_60_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphilll_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_uphill_5.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_20_60_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_20_60_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_downhill_5.vmod");
        }


        [TestMethod, Ignore]
        public void Coach_Accelerate_20_60_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_uphill_25.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_20_60_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_20_60_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_downhill_25.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_20_60_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_60_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_uphill_15.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_20_60_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_20_60_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_60_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_60_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_level.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_uphill_1()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_1);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_1.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_uphill_1.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_uphill_1.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_uphill_2()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_2);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_2.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_uphill_2.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_uphill_2.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_uphill_5.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_uphill_25.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_85_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_downhill_25.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_uphill_10()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_10);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_0_85_uphill_10.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_uphill_10.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_uphill_10.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_85_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_0_85_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_stop_0_85_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_stop_0_85_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_stop_0_85_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_stop_0_85_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_stop_0_85_level.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_20_22_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_22_uphill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Accelerate_20_22_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_20_22_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Accelerate_20_22_uphill_5.vmod");
        }

        #endregion

        #region Decelerate

        [TestMethod]
        public void Coach_Decelerate_22_20_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_22_20_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_22_20_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_22_20_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_22_20_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_60_20_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_level.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_45_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_45_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_45_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_45_0_level.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_45_0_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_uphill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_45_0_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_45_0_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_45_0_uphill_5.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_45_0_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_45_0_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_45_0_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_45_0_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_60_20_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_uphill_5.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_60_20_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_60_20_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_downhill_5.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_60_20_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_uphill_25.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_60_20_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_60_20_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_downhill_25.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_60_20_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_60_20_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_uphill_15.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_60_20_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_60_20_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_60_20_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_60_20_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_level.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_80_0_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_uphill_5.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_80_0_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_downhill_5.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_80_0_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_steep_uphill_25.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_80_0_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_80_0_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_downhill_25.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_80_0_uphill_3()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_3);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Decelerate_80_0_uphill_3.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_uphill_3.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_uphill_3.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_80_0_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_steep_uphill_15.vmod");
        }

        [TestMethod]
        public void Coach_Decelerate_80_0_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Decelerate_80_0_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Decelerate_80_0_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Decelerate_80_0_downhill_15.vmod");
        }

        #endregion

        #region Drive

        [TestMethod]
        public void Coach_Drive_80_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_level.vmod");
        }

        [TestMethod]
        public void Coach_Drive_80_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_uphill_5.vmod");
        }

        [TestMethod]
        public void Coach_Drive_80_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Drive_20_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_20_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_20_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_20_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_20_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Drive_30_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_30_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_30_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Drive_50_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_50_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_50_downhill_15.vmod");
        }

        [TestMethod, Ignore]
        public void Coach_Drive_80_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_uphill_25.vmod");
        }

        [TestMethod]
        public void Coach_Drive_80_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Drive_80_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_uphill_15.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_level.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(new[] {
                // <s>,<v>,<grad>,<stop>
                "   0,  10, 5,    0",
                "800,  10, 5,    0",
            });
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_uphill_5.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_downhill_25.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_25);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_uphill_25.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_downhill_15.vmod");
        }

        [TestMethod]
        public void Coach_Drive_10_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_15);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_10_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_10_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_10_uphill_15.vmod");
        }

        #endregion

        #region Slope

        [TestMethod]
        public void Coach_Drive_80_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Increasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_slope_inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_slope_inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_50_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Increasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_slope_inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_50_slope_inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_50_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_30_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Increasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_slope_inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_30_slope_inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_30_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_80_Decreasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Decreasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_slope_dec.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_slope_dec.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_Decreasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_50_Decreasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Decreasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_slope_dec.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_50_slope_dec.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_50_Decreasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_30_Decreasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Decreasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_slope_dec.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_30_slope_dec.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_30_Decreasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_80_Dec_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Dec_Increasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_80_slope_dec-inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_80_slope_dec-inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_80_Dec_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Coach_Drive_50_Dec_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Dec_Increasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_50_slope_dec-inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_50_slope_dec-inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_50_Dec_Increasing_Slope.vmod");
        }


        [TestMethod]
        public void Coach_Drive_30_Dec_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Dec_Increasing_Slope);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle, "Coach_DriverStrategy_Drive_30_slope_dec-inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_30_slope_dec-inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_30_Dec_Increasing_Slope.vmod");
        }

        #endregion

        #region Misc

        [TestMethod]
        public void Coach_DecelerateWhileBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerateWhileBrake_80_0_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_DecelerateWhileBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Coach_AccelerateWhileBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateWhileBrake_80_0_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_AccelerateWhileBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Coach_AccelerateAtBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateAtBrake_80_0_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_AccelerateAtBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_AccelerateAtBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_AccelerateAtBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Coach_AccelerateBeforeBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateBeforeBrake_80_0_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_AccelerateBeforeBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Coach_Drive_stop_85_stop_85_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_stop_85_stop_85_level);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Drive_stop_85_stop_85_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Drive_stop_85_stop_85_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_stop_85_stop_85_level.vmod");
        }

        #endregion

        #region AccelerateOverspeed

        [TestMethod]
        public void Coach_Accelerate_0_85_downhill_5_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_85_downhill_5-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_downhill_5-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_85_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_85_downhill_3_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_3);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_85_downhill_3-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_downhill_3-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_85_downhill_3.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_0_85_downhill_1_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_1);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_85_downhill_1-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_85_downhill_1-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_85_downhill_1.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_0_60_downhill_5_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_60_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_60_downhill_5-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_60_downhill_5-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_60_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_60_downhill_3_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_60_downhill_3);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_60_downhill_3-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_60_downhill_3-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_60_downhill_3.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_0_60_downhill_1_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_60_downhill_1);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_60_downhill_1-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_60_downhill_1-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_60_downhill_1.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_0_40_downhill_5_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_40_downhill_5);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_40_downhill_5-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_40_downhill_5-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_40_downhill_5.vmod");
        }

        [TestMethod]
        public void Coach_Accelerate_0_40_downhill_3_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_40_downhill_3);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_40_downhill_3-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_40_downhill_3-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_40_downhill_3.vmod");
        }


        [TestMethod]
        public void Coach_Accelerate_0_40_downhill_1_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_40_downhill_1);
            var run = CoachPowerTrain.CreateEngineeringRun(cycle,
                "Coach_DriverStrategy_Accelerate_0_40_downhill_1-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Coach_DriverStrategy_Accelerate_0_40_downhill_1-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\Coach_Overspeed\24t Coach_Cycle_Accelerate_0_40_downhill_1.vmod");
        }

        #endregion
    }
}