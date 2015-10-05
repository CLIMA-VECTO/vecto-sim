using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class VehicleTest
	{
		public static readonly double Tolerance = 0.001;

		private const string VehicleDataFileCoach = @"TestData\Components\24t Coach.vveh";

		private const string VehicleDataFileTruck = @"TestData\Components\40t_Long_Haul_Truck.vveh";

		[TestMethod]
		public void VehiclePortTest()
		{
			var container = new VehicleContainer();

			//var reader = new EngineeringModeSimulationDataReader();
			var vehicleData = EngineeringModeSimulationDataReader.CreateVehicleDataFromFile(VehicleDataFileCoach);
			//VehicleData.ReadFromFile(VehicleDataFile);
			//vehicleData.CrossWindCorrection = VehicleData.CrossWindCorrectionMode.NoCorrection;
			var vehicle = new Vehicle(container, vehicleData);

			var mockPort = new MockFvOutPort();
			vehicle.InPort().Connect(mockPort);

			vehicle.Initialize(17.210535.SI<MeterPerSecond>(), 0.SI<Radian>());

			var requestPort = vehicle.OutPort();

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();

			var accell = -0.256231159.SI<MeterPerSquareSecond>();
			var gradient = Math.Atan(0.00366547048).SI<Radian>();

			var retVal = requestPort.Request(absTime, dt, accell, gradient);

			Assert.AreEqual(-2549.06772173622, mockPort.Force.Value(), 0.0001);
			Assert.AreEqual(16.954303841, mockPort.Velocity.Value(), 0.0001);
		}

		[TestMethod]
		public void VehicleSlopeResistanceTest()
		{
			var container = new VehicleContainer();

			var vehicleData = EngineeringModeSimulationDataReader.CreateVehicleDataFromFile(VehicleDataFileTruck);
			vehicleData.AerodynamicDragAera = 6.46.SI<SquareMeter>();

			var vehicle = new Vehicle(container, vehicleData);

			var mockPort = new MockFvOutPort();
			vehicle.InPort().Connect(mockPort);


			var tmp = vehicle.ComputeEffectiveAirDragArea(0.KMPHtoMeterPerSecond());
			Assert.AreEqual(8.06, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(60.KMPHtoMeterPerSecond());
			Assert.AreEqual(8.06, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(75.KMPHtoMeterPerSecond());
			Assert.AreEqual(7.64, tmp.Value(), Tolerance);
		}
	}
}