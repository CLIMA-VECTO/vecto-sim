using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Integration.DriverStrategy
{
    [TestClass]
    public class DriverStrategyTestTruck
    {
        [TestInitialize]
        public void DisableLogging()
        {
            //LogManager.DisableLogging();
            //GraphWriter.Disable();
        }

        #region Accelerate

        [TestMethod]
        public void Truck_Accelerate_20_60_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_Level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_20_60_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_level.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_20_60_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphilll_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_60_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_uphill_5.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_20_60_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_60_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_downhill_5.vmod");
        }


        [TestMethod, Ignore]
        public void Truck_Accelerate_20_60_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_60_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_uphill_25.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_20_60_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_60_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_downhill_25.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_20_60_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_uphill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_60_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_uphill_15.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_20_60_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_60_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_60_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_60_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_60_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_level.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_uphill_1()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_1);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_uphill_1.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_1.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_1.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_uphill_2()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_2);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_uphill_2.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_2.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_2.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_5.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
            Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Accelerate_0_85_downhill_5.vmod").Run();

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_25.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_25.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_uphill_10()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_uphill_10);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_uphill_10.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_uphill_10.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_uphill_10.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_stop_0_85_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_stop_0_85_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_stop_0_85_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_stop_0_85_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_stop_0_85_level.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_20_22_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_20_22_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_20_22_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_20_22_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Accelerate_20_22_uphill_5.vmod");
        }

        #endregion

        #region Decelerate

        [TestMethod]
        public void Truck_Decelerate_22_20_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_22_20_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_22_20_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_22_20_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_22_20_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_60_20_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_60_20_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_level.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_45_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_45_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_45_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_45_0_level.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_45_0_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_45_0_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_45_0_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_45_0_uphill_5.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_45_0_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_45_0_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_45_0_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_45_0_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_45_0_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_60_20_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_60_20_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_uphill_5.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_60_20_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_60_20_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_downhill_5.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_60_20_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_60_20_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_uphill_25.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_60_20_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_60_20_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_downhill_25.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_60_20_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_uphill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_60_20_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_uphill_15.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_60_20_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_60_20_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_60_20_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_60_20_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_60_20_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Decelerate_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_level.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_80_0_uphill_3()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_3);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_uphill_3.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_uphill_3.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_uphill_3.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_80_0_SlopeChangeDuringCoast()
        {
            var data = new string[] {
                // <s>,<v>,<grad>,<stop>
                "  0,  60, -1.15,     0",
                " 70,  60, -1.85,     0",
                "300,   0, -1.85,     2",
            };

            var cycle = SimpleDrivingCycles.CreateCycleData(data);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_SlopeChangeDuringCoast.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);
        }

        [TestMethod, Ignore]
        public void Truck_Decelerate_80_0_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_uphill_5.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_80_0_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_downhill_5.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_80_0_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_steep_uphill_25.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_80_0_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_downhill_25.vmod");
        }

        [TestMethod, Ignore]
        public void Decelerate_80_0_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_uphill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_steep_uphill_15.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_80_0_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerate_80_0_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_80_0_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_80_0_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_20_0_downhill_2()
        {
            var cycleData = new string[] {
                // <s>,<v>,<grad>,<stop>
                "   0,  20, -1.8,  0",
                "2052,  0,  -1.8,  2"
            };
            var cycle = SimpleDrivingCycles.CreateCycleData(cycleData);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Decelerate_20_0_downhill_2_RefLoad.vmod",
                7500.SI<Kilogram>(), 12900.SI<Kilogram>());

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            //GraphWriter.Write("Truck_DriverStrategy_Decelerate_80_0_downhill_15.vmod",
            //	@"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Decelerate_20_0_downhill_2_RefLoad.vmod");
        }

        [TestMethod]
        public void Truck_Decelerate_55_43_0_downhill_RefLoad()
        {
            var cycleData = new string[] {
                // <s>,<v>,<grad>,<stop>
                "   0,  55.2, -0.6,  0",
                " 500,  43.6, -0.6,  0",
                " 510,  43.6, -0.3,  0",
                " 623,   0, -0.2,  4"
            };
            var cycle = SimpleDrivingCycles.CreateCycleData(cycleData);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_Decelerate_55_43_0_downhill_RefLoad.vmod",
                7500.SI<Kilogram>(), 12900.SI<Kilogram>());

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);
        }

        #endregion

        #region Drive

        [TestMethod]
        public void Truck_Drive_80_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_level.vmod");
        }

        [TestMethod]
        public void Truck_Drive_80_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_uphill_5.vmod");
        }

        [TestMethod]
        public void Truck_Drive_80_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Drive_20_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_20_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_20_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_20_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_20_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Drive_30_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_30_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Drive_50_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_50_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_downhill_15.vmod");
        }

        [TestMethod, Ignore]
        public void Truck_Drive_80_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_uphill_25.vmod");
        }

        [TestMethod]
        public void Truck_Drive_80_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Drive_80_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_uphill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_uphill_15.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_level.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_uphill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_uphill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_uphill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_uphill_5.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_downhill_5()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_downhill_5.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_downhill_5.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_downhill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_downhill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_downhill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_downhill_25.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_uphill_25()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_25);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_uphill_25.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_uphill_25.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_uphill_25.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_downhill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_downhill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_downhill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_downhill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_downhill_15.vmod");
        }

        [TestMethod]
        public void Truck_Drive_10_uphill_15()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_10_uphill_15);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_10_uphill_15.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_10_uphill_15.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_10_uphill_15.vmod");
        }

        #endregion

        #region Slope

        [TestMethod]
        public void Truck_Drive_80_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Increasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_slope_inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_slope_inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_50_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Increasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_slope_inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_50_slope_inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_30_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Increasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_slope_inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_30_slope_inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_80_Decreasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Decreasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_slope_dec.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_slope_dec.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_Decreasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_50_Decreasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Decreasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_slope_dec.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_50_slope_dec.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_Decreasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_30_Decreasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Decreasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_slope_dec.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_30_slope_dec.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_Decreasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_80_Dec_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_80_Dec_Increasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_80_slope_dec-inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_80_slope_dec-inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_80_Dec_Increasing_Slope.vmod");
        }

        [TestMethod]
        public void Truck_Drive_50_Dec_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_50_Dec_Increasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_50_slope_dec-inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_50_slope_dec-inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_50_Dec_Increasing_Slope.vmod");
        }


        [TestMethod]
        public void Truck_Drive_30_Dec_Increasing_Slope()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_30_Dec_Increasing_Slope);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_Drive_30_slope_dec-inc.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_30_slope_dec-inc.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_30_Dec_Increasing_Slope.vmod");
        }

        #endregion

        #region Misc

        [TestMethod]
        public void Truck_DecelerateWhileBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDecelerateWhileBrake_80_0_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_DecelerateWhileBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_DecelerateWhileBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Truck_AccelerateWhileBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateWhileBrake_80_0_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_AccelerateWhileBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_AccelerateWhileBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Truck_AccelerateAtBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateAtBrake_80_0_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_AccelerateAtBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_AccelerateAtBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_AccelerateAtBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Truck_AccelerateBeforeBrake_80_0_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerateBeforeBrake_80_0_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_AccelerateBeforeBrake_80_0_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_AccelerateBeforeBrake_80_0_level.vmod");
        }

        [TestMethod]
        public void Truck_Drive_stop_85_stop_85_level()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleDrive_stop_85_stop_85_level);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Drive_stop_85_stop_85_level.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Drive_stop_85_stop_85_level.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck\40t_Long_Haul_Truck_Cycle_Drive_stop_85_stop_85_level.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_48_52_beforeStop_lefel()
        {
            var data = new string[] {
                // <s>,<v>,<grad>,<stop>
                "  0,  49.9, -5,     0",
                "200,  52, -5,     0",
                "300,   0, -5,     2",
            };

            var cycle = SimpleDrivingCycles.CreateCycleData(data);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_48_52_beforeStop_level.vmod",
                7500.SI<Kilogram>(), 19000.SI<Kilogram>());

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            //GraphWriter.Write("Truck_DriverStrategy_Accelerate_48_52_beforeStop_level.vmod");
        }

        #endregion

        #region AccelerateOverspeed

        [TestMethod]
        public void Truck_Accelerate_0_85_downhill_5_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_downhill_5-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_5-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_85_downhill_3_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_3);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_downhill_3-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_3-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_3.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_0_85_downhill_1_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_85_downhill_1);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_85_downhill_1-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_85_downhill_1-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_85_downhill_1.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_0_60_downhill_5_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_60_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_60_downhill_5-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_60_downhill_5-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_60_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_60_downhill_3_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_60_downhill_3);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_60_downhill_3-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_60_downhill_3-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_60_downhill_3.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_0_60_downhill_1_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_60_downhill_1);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_60_downhill_1-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_60_downhill_1-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_60_downhill_1.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_0_40_downhill_5_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_40_downhill_5);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_40_downhill_5-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_40_downhill_5-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_40_downhill_5.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_0_40_downhill_3_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_40_downhill_3);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_40_downhill_3-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_40_downhill_3-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_40_downhill_3.vmod");
        }


        [TestMethod]
        public void Truck_Accelerate_0_40_downhill_1_overspeed()
        {
            var cycle = SimpleDrivingCycles.CreateCycleData(SimpleDrivingCycles.CycleAccelerate_0_40_downhill_1);
            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_0_40_downhill_1-overspeed.vmod", true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);

            GraphWriter.Write("Truck_DriverStrategy_Accelerate_0_40_downhill_1-overspeed.vmod",
                @"..\..\TestData\Integration\DriverStrategy\Vecto2.2\40t Truck_Overspeed\40t_Long_Haul_Truck_Cycle_Accelerate_0_40_downhill_1.vmod");
        }

        [TestMethod]
        public void Truck_Accelerate_Decelerate_Downhill_overspeed()
        {
            var cycleData = new string[] {
                // <s>,<v>,<grad>,<stop>
                "  0,  60,  0,     0",
                " 10,  60, -6,     0",
                "100,  55, -6,     0",
                "300,  55, -6,     0"
            };
            var cycle = SimpleDrivingCycles.CreateCycleData(cycleData);

            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle,
                "Truck_DriverStrategy_Accelerate_Decelerate-overspeed.vmod",
                true);

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);
        }

        [TestMethod]
        public void Truck_SlopeChangeBeforeStop()
        {
            var cycleData = new string[] {
                // <s>,<v>,<grad>,<stop>
                "  0,  60, -1.4,     0",
                "298,  60, -1.7,     0",
                "300,   0, -1.7,     4",
            };
            var cycle = SimpleDrivingCycles.CreateCycleData(cycleData);

            var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck_DriverStrategy_SlopeChangeBeforeStop.vmod");

            run.Run();
            Assert.IsTrue(run.FinishedWithoutErrors);
        }

        [TestMethod]
        public void Truck_FrequentSlopeChanges()
        {
            var cycleData = new string[] {
                // <s>,<v>,<grad>,<stop>
                "  0,  60,  0,     0",
                " 10,  60, -6,     0",
                "100,  55, -6,     0",
                "300,  55, -6,     0"
            };
        }

        #endregion
    }
}