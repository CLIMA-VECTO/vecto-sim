using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories;
using TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class VehicleDataTest
	{
		private const string VehicleDataFile = @"TestData\Components\24t Coach.vveh";

		[TestMethod]
		public void ReadVehicleFileTest()
		{
			IDataFileReader reader = new EngineeringModeSimulationComponentFactory();
			var vehicleData = reader.ReadVehicleDataFile(VehicleDataFile);
			//VehicleData.ReadFromFile(VehicleDataFile);

			Assert.AreEqual(VehicleCategory.Coach, vehicleData.VehicleCategory);
		}
	}
}