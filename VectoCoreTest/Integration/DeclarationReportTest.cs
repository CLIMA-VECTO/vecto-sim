using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;

namespace TUGraz.VectoCore.Tests.Integration
{
	[TestClass]
	public class DeclarationReportTest
	{
		[TestMethod]
		public void RunDeclarationMode()
		{
			var sumWriter = new SummaryFileWriter(@"job-report.vsum");
			var jobContainer = new JobContainer(sumWriter);

			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode, @"TestData\Jobs\job-report.vecto");

			jobContainer.AddRuns(factory);
			jobContainer.Execute();

			//ResultFileHelper.TestSumFile(@"TestData\Results\Integration\job.vsum", @"job.vsum");

			//ResultFileHelper.TestModFile(@"TestData\Results\Integration\job_1-Gear-Test-dist.vmod",
			//	@"TestData\job_1-Gear-Test-dist.vmod", testRowCount: false);
		}
	}
}