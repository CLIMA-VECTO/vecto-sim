using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
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
			//GearData gearData = new GearData();
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
				mockPort.Torque.Value(), 0.01,
				"Torque Engine Side")
				;
			Assert.AreEqual((angSpeed * gbxData.AxleGearData.Ratio).Value(), mockPort.AngularVelocity.Value(), 0.01,
				"Torque Engine Side");
		}

		protected PerSecond SpeedToAngularSpeed(double v, double r)
		{
			return ((60 * v) / (2 * r * Math.PI / 1000)).RPMtoRad();
		}
	}
}