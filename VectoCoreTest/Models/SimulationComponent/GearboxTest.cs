using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Targets;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
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
		protected string EngineDataFile = @"TestData\Components\24t Coach.veng";

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
		public void Gearbox_LessThanTwoGearsException()
		{
			var wrongFile = @"TestData\Components\24t Coach LessThanTwoGears.vgbx";
			AssertHelper.Exception<VectoSimulationException>(
				() => DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(wrongFile, EngineDataFile),
				"At least two Gear-Entries must be defined in Gearbox: 1 Axle-Gear and at least 1 Gearbox-Gear!");
		}

		[TestMethod]
		public void Gearbox_LossMapInterpolationFail()
		{
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(new VehicleContainer(), gearboxData);

			AssertHelper.Exception<VectoException>(
				() => gearbox.OutPort().Request(0.SI<Second>(), 1.SI<Second>(), 5000.SI<NewtonMeter>(), 10000.SI<PerSecond>()),
				"Failed to interpolate in TransmissionLossMap. angularVelocity: 63800.0000 [1/s], torque: 5000.0000 [Nm]");
		}

		[TestMethod]
		public void Gearbox_IntersectFullLoadCurves()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			var ratio = 6.38;

			//todo: serach for a overload point in gearbox fullloadcurve
			//todo: serach for a overload point in engine fullloadcurve
			var expected = new[] {
				new { t = 2129, n = 1600, loss = 55.0 }, //todo: calc exact loss
				new { t = 2250, n = 1200, loss = 55.0 }, //todo: calc exact loss
			};

			foreach (var exp in expected) {
				var torque = (exp.t.SI<NewtonMeter>() - exp.loss.SI<NewtonMeter>()) * ratio;
				var angularVelocity = exp.n.RPMtoRad() / ratio;

				var response = gearbox.OutPort().Request(0.SI<Second>(), 1.SI<Second>(), torque, angularVelocity);
				Assert.IsInstanceOfType(response, typeof(ResponseFailOverload));
			}

			Assert.Inconclusive("Test if the intersection of two fullloadcurves is correct");
		}

		[TestMethod]
		public void Gearbox_Request()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			var ratios = new[] { 0.0, 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };
			// the first element 0.0 is just a placeholder for axlegear, not used in this test

			var expected = new[] {
				new { gear = 1, t = 50, n = 800, loss = 10.108 },
				new { gear = 1, t = 2450, n = 800, loss = 58.11 },
				new { gear = 1, t = -1000, n = 800, loss = 29.11 },
				new { gear = 1, t = 850, n = 800, loss = 26.11 },
				new { gear = 1, t = 850, n = 0, loss = 22.06 },
				new { gear = 1, t = 850, n = 200, loss = 23.07 },
				new { gear = 1, t = 850, n = 2000, loss = 32.18 },
				new { gear = 2, t = 50, n = 800, loss = 10.108 },
				new { gear = 2, t = 2450, n = 800, loss = 58.11 },
				new { gear = 2, t = -1000, n = 800, loss = 29.11 },
				new { gear = 2, t = 850, n = 800, loss = 26.11 },
				new { gear = 2, t = 850, n = 0, loss = 22.06 },
				new { gear = 2, t = 850, n = 200, loss = 23.07 },
				new { gear = 2, t = 850, n = 2000, loss = 32.18 },
				new { gear = 7, t = -1000, n = 0, loss = 10.06 },
				new { gear = 7, t = -1000, n = 1200, loss = 16.132 },
				new { gear = 7, t = -1000, n = 2000, loss = 20.18 },
				new { gear = 7, t = 850, n = 0, loss = 9.31 },
				new { gear = 7, t = 850, n = 1200, loss = 15.382 },
				new { gear = 7, t = 850, n = 2000, loss = 19.43 },
				new { gear = 7, t = 2450, n = 0, loss = 17.31 },
				new { gear = 7, t = 2450, n = 1200, loss = 23.382 },
				new { gear = 7, t = 2450, n = 2000, loss = 27.43 },
			};

			for (var i = 0; i < expected.Length; i++) {
				var exp = expected[i];
				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();
				var expectedLoss = exp.loss.SI<NewtonMeter>();

				var torque = (expectedT - expectedLoss) * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				gearbox.Gear = (uint)exp.gear;
				gearbox.OutPort().Request(0.SI<Second>(), 1.SI<Second>(), torque, angularVelocity);
				AssertHelper.AreRelativeEqual(0.SI<Second>(), port.AbsTime, message: i.ToString());
				AssertHelper.AreRelativeEqual(1.SI<Second>(), port.Dt, message: i.ToString());
				AssertHelper.AreRelativeEqual(expectedN, port.AngularVelocity, message: i.ToString());
				AssertHelper.AreRelativeEqual(expectedT, port.Torque, message: i.ToString());
			}
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