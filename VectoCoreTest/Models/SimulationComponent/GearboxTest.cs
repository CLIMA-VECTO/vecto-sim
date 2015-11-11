﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Integration.SimulationRuns;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class GearboxTest
	{
		protected string GearboxDataFile = @"TestData\Components\24t Coach.vgbx";
		protected string EngineDataFile = @"TestData\Components\24t Coach.veng";

		private static Logger Log = LogManager.GetLogger(typeof(FullPowerTrain).ToString());

		public const string CycleFile = @"TestData\Integration\FullPowerTrain\1-Gear-Test-dist.vdri";
		public const string CoachCycleFile = @"TestData\Integration\FullPowerTrain\Coach.vdri";
		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		public const string AccelerationFile = @"TestData\Components\Coach.vacc";

		public const string IndirectLossMap = @"TestData\Components\Indirect Gear.vtlm";
		public const string DirectLossMap = @"TestData\Components\Direct Gear.vtlm";
		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";


		public TestContext TestContext { get; set; }

		// todo: add realistic FullLoadCurve
		private static GearboxData CreateGearboxData()
		{
			var ratios = new[] { 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };

			return new GearboxData {
				Gears = ratios.Select((ratio, i) =>
					Tuple.Create((uint)i,
						new GearData {
							FullLoadCurve = FullLoadCurve.ReadFromFile(GearboxFullLoadCurveFile),
							LossMap = TransmissionLossMap.ReadFromFile((i != 6) ? IndirectLossMap : DirectLossMap, ratio,
								string.Format("Gear {0}", i)),
							Ratio = ratio,
							ShiftPolygon = ShiftPolygon.ReadFromFile(GearboxShiftPolygonFile)
						}))
					.ToDictionary(k => k.Item1 + 1, v => v.Item2),
				ShiftTime = 2.SI<Second>(),
				Inertia = 0.SI<KilogramSquareMeter>(),
				TractionInterruption = 1.SI<Second>(),
			};
		}


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
			var gearbox = new Gearbox(container, gearboxData, new AMTShiftStrategy(gearboxData, container));
			var driver = new MockDriver(container);
			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);
			container.Engine = port;

			gearbox.Initialize(0.SI<NewtonMeter>(), 0.RPMtoRad());

			var ratio = 6.38;
			var absTime = 0.SI<Second>();
			var dt = 2.SI<Second>();
			var t = 2600.SI<NewtonMeter>();
			var n = 1600.RPMtoRad();
			var response = gearbox.OutPort().Request(absTime, dt, t * ratio, n / ratio);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			absTime += dt;
			t = -1300.SI<NewtonMeter>();
			n = 1000.RPMtoRad();
			response = gearbox.OutPort().Request(absTime, dt, t * ratio, n / ratio);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(absTime, port.AbsTime);
			Assert.AreEqual(dt, port.Dt);
			Assert.AreEqual(n, port.AngularVelocity);
			Assert.AreEqual(t, port.Torque);
		}

		[TestMethod]
		public void Gearbox_IntersectFullLoadCurves()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData, new AMTShiftStrategy(gearboxData, container));
			var driver = new MockDriver(container);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);
			container.Engine = port;

			gearbox.Initialize(0.SI<NewtonMeter>(), 0.RPMtoRad());

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

				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
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
			var gearboxData = CreateGearboxData();
			var gearbox = new Gearbox(container, gearboxData, new AMTShiftStrategy(gearboxData, container));

			var driver = new MockDriver(container);

			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);
			container.Engine = port;

			var ratios = new[] { 0.0, 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };
			// the first element 0.0 is just a placeholder for axlegear, not used in this test

			var expected = new[] {
				new { gear = 1, t = -1000, n = 600, loss = 28.096, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = -1000, n = 600, loss = 28.096, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = -1000, n = 600, loss = 13.096, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 850, n = 600, loss = 12.346, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 2050, n = 1200, loss = 21.382, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 50, n = 600, loss = 9.096, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 2450, n = 800, loss = 58.11, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 850, n = 800, loss = 26.11, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 850, n = 0, loss = 0.0, responseType = typeof(ResponseSuccess) },
				new { gear = 1, t = 850, n = 200, loss = 23.07, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 50, n = 600, loss = 9.096, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 2050, n = 1200, loss = 52.132, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 850, n = 800, loss = 26.11, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 850, n = 0, loss = 0.0, responseType = typeof(ResponseSuccess) },
				new { gear = 2, t = 850, n = 600, loss = 25.096, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = -1000, n = 0, loss = 0.0, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 850, n = 0, loss = 0.0, responseType = typeof(ResponseSuccess) },
				new { gear = 7, t = 2450, n = 0, loss = 0.0, responseType = typeof(ResponseSuccess) },
			};

			var absTime = 0.SI<Second>();
			var dt = 2.SI<Second>();

			foreach (var exp in expected) {
				gearbox.OutPort().Initialize(0.SI<NewtonMeter>(), 0.SI<PerSecond>());

				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();
				var expectedLoss = exp.loss.SI<NewtonMeter>();

				var torque = (expectedT - expectedLoss) * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				gearbox.Gear = (uint)exp.gear;
				var response = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(response, exp.responseType, exp.ToString());

				if (exp.responseType == typeof(ResponseSuccess)) {
					AssertHelper.AreRelativeEqual(absTime, port.AbsTime, message: exp.ToString());
					AssertHelper.AreRelativeEqual(dt, port.Dt, message: exp.ToString());
					AssertHelper.AreRelativeEqual(expectedN, port.AngularVelocity, message: exp.ToString());
					AssertHelper.AreRelativeEqual(expectedT, port.Torque, message: exp.ToString(), toleranceFactor: 1e-5);
				}
			}
		}


		[TestMethod]
		public void Gearbox_ShiftDown()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData, new AMTShiftStrategy(gearboxData, container));

			var driver = new MockDriver(container);
			var port = new MockTnOutPort();
			gearbox.InPort().Connect(port);
			var vehicle = new MockVehicle(container) { MyVehicleSpeed = 10.SI<MeterPerSecond>() };
			container.Engine = port;

			var ratios = new[] { 0.0, 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };
			// the first element 0.0 is just a placeholder for axlegear, not used in this test

			var expected = new[] {
				new { gear = 8, newGear = 7, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 7, newGear = 6, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 6, newGear = 5, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 5, newGear = 4, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 4, newGear = 3, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 3, newGear = 2, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 2, newGear = 1, t = 1500, n = 750, responseType = typeof(ResponseGearShift) },
				new { gear = 1, newGear = 1, t = 1200, n = 700, responseType = typeof(ResponseSuccess) },
				new { gear = 8, newGear = 1, t = 15000, n = 50, responseType = typeof(ResponseGearShift) }
			};

			var absTime = 0.SI<Second>();
			var dt = 2.SI<Second>();

			gearbox.OutPort().Initialize(1.SI<NewtonMeter>(), 1.SI<PerSecond>());

			foreach (var exp in expected) {
				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();

				var torque = expectedT * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				gearbox.Gear = (uint)exp.gear;
				var gearShiftResponse = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(gearShiftResponse, exp.responseType, exp.ToString());

				absTime += dt;
				var successResponse = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(successResponse, typeof(ResponseSuccess), exp.ToString());
				Assert.AreEqual((uint)exp.newGear, container.Gear, exp.ToString());
				absTime += dt;
			}
		}

		[TestMethod]
		public void Gearbox_ShiftUp()
		{
			var container = new VehicleContainer();
			var gearboxData = DeclarationModeSimulationDataReader.CreateGearboxDataFromFile(GearboxDataFile, EngineDataFile);
			var gearbox = new Gearbox(container, gearboxData, new AMTShiftStrategy(gearboxData, container));
			var driver = new MockDriver(container);
			var vehicle = new MockVehicle(container) { MyVehicleSpeed = 10.SI<MeterPerSecond>() };
			var port = new MockTnOutPort();
			container.Engine = port;
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

			gearbox.OutPort().Initialize(1.SI<NewtonMeter>(), 1.SI<PerSecond>());

			absTime += dt;
			foreach (var exp in expected) {
				var expectedT = exp.t.SI<NewtonMeter>();
				var expectedN = exp.n.RPMtoRad();

				var torque = expectedT * ratios[exp.gear];
				var angularVelocity = expectedN / ratios[exp.gear];

				gearbox.Gear = (uint)exp.gear;
				var response = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(response, exp.responseType, exp.ToString());

				absTime += dt;
				response = gearbox.OutPort().Request(absTime, dt, torque, angularVelocity);
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess), exp.ToString());
				Assert.AreEqual((uint)exp.newGear, container.Gear, exp.ToString());
				absTime += dt;
			}
		}
	}
}