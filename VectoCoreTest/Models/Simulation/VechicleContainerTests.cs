using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories;
using TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class VechicleContainerTests
	{
		private const string EngineFile = @"TestData\Components\24t Coach.veng";

		[TestMethod]
		public void VechicleContainerHasEngine()
		{
			var vehicle = new VehicleContainer();
			//var engineData = CombustionEngineData.ReadFromFile(EngineFile);
			var engineData = new EngineeringModeSimulationComponentFactory().CreateEngineDataFromFile(EngineFile);
			var engine = new CombustionEngine(vehicle, engineData);

			Assert.IsNotNull(vehicle.EngineSpeed());
		}
	}
}