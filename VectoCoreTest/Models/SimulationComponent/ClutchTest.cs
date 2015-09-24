using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class ClutchTest
	{
		private const string CoachEngine = @"TestData\Components\24t Coach.veng";

		[TestMethod]
		public void TestClutch()
		{
			var container = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var gearbox = new MockGearbox(container);

			var clutch = new Clutch(container, engineData);

			var inPort = clutch.InPort();
			var outPort = new MockTnOutPort();

			inPort.Connect(outPort);

			var clutchOutPort = clutch.OutPort();

			var driver = new MockDriver(container);

			//Test - Clutch slipping
			clutchOutPort.Request(0.SI<Second>(), 0.SI<Second>(), 100.SI<NewtonMeter>(), 0.SI<PerSecond>());

			Assert.AreEqual(0, outPort.Torque.Value(), 0.001);
			Assert.AreEqual(62.119969, outPort.AngularVelocity.Value(), 0.001);

			clutchOutPort.Request(0.SI<Second>(), 0.SI<Second>(), 100.SI<NewtonMeter>(), 30.SI<PerSecond>());

			Assert.AreEqual(48.293649, outPort.Torque.Value(), 0.001);
			Assert.AreEqual(62.119969, outPort.AngularVelocity.Value(), 0.001);

			//Test - Clutch opened
			driver.VehicleStopped = true;
			clutchOutPort.Request(0.SI<Second>(), 0.SI<Second>(), 100.SI<NewtonMeter>(), 30.SI<PerSecond>());

			Assert.AreEqual(0, outPort.Torque.Value(), 0.001);
			Assert.AreEqual(engineData.IdleSpeed.Value(), outPort.AngularVelocity.Value(), 0.001);

			//Test - Clutch closed
			driver.VehicleStopped = false;
			clutchOutPort.Request(0.SI<Second>(), 0.SI<Second>(), 100.SI<NewtonMeter>(), 80.SI<PerSecond>());

			Assert.AreEqual(100.0, outPort.Torque.Value(), 0.001);
			Assert.AreEqual(80.0, outPort.AngularVelocity.Value(), 0.001);
		}
	}
}