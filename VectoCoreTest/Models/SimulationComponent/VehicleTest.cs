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
		public void VehicleAirResistanceTest()
		{
			var container = new VehicleContainer();

			var vehicleData = EngineeringModeSimulationDataReader.CreateVehicleDataFromFile(VehicleDataFileTruck);
			vehicleData.AerodynamicDragAera = 6.46.SI<SquareMeter>();

			var vehicle = new Vehicle(container, vehicleData);

			var mockPort = new MockFvOutPort();
			vehicle.InPort().Connect(mockPort);


			var tmp = vehicle.ComputeEffectiveAirDragArea(0.KMPHtoMeterPerSecond());
			Assert.AreEqual(8.12204, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(60.KMPHtoMeterPerSecond());
			Assert.AreEqual(8.12204, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(75.KMPHtoMeterPerSecond());
			Assert.AreEqual(7.67232, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(100.KMPHtoMeterPerSecond());
			Assert.AreEqual(7.23949, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(52.1234.KMPHtoMeterPerSecond());
			Assert.AreEqual(8.12204, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(73.5432.KMPHtoMeterPerSecond());
			Assert.AreEqual(7.70967, tmp.Value(), Tolerance);

			tmp = vehicle.ComputeEffectiveAirDragArea(92.8765.KMPHtoMeterPerSecond());
			Assert.AreEqual(7.33617, tmp.Value(), Tolerance);

			// ====================

			var dt = 0.5.SI<Second>();
			vehicle.Initialize(60.KMPHtoMeterPerSecond(), 0.SI<Radian>());

			var avgForce = vehicle.AirDragResistance(0.SI<MeterPerSquareSecond>(), dt);
			Assert.AreEqual(1340.13618774784, avgForce.Value(), Tolerance);

			avgForce = vehicle.AirDragResistance(1.SI<MeterPerSquareSecond>(), dt);
			Assert.AreEqual(1375.658146, avgForce.Value(), Tolerance);

			avgForce = vehicle.AirDragResistance(0.5.SI<MeterPerSquareSecond>(), dt);
			Assert.AreEqual(1357.785735, avgForce.Value(), Tolerance);

			// - - - - - - 
			vehicle.Initialize(72.KMPHtoMeterPerSecond(), 0.SI<Radian>());

			avgForce = vehicle.AirDragResistance(0.5.SI<MeterPerSquareSecond>(), dt);
			Assert.AreEqual(1861.603488, avgForce.Value(), Tolerance);

			dt = 3.SI<Second>();

			avgForce = vehicle.AirDragResistance(1.SI<MeterPerSquareSecond>(), dt);
			Assert.AreEqual(2102.13153, avgForce.Value(), Tolerance);
		}
	}
}