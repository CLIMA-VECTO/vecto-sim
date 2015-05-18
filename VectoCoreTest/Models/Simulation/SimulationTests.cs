using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Utils;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class SimulationTests
	{
		private const string EngineFile = @"TestData\Components\24t Coach.veng";
		private const string CycleFile = @"TestData\Cycles\Coach Engine Only short.vdri";

		[TestMethod]
		public void TestSimulationEngineOnly()
		{
			var resultFileName = "TestEngineOnly-result.vmod";
			var job = CreateJob(resultFileName);

			var container = job.GetContainer();

			Assert.AreEqual(560.RPMtoRad(), container.EngineSpeed());
			Assert.AreEqual(0U, container.Gear());
		}

		[TestMethod]
		public void TestEngineOnly_JobRun()
		{
			var actual = "TestEngineOnly_JobRun-result.vmod";
			var expected = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

			var job = CreateJob(actual);
			job.Run();

			ResultFileHelper.TestModFile(expected, actual);
		}

		[TestMethod]
		public void TestEngineOnly_SimulatorRun()
		{
			var actual = @"TestEngineOnly_SimulatorRun-result.vmod";
			var expected = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

			var job = CreateJob(actual);

			var sim = new JobContainer(new TestSumWriter());
			sim.AddJob(job);
			sim.RunJobs();

			ResultFileHelper.TestModFile(expected, actual);
		}

		public IVectoSimulator CreateJob(string resultFileName)
		{
			var sumFileName = resultFileName.Substring(0, resultFileName.Length - 4) + "vsum";

			var dataWriter = new ModalDataWriter(resultFileName, engineOnly: true);
			var sumWriter = new SummaryFileWriter(sumFileName);
			var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, jobFileName: "", jobName: "",
				dataWriter: dataWriter, sumWriter: sumWriter);

			return job;
		}


		[TestMethod]
		public void TestEngineOnly_MultipleJobs()
		{
			var resultFiles = new[] {
				@"TestEngineOnly-MultipleJobs-result1",
				@"TestEngineOnly-MultipleJobs-result2",
				@"TestEngineOnly-MultipleJobs-result3"
			};

			var simulation = new JobContainer(new TestSumWriter());
			foreach (var resultFile in resultFiles) {
				simulation.AddJob(CreateJob(resultFile));
			}
			simulation.RunJobs();

			ResultFileHelper.TestModFiles(resultFiles.Select(x => x + "_Coach Engine Only short.vmod"),
				Enumerable.Repeat(@"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod", resultFiles.Length));
		}

		[TestMethod]
		public void Test_VectoJob()
		{
			var jobData = VectoJobData.ReadFromFile(@"TestData\Jobs\24t Coach.vecto");
			var jobContainer = new JobContainer(jobData);
			jobContainer.RunJobs();

			ResultFileHelper.TestSumFile(@"TestData\Results\EngineOnlyCycles\24t Coach.vsum", @"TestData\Jobs\24t Coach.vsum");
			ResultFileHelper.TestModFiles(new[] {
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only1.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only2.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only3.vmod"
			}, new[] { "24t Coach_Engine Only1.vmod", "24t Coach_Engine Only2.vmod", "24t Coach_Engine Only3.vmod" });
		}
	}
}