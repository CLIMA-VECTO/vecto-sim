using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Integration
{
	[TestClass]
	public class FullCycleDeclarationTest
	{
		public const string TruckDeclarationJob = @"TestData\Integration\DeclarationMode\40t Truck\40t_Long_Haul_Truck.vecto";


		[TestMethod]
		public void Truck40t_LongHaulCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("LongHaul");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_LongHaulCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_RegionalDeliveryCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("RegionalDelivery");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_RegionalDeliveryCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_UrbanDeliveryCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("UrbanDelivery");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_UrbanDeliveryCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_MunicipalCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("Municipal");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_MunicipalCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_ConstructionCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("Construction");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_ConstructionCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_HeavyUrbanCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("HeavyUrban");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_HeavyUrbanCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_SubUrbanCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("SubUrban");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_SubUrbanCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_InterUrbanCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("InterUrban");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_InterUrbanCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_CoachCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("Coach");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_CoachCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40tDeclarationTest()
		{
			LogManager.DisableLogging();

			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode, TruckDeclarationJob);
			var sumFileName = Path.GetFileNameWithoutExtension(TruckDeclarationJob) + Constants.FileExtensions.SumFile;
			var sumWriter = new SummaryFileWriter(sumFileName);
			var jobContainer = new JobContainer(sumWriter);

			jobContainer.AddRuns(factory);

			jobContainer.Execute();

			foreach (var run in jobContainer._runs) {
				Assert.IsTrue(run.FinishedWithoutErrors);
			}
		}
	}
}