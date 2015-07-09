using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class FactoryTest
	{
		public const string DeclarationJobFile = @"Testdata\Jobs\12t Delivery Truck.vecto";

		public const string EngineeringJobFile = @"Testdata\Jobs\24t Coach.vecto";

		[TestMethod]
		public void CreateDeclarationSimulationRun()
		{
			var resultFileName = "test.vmod";
			var sumFileName = resultFileName.Substring(0, resultFileName.Length - 5) + Constants.FileExtensions.SumFile;

			//var dataWriter = new ModalDataWriter(resultFileName, engineOnly: true);
			var sumWriter = new SummaryFileWriter(sumFileName);

			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode) { SumWriter = sumWriter };

			factory.DataReader.SetJobFile(DeclarationJobFile);

			var run = factory.SimulationRuns().First();
			var vehicleContainer = (VehicleContainer) run.GetContainer();

			Assert.AreEqual(9, vehicleContainer._components.Count);

			Assert.IsInstanceOfType(vehicleContainer._gearbox, typeof (Gearbox), "gearbox not installed");
			Assert.IsInstanceOfType(vehicleContainer._engine, typeof (CombustionEngine), "engine not installed");
			Assert.IsInstanceOfType(vehicleContainer._vehicle, typeof (Vehicle), "vehicle not installed");

			var gearbox = vehicleContainer._gearbox as Gearbox;
			Assert.IsNotNull(gearbox);


			// -- shiftpolygon downshift 

			Assert.AreEqual(600.RPMtoRad().Double(), gearbox.Data[1].ShiftPolygon.Downshift[0].AngularSpeed.Double(), 0.0001);
			Assert.AreEqual(0.0, gearbox.Data[1].ShiftPolygon.Downshift[0].Torque.Double(), 0.0001);

			Assert.AreEqual(600.RPMtoRad().Double(), gearbox.Data[1].ShiftPolygon.Downshift[1].AngularSpeed.Double(), 0.0001);
			Assert.AreEqual(266.8277, gearbox.Data[1].ShiftPolygon.Downshift[1].Torque.Double(), 0.1);

			Assert.AreEqual(1310.7646.RPMtoRad().Double(), gearbox.Data[1].ShiftPolygon.Downshift[2].AngularSpeed.Double(), 0.1);
			Assert.AreEqual(899, gearbox.Data[1].ShiftPolygon.Downshift[2].Torque.Double(), 0.0001);

			// -- shiftpolygon upshift

			Assert.AreEqual(1531.5293.RPMtoRad().Double(), gearbox.Data[1].ShiftPolygon.Upshift[0].AngularSpeed.Double(), 0.1);
			Assert.AreEqual(0, gearbox.Data[1].ShiftPolygon.Upshift[0].Torque.Double(), 0.0001);

			Assert.AreEqual(1531.5293.RPMtoRad().Double(), gearbox.Data[1].ShiftPolygon.Upshift[1].AngularSpeed.Double(), 0.1);
			Assert.AreEqual(459.881, gearbox.Data[1].ShiftPolygon.Upshift[1].Torque.Double(), 0.1);

			Assert.AreEqual(2421.RPMtoRad().Double(), gearbox.Data[1].ShiftPolygon.Upshift[2].AngularSpeed.Double(), 0.1);
			Assert.AreEqual(899, gearbox.Data[1].ShiftPolygon.Upshift[2].Torque.Double(), 0.1);
		}

		[TestMethod]
		public void CreateEngineeringSimulationRun()
		{
			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineeringMode);

			factory.DataReader.SetJobFile(EngineeringJobFile);

			var run = factory.SimulationRuns().First();

			var vehicleContainer = (VehicleContainer) run.GetContainer();
			Assert.AreEqual(10, vehicleContainer._components.Count);

			Assert.IsInstanceOfType(vehicleContainer._gearbox, typeof (Gearbox), "gearbox not installed");
			Assert.IsInstanceOfType(vehicleContainer._engine, typeof (CombustionEngine), "engine not installed");
			Assert.IsInstanceOfType(vehicleContainer._vehicle, typeof (Vehicle), "vehicle not installed");
		}
	}
}