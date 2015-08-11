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
	public class GearboxTest
	{
		protected string GearboxDataFile = @"TestData\Components\24t Coach.vgbx";

		public TestContext TestContext { get; set; }

		[TestMethod]
		public void AxleGearTest()
		{
			var vehicle = new VehicleContainer();
			var gbxData = EngineeringModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile);
			//Gears gearData = new Gears();
			var axleGear = new AxleGear(vehicle, gbxData.AxleGearData);

			var mockPort = new MockTnOutPort();
			axleGear.InPort().Connect(mockPort);

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();

			var rdyn = 520;
			var speed = 20.320;


			var angSpeed = SpeedToAngularSpeed(speed, rdyn);
			var PvD = 279698.4.SI<Watt>();
			// Double.Parse(TestContext.DataRow["PowerGbxOut"].ToString(), CultureInfo.InvariantCulture).SI<Watt>();

			var torqueToWheels = Formulas.PowerToTorque(PvD, angSpeed);
			//var torqueFromEngine = 0.SI<NewtonMeter>();

			axleGear.Request(absTime, dt, torqueToWheels, angSpeed);

			var loss = 9401.44062.SI<Watt>();

			Assert.AreEqual(Formulas.PowerToTorque(PvD + loss, angSpeed * gbxData.AxleGearData.Ratio).Value(),
				mockPort.Torque.Value(), 0.01, "Torque Engine Side");
			Assert.AreEqual((angSpeed * gbxData.AxleGearData.Ratio).Value(), mockPort.AngularVelocity.Value(), 0.01,
				"Torque Engine Side");
		}

		protected PerSecond SpeedToAngularSpeed(double v, double r)
		{
			return ((60 * v) / (2 * r * Math.PI / 1000)).RPMtoRad();
		}

		[TestMethod]
		public void Gearbox_Request()
		{
			var container = new VehicleContainer();
			var gearboxData = EngineeringModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			var absTime = 0.SI<Second>();
			var dt = 0.5.SI<Second>();
			var torque = 500.SI<NewtonMeter>();
			var angularVelocity = 1400.RPMtoRad();

			var ratio = 1; // todo: set correct ratio
			var expectedN = angularVelocity * ratio;
			var expectedT = 500.SI<NewtonMeter>(); //todo: set correct value

			gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
			AssertHelper.AreRelativeEqual(absTime, port.AbsTime);
			AssertHelper.AreRelativeEqual(dt, port.Dt);
			AssertHelper.AreRelativeEqual(expectedN, port.AngularVelocity);
			AssertHelper.AreRelativeEqual(expectedT, port.Torque);

			// todo check for different ranges and gears
			// todo set initial gear

			Assert.Inconclusive();
		}

		[TestMethod]
		public void Gearbox_ShiftUp()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Gearbox_ShiftDown()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Gearbox_Overload()
		{
			Assert.Inconclusive();
		}
	}
}