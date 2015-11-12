using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
    [TestClass]
    public class SimulationTests
    {
        private const string EngineFile = @"TestData\Components\24t Coach.veng";
        private const string CycleFile = @"TestData\Cycles\Coach Engine Only short.vdri";

        private const string EngineOnlyJob = @"TestData\Jobs\EngineOnlyJob.vecto";

        [TestMethod]
        public void TestSimulationEngineOnly()
        {
            var resultFileName = "TestEngineOnly-result.vmod";
            var job = CreateRun(resultFileName);

            var container = job.GetContainer();

            Assert.AreEqual(560.RPMtoRad(), container.EngineSpeed);
            Assert.AreEqual(0U, container.Gear);
        }

        [TestMethod]
        public void TestEngineOnly_JobRun()
        {
            var actual = @"TestData\Jobs\EngineOnlyJob_Coach Engine Only short.vmod";
            var expected = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

            var job = CreateRun(actual);
            job.Run();

            ResultFileHelper.TestModFile(expected, actual);
        }

        private class MockSumWriter : SummaryFileWriter
        {
            public override void Write(bool isEngineOnly, IModalDataWriter data, string jobFileName, string jobName,
                string cycleFileName,
                Kilogram vehicleMass, Kilogram vehicleLoading) {}

            public override void Finish() {}
        }


        [TestMethod]
        public void TestEngineOnly_SimulatorRun()
        {
            var actual = @"TestData\Jobs\EngineOnlyJob_Coach Engine Only short.vmod";
            var expected = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

            var run = CreateRun(actual);

            var sim = new JobContainer(new MockSumWriter());
            sim.AddRun(run);
            sim.Execute();
            sim.WaitFinished();

            ResultFileHelper.TestModFile(expected, actual);
        }

        public IVectoRun CreateRun(string resultFileName)
        {
            var sumFileName = resultFileName.Substring(0, resultFileName.Length - 5) + Constants.FileExtensions.SumFile;

            var dataWriter = new ModalDataWriter(resultFileName, SimulatorFactory.FactoryMode.EngineOnlyMode);
            var sumWriter = new SummaryFileWriter(sumFileName);

            var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineOnlyMode, EngineOnlyJob) {
                SumWriter = sumWriter
            };

            return factory.SimulationRuns().First();
        }

        [TestMethod]
        public void Test_VectoJob()
        {
            var sumWriter = new SummaryFileWriter(@"24t Coach.vsum");
            var jobContainer = new JobContainer(sumWriter);

            var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineOnlyMode,
                @"TestData\Jobs\24t Coach EngineOnly.vecto");

            jobContainer.AddRuns(runsFactory);
            jobContainer.Execute();

            jobContainer.WaitFinished();

            ResultFileHelper.TestSumFile(@"TestData\Results\EngineOnlyCycles\24t Coach.vsum", @"24t Coach.vsum");

            ResultFileHelper.TestModFiles(new[] {
                @"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only1.vmod",
                @"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only2.vmod",
                @"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only3.vmod"
            }, new[] {
                @"TestData\Jobs\24t Coach EngineOnly_Engine Only1.vmod",
                @"TestData\Jobs\24t Coach EngineOnly_Engine Only2.vmod",
                @"TestData\Jobs\24t Coach EngineOnly_Engine Only3.vmod"
            })
                ;
        }
    }
}