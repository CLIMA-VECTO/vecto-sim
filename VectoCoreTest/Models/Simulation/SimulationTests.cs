using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
    [TestClass]
    public class SimulationTests
    {
        private const string EngineFile = @"TestData\Components\24t Coach.veng";
        private const string CycleFile = @"TestData\Cycles\Coach Engine Only.vdri";

        [TestMethod]
        public void TestSimulationEngineOnly()
        {
            var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, "TestEngineOnly-result.vmod");

            var container = job.GetContainer();

            Assert.AreEqual(560.0.RPMtoRad(), container.EngineSpeed());
            Assert.AreEqual(0U, container.Gear());

            try {
                container.VehicleSpeed();
                Assert.Fail(
                    "Access to Vehicle speed should fail, because there should be no vehicle in EngineOnly Mode.");
            } catch (VectoException ex) {
                Assert.AreEqual(ex.Message, "no vehicle available!", "Vehicle speed wrong exception message.");
            }
        }

        [TestMethod]
        public void TestEngineOnly_JobRun()
        {
            var resultFileName = "TestEngineOnly_JobRun-result.vmod";
            var expectedResultsName = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly.vmod";

            var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, resultFileName);
            job.Run();

            var results = ModalResults.ReadFromFile(resultFileName);
            var expectedResults = ModalResults.ReadFromFile(expectedResultsName);

            Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");

            for (var i = 0; i < expectedResults.Rows.Count; i++) {
                var row = results.Rows[i];
                var expectedRow = expectedResults.Rows[i];

                foreach (DataColumn col in expectedResults.Columns) {
                    Assert.AreEqual(expectedRow[col], row[col], "Moddata: Value differs (Row {0}, Col {1}): {2} != {3}");
                }
            }
        }

        [TestMethod]
        public void TestEngineOnly_SimulatorRun()
        {
            var sim = new JobContainer();
            var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile,
                "TestEngineOnly-SimulatorRun-result.vmod");
            sim.AddJob(job);
            sim.RunSimulation();

            // todo: Add additional assertions.
            Assert.Fail("Todo: Add additional assertions.");
        }

        [TestMethod]
        public void TestEngineOnly_MultipleJobs()
        {
            var simulation = new JobContainer();

            var sim1 = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile,
                "TestEngineOnly-MultipleJobs-result1.vmod");
            simulation.AddJob(sim1);

            var sim2 = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile,
                "TestEngineOnly-MultipleJobs-result2.vmod");
            simulation.AddJob(sim2);

            var sim3 = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile,
                "TestEngineOnly-MultipleJobs-result3.vmod");
            simulation.AddJob(sim3);

            simulation.RunSimulation();

            // todo: Add additional assertions.
            Assert.Fail("Todo: Add additional assertions.");
        }
    }
}