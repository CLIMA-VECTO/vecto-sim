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
			var expectedResultsName = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";

			var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, resultFileName);
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

			var sim = new JobContainer();
			var job = SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, resultFileName);
			sim.AddJob(job);
			sim.RunJobs();

			var results = ModalResults.ReadFromFile(resultFileName);
			var expectedResults = ModalResults.ReadFromFile(expectedResultsName);

			Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");
		}

		public IVectoSimulator CreateJob(string resultFileName)
		{
			if (File.Exists(resultFileName)) {
				File.Delete(resultFileName);
			}
			return SimulatorFactory.CreateTimeBasedEngineOnlyJob(EngineFile, CycleFile, resultFileName);
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

			var simulation = new JobContainer();
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


			// compare results

			var expectedResultFiles = new[] {
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only1.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only2.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only3.vmod"
			};

			var expectedSumFile = @"TestData\Results\EngineOnlyCycles\24t Coach.vsum";
			var sumFile = @"24t Coach.vsum";
			var resultFiles = expectedResultFiles.Select(x => Path.GetFileName(x));

			Assert.IsTrue(File.Exists(sumFile), "sum file is missing: " + sumFile);
			foreach (var result in resultFiles) {
				Assert.IsTrue(File.Exists(result), "vmod file is missing: " + result);
			}

			//Assert.AreEqual(File.ReadAllLines(sumFile).Length, File.ReadAllLines(expectedSumFile).Length,
			//	"sum file row count differs.");

			var resultFileIt = resultFiles.GetEnumerator();

			foreach (var expectedResultFile in expectedResultFiles) {
				resultFileIt.MoveNext();
				var results = ModalResults.ReadFromFile(resultFileIt.Current);
				var expectedResults = ModalResults.ReadFromFile(expectedResultFile);

				Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");
			}

			//todo compare sum file
			Assert.Inconclusive("todo: compare sum file!!");
		}
	}
}