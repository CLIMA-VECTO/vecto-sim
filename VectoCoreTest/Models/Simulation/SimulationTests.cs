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
			var resultFiles =
				new[] { 1, 2, 3 }.Select(x => string.Format("TestEngineOnly-MultipleJobs-result{0}.vmod", x)).ToList();

			var expectedResultsName = @"TestData\Results\EngineOnlyCycles\24tCoach_EngineOnly short.vmod";
			var expectedResults = ModalResults.ReadFromFile(expectedResultsName);

			var simulation = new JobContainer();
			foreach (var resultFile in resultFiles) {
				simulation.AddJob(CreateJob(resultFile));
			}

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
			var resultFiles = new[] { 1, 2, 3 }.Select(x => string.Format(@"24t Coach{0}.vmod", x)).ToList();
			var sumFile = @"24t Coach.vsum";
			var expectedSumFile = "24t Coach-expected.vsum";

			Assert.IsTrue(File.Exists(sumFile), "sum file is missing: " + sumFile);
			foreach (var result in resultFiles) {
				Assert.IsTrue(File.Exists(result), "vmod file is missing: " + result);
			}
			var expectedResultFiles =
				resultFiles.Select(x => Path.GetFileNameWithoutExtension(x) + "-expected.vmod").GetEnumerator();

			Assert.AreEqual(File.ReadAllLines(sumFile).Length, File.ReadAllLines(expectedSumFile).Length,
				"sum file row count differs.");

			foreach (var resultFile in resultFiles) {
				var results = ModalResults.ReadFromFile(resultFile);
				expectedResultFiles.MoveNext();
				var expectedResults = ModalResults.ReadFromFile(expectedResultFiles.Current);

				Assert.AreEqual(expectedResults.Rows.Count, results.Rows.Count, "Moddata: Row count differs.");
			}

			Assert.Inconclusive("compare correct filenames!!");
		}
	}
}