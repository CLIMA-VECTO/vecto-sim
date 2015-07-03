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
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var gearbox = new DummyGearbox(vehicle);

			var clutch = new Clutch(vehicle, engineData);

			var inPort = clutch.InShaft();
			var outPort = new MockTnOutPort();

			inPort.Connect(outPort);

			var clutchOutPort = clutch.OutShaft();

			//Test - Clutch slipping
			gearbox.CurrentGear = 1;
			clutchOutPort.Request(new TimeSpan(), new TimeSpan(), 100.SI<NewtonMeter>(), 30.SI<PerSecond>());

			Assert.AreEqual(48.293649, (double) outPort.Torque, 0.001);
			Assert.AreEqual(62.119969, (double) outPort.AngularVelocity, 0.001);

			//Test - Clutch opened
			gearbox.CurrentGear = 0;
			clutchOutPort.Request(new TimeSpan(), new TimeSpan(), 100.SI<NewtonMeter>(), 30.SI<PerSecond>());

			Assert.AreEqual(0, (double) outPort.Torque, 0.001);
			Assert.AreEqual((double) engineData.IdleSpeed, (double) outPort.AngularVelocity, 0.001);

			//Test - Clutch closed
			gearbox.CurrentGear = 1;
			clutchOutPort.Request(new TimeSpan(), new TimeSpan(), 100.SI<NewtonMeter>(), 80.SI<PerSecond>());

			Assert.AreEqual(100.0, (double) outPort.Torque, 0.001);
			Assert.AreEqual(80.0, (double) outPort.AngularVelocity, 0.001);
		}
	}
}