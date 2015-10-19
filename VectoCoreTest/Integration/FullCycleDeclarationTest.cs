using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;

namespace TUGraz.VectoCore.Tests.Integration
{
	[TestClass]
	public class FullCycleDeclarationTest
	{
		public const string TruckDeclarationJob = @"TestData\Integration\DeclarationMode\40t Truck\40t_Long_Haul_Truck.vecto";

		[TestMethod]
		public void Truck40tDeclarationTest()
		{
			//LogManager.DisableLogging();

			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode, TruckDeclarationJob);
			var sumFileName = Path.GetFileNameWithoutExtension(TruckDeclarationJob) + Constants.FileExtensions.SumFile;
			var sumWriter = new SummaryFileWriter(sumFileName);
			var jobContainer = new JobContainer(sumWriter);

			jobContainer.AddRuns(factory);

			jobContainer.Execute();
		}
	}
}