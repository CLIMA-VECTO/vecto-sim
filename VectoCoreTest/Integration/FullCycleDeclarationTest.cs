using System.IO;
using System.Linq;
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
				7500.SI<Kilogram>(), 12900.SI<Kilogram>(), true);

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_RegionalDeliveryCycle_RefLoad_Declaration()
		{
			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode,
				@"c:\Users\Technik\Downloads\40t Long Haul Truck\40t_Long_Haul_Truck.vecto");
			factory.WriteModalResults = true;
			factory.SumWriter = new SummaryFileWriter("Test.vsum");
			var runs = factory.SimulationRuns().ToArray();

			var run = runs[4];
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
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("Suburban");
			var run = Truck40tPowerTrain.CreateEngineeringRun(cycle, "Truck40t_SubUrbanCycle_RefLoad.vmod",
				7500.SI<Kilogram>(), 12900.SI<Kilogram>());

			run.Run();
			Assert.IsTrue(run.FinishedWithoutErrors);
		}

		[TestMethod]
		public void Truck40t_InterUrbanCycle_RefLoad()
		{
			var cycle = SimpleDrivingCycles.ReadDeclarationCycle("Interurban");
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
			factory.WriteModalResults = true;
			var sumFileName = Path.GetFileNameWithoutExtension(TruckDeclarationJob) + Constants.FileExtensions.SumFile;
			var sumWriter = new SummaryFileWriter(sumFileName);
			var jobContainer = new JobContainer(sumWriter);
			jobContainer.AddRuns(factory);

			jobContainer.Execute();

			foreach (var run in jobContainer.Runs) {
				Assert.IsTrue(run.Run.FinishedWithoutErrors);
			}
		}

		[TestMethod]
		public void Truck12t_LongHaulCycle_RefLoad_Declaration()
		{
			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode,
				@"c:\Users\Technik\Downloads\12t Delivery Truck\12t Delivery Truck.vecto");
			factory.WriteModalResults = true;
			factory.SumWriter = new SummaryFileWriter("Test.vsum");
			var runs = factory.SimulationRuns().ToArray();

			var run = runs[1];
			run.Run();

			Assert.IsTrue(run.FinishedWithoutErrors);
		}
	}
}