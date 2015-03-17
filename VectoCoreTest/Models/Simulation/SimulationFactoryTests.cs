using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
    [TestClass]
    public class SimulationFactoryTests
    {
        private const string EngineFile = @"TestData\EngineOnly\EngineMaps\24t Coach.veng";
        private const string CycleFile = @"TestData\EngineOnly\Cycles\Coach Engine Only.vdri";

        [TestMethod]
        public void TestEngineOnly()
        {
            var job = SimulationFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile,  "TestEngineOnly-result.vmod");

            var container = job.GetContainer();

            Assert.AreEqual(560, container.EngineSpeed(), "Engine Speed");
            Assert.AreEqual(0U, container.Gear(), "Gear");

            try
            {
                container.VehicleSpeed();
                Assert.Fail("Access to Vehicle speed should fail, because there should be no vehicle in EngineOnly Mode.");
            }
            catch (VectoException ex)
            {
                Assert.AreEqual(ex.Message, "no vehicle available!", "Vehicle speed wrong exception message.");
            }
        }

        [TestMethod]
        public void TestEngineOnly_JobRun()
        {
            var job = SimulationFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, "TestEngineOnly-result.vmod");
            job.Run();

            //todo add assertions
            Assert.Fail();
        }

        [TestMethod]
        public void TestEngineOnly_SimulatorRun()
        {
            var sim = new VectoSimulator();
            var job = SimulationFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, "TestEngineOnly-result.vmod");
            sim.AddJob(job);            
            sim.RunSimulation();

            //todo add assertions
            Assert.Fail();
        }
    }
}
