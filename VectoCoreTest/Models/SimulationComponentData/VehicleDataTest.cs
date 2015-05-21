using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class VehicleDataTest
	{
		private const string VehicleDataFile = @"TestData\Components\24t Coach.vveh";

		[TestMethod]
		public void ReadVehicleFileTest()
		{
			var vehicleData = VehicleData.ReadFromFile(VehicleDataFile);

			Assert.AreEqual(VehicleCategory.Coach, vehicleData.VehicleCategory);
		}
	}
}