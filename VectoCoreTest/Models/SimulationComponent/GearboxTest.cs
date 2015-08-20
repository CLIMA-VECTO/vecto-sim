using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
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
			var container = new VehicleContainer();
			var gearbox = new Gearbox(container, gearboxData);

			container.Gear = 1;

			var response = gearbox.OutPort()
				.Request(0.SI<Second>(), 1.SI<Second>(), 1500.SI<NewtonMeter>() / 6.38, 700.SI<PerSecond>() * 6.38);

			Assert.IsInstanceOfType(response, typeof(ResponseGearboxOverload));
		}

		[TestMethod]
		public void Gearbox_IntersectFullLoadCurves()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			container.Gear = 1;

			var ratio = 6.38;

			var expected = new[] {
				new { t = 2500, n = 900 },
				new { t = 2500, n = 1700 },
				new { t = 2129, n = 1600 }, // to high for gearbox, but ok for engine
			};

			foreach (var exp in expected) {
				var torque = exp.t.SI<NewtonMeter>() * ratio;
				var angularVelocity = exp.n.RPMtoRad() / ratio;

				var response = gearbox.OutPort().Request(0.SI<Second>(), 1.SI<Second>(), torque, angularVelocity);
				Assert.IsInstanceOfType(response, typeof(ResponseGearboxOverload));
			}

			var expectedCorrect = new[] {
				new { t = 500, n = 700 },
				new { t = 1500, n = 1100 },
				new { t = 2000, n = 1500 },
				new { t = 2240, n = 1200 } // ok for gearbox but would be to high for engine
			};

			foreach (var exp in expectedCorrect) {
				var torque = exp.t.SI<NewtonMeter>() * ratio;
				var angularVelocity = exp.n.RPMtoRad() / ratio;

				var response = gearbox.OutPort().Request(0.SI<Second>(), 1.SI<Second>(), torque, angularVelocity);
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			}
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
				new { gear = 1, t = 50, n = 800, loss = 10.108, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 2450, n = 800, loss = 58.11, responseType = typeof(ResponseGearboxOverload) },
				new { gear = 1, t = -1000, n = 800, loss = 29.11, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 850, n = 800, loss = 26.11, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 850, n = 0, loss = 22.06, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 850, n = 200, loss = 23.07, responseType = typeof(ResponseGearboxOverload) },
				new { gear = 2, t = 50, n = 800, loss = 10.108, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 2450, n = 800, loss = 58.11, responseType = typeof(ResponseGearShift) },
				new { gear = 2, t = -1000, n = 800, loss = 29.11, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 850, n = 800, loss = 26.11, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 850, n = 0, loss = 22.06, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 850, n = 400, loss = 11.334, responseType = typeof(ResponseGearShift) },
				new { gear = 2, t = 850, n = 2000, loss = 32.18, responseType = typeof(ResponseGearShift) },
				new { gear = 7, t = -1000, n = 0, loss = 10.06, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = -1000, n = 1200, loss = 16.132, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 850, n = 0, loss = 9.31, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 850, n = 1200, loss = 15.382, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 850, n = 2000, loss = 19.43, responseType = typeof(ResponseGearShift) },
				new { gear = 7, t = 2450, n = 0, loss = 17.31, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 2450, n = 1200, loss = 23.382, responseType = typeof(ResponseGearboxOverload) }
			};

			var absTime = 0.SI<Second>();
			var dt = 2.SI<Second>();

			foreach (var exp in expected) {
				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();
				var expectedLoss = exp.loss.SI<NewtonMeter>();

				var torque = (expectedT - expectedLoss) * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				container.Gear = (uint)exp.gear;
				var response = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(response, exp.responseType, exp.ToString());

				if (angularVelocity.IsEqual(0)) {
					expectedT = 0.SI<NewtonMeter>();
				}

				if (exp.responseType == typeof(ResponseSuccess)) {
					AssertHelper.AreRelativeEqual(absTime, port.AbsTime, message: exp.ToString());
					AssertHelper.AreRelativeEqual(dt, port.Dt, message: exp.ToString());
					AssertHelper.AreRelativeEqual(expectedN, port.AngularVelocity, message: exp.ToString());
					AssertHelper.AreRelativeEqual(expectedT, port.Torque, message: exp.ToString());
				}
				absTime += dt;
			}
		}


		[TestMethod]
		public void Gearbox_ShiftDown()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			var ratios = new[] { 0.0, 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };
			// the first element 0.0 is just a placeholder for axlegear, not used in this test

			var expected = new[] {
				new { gear = 8, newGear = 7, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 7, newGear = 6, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 6, newGear = 5, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 5, newGear = 4, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 4, newGear = 3, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 3, newGear = 2, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 2, newGear = 1, t = 1500, n = 700, responseType = typeof(ResponseGearShift) },
				new { gear = 1, newGear = 1, t = 1200, n = 700, responseType = typeof(ResponseSuccess) },
				new { gear = 8, newGear = 1, t = 10000, n = 120, responseType = typeof(ResponseGearShift) }
			};

			var absTime = 0.SI<Second>();
			var dt = 2.SI<Second>();

			foreach (var exp in expected) {
				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();

				var torque = expectedT * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				container.Gear = (uint)exp.gear;
				var response = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(response, exp.responseType, exp.ToString());
				Assert.AreEqual((uint)exp.newGear, container.Gear, exp.ToString());
				absTime += dt;
			}
		}

		[TestMethod]
		public void Gearbox_ShiftUp()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			var ratios = new[] { 0.0, 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };
			// the first element 0.0 is just a placeholder for axlegear, not used in this test

			var expected = new[] {
				new { gear = 7, newGear = 8, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 6, newGear = 7, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 5, newGear = 6, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 4, newGear = 5, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 3, newGear = 4, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 2, newGear = 3, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 1, newGear = 2, t = 1000, n = 1400, responseType = typeof(ResponseGearShift) },
				new { gear = 8, newGear = 8, t = 1000, n = 1400, responseType = typeof(ResponseSuccess) },
				new { gear = 1, newGear = 8, t = 200, n = 9000, responseType = typeof(ResponseGearShift) }
			};

			var absTime = 0.SI<Second>();
			var dt = 2.SI<Second>();

			foreach (var exp in expected) {
				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();

				var torque = expectedT * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				container.Gear = (uint)exp.gear;
				var response = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(response, exp.responseType, exp.ToString());
				Assert.AreEqual((uint)exp.newGear, container.Gear, exp.ToString());
				absTime += dt;
			}
		}

		[TestMethod]
		public void Gearbox_NoGear()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);

			container.Gear = 0;
			var response = gearbox.OutPort()
				.Request(0.SI<Second>(), 1.SI<Second>(), 50000000.SI<NewtonMeter>(), 1000000.SI<PerSecond>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			AssertHelper.AreRelativeEqual(0.SI<Second>(), port.AbsTime);
			AssertHelper.AreRelativeEqual(1.SI<Second>(), port.Dt);
			AssertHelper.AreRelativeEqual(0.SI<PerSecond>(), port.AngularVelocity);
			AssertHelper.AreRelativeEqual(0.SI<NewtonMeter>(), port.Torque);
		}
	}
}