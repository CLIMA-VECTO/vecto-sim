using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories;
using TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class VehicleTest
	{
		private const string VehicleDataFile = @"TestData\Components\24t Coach.vveh";

		[TestMethod]
		public void VehiclePortTest()
		{
			var container = new VehicleContainer();

			var reader = new EngineeringModeSimulationComponentFactory();
			var vehicleData = reader.ReadVehicleDataFile(VehicleDataFile);
			//VehicleData.ReadFromFile(VehicleDataFile);
			//vehicleData.CrossWindCorrection = VehicleData.CrossWindCorrectionMode.NoCorrection;
			var vehicle = new Vehicle(container, vehicleData, 17.210535);

			var mockPort = new MockFvOutPort();

			vehicle.InPort().Connect(mockPort);

			var requestPort = vehicle.OutShaft();

			var absTime = TimeSpan.FromSeconds(0);
			var dt = TimeSpan.FromSeconds(1);

			var accell = -0.256231159.SI<MeterPerSquareSecond>();
			var gradient = Math.Atan(0.00366547048).SI<Radian>();

			var retVal = requestPort.Request(absTime, dt, accell, gradient);

			Assert.AreEqual(-2549.07832743748, mockPort.Force.Double(), 0.0001);
			Assert.AreEqual(17.0824194205, mockPort.Velocity.Double(), 0.0001);
		}
	}
}