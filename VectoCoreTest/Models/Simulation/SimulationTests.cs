using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Utils;

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
			var resultFileName = "TestEngineOnly_JobRun-result.vmod";
			var expectedResultsName = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

			var job = CreateJob(resultFileName);
			job.Run();

			var results = ModalResults.ReadFromFile(resultFileName);
			var expectedResults = ModalResults.ReadFromFile(expectedResultsName);

			Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");
		}

		[TestMethod]
		public void TestEngineOnly_SimulatorRun()
		{
			var resultFileName = "TestEngineOnly_SimulatorRun-result.vmod";
			var expectedResultsName = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

			var job = CreateJob(resultFileName);

			var sim = new JobContainer(new TestSumWriter());
			sim.AddJob(job);
			sim.RunJobs();

			var results = ModalResults.ReadFromFile(resultFileName);
			var expectedResults = ModalResults.ReadFromFile(expectedResultsName);

			Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");
		}

		public IVectoSimulator CreateJob(string resultFileName)
		{
			var sumFileName = resultFileName.Substring(0, resultFileName.Length - 4) + "vsum";

			if (File.Exists(resultFileName)) {
				File.Delete(resultFileName);
			}

			if (File.Exists(sumFileName)) {
				File.Delete(sumFileName);
			}

			var dataWriter = new ModalDataWriter(resultFileName);
			var sumWriter = new SummaryFileWriter(sumFileName);
			var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, dataWriter, sumWriter);

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
			var expectedResultsName = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";
			var expectedResults = ModalResults.ReadFromFile(expectedResultsName);

			var simulation = new JobContainer(new TestSumWriter());
			foreach (var resultFile in resultFiles) {
				simulation.AddJob(CreateJob(resultFile));
			}

			resultFiles = resultFiles.Select(x => x + "_Coach Engine Only short.vmod").ToArray();

			simulation.RunJobs();

			foreach (var resultFile in resultFiles) {
				var results = ModalResults.ReadFromFile(resultFile);
				Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");
			}
		}

		[TestMethod]
		public void Test_VectoJob()
		{
			//run jobs
			var jobData = VectoJobData.ReadFromFile(@"TestData\Jobs\24t Coach.vecto");
			var jobContainer = new JobContainer(jobData);
			jobContainer.RunJobs();


			// check sum file
			var expectedSumFile = @"TestData\Results\EngineOnlyCycles\24t Coach.vsum";
			var sumFile = @"TestData\Jobs\24t Coach.vsum";
			Assert.IsTrue(File.Exists(sumFile), "sum file is missing: " + sumFile);
			Assert.AreEqual(File.ReadAllLines(sumFile).Length, File.ReadAllLines(expectedSumFile).Length,
				string.Format("sum file row count differs. Expected File: {0}, Actual File: {1}", expectedSumFile, sumFile));

			// check vmod files
			var expectedResultFiles = new[] {
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only1.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only2.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only3.vmod"
			};
			var resultFiles = expectedResultFiles.Select(x => Path.GetFileName(x));
			foreach (var result in resultFiles) {
				Assert.IsTrue(File.Exists(result), "vmod file is missing: " + result);
			}

			var resultFileIt = resultFiles.GetEnumerator();

			foreach (var expectedResultFile in expectedResultFiles) {
				resultFileIt.MoveNext();
				var results = ModalResults.ReadFromFile(resultFileIt.Current);
				var expectedResults = ModalResults.ReadFromFile(expectedResultFile);

				Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count,
					string.Format("Moddata: Row count differs. Expected File: {0}, Actual File: {1}", expectedResultFile,
						resultFileIt.Current));
			}
		}
	}
}