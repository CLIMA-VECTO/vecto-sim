using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class AuxTests
	{
		private const string EngineFile = @"TestData\Components\24t Coach.veng";

		[TestMethod]
		public void Test_Aux_WriteModFileSumFile()
		{
			var sumWriter = new SummaryFileWriter(@"24t Coach.vsum");
			var jobContainer = new JobContainer(sumWriter);

			var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineOnlyMode);
			runsFactory.DataReader.SetJobFile(@"TestData\Jobs\24t Coach.vecto");

			jobContainer.AddRuns(runsFactory);
			jobContainer.Execute();

			ResultFileHelper.TestSumFile(@"TestData\Results\EngineOnlyCycles\24t Coach.vsum",
				@"TestData\Jobs\24t Coach.vsum");
			ResultFileHelper.TestModFiles(new[] {
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only1.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only2.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only3.vmod"
			}, new[] { "24t Coach_Engine Only1.vmod", "24t Coach_Engine Only2.vmod", "24t Coach_Engine Only3.vmod" });
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxFileInterpolate()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxColumnMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxFileMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxReadJobFile()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxDeclaration()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxDeclarationWrongConfiguration()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxEngineering()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxCycleAdditionalFieldMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxCycleAdditionalFieldOnly()
		{
			Assert.Inconclusive();
		}
	}
}