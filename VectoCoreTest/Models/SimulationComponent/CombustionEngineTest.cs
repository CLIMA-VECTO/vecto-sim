using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class CombustionEngineTest
	{
		protected double Tolerance = 1E-3;

		private const string CoachEngine = @"TestData\Components\24t Coach.veng";

		private const string TruckEngine = @"TestData\Components\40t_Long_Haul_Truck.veng";

		public TestContext TestContext { get; set; }

		[ClassInitialize]
		public static void ClassInitialize(TestContext ctx)
		{
			AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
		}

		/// <summary>
		/// Assert an expected Exception.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <param name="message"></param>
		public static void AssertException<T>(Action func, string message = null) where T : Exception
		{
			try {
				func();
				Assert.Fail("Expected Exception {0}, but no exception occured.", typeof(T));
			} catch (T ex) {
				if (message != null) {
					Assert.AreEqual(message, ex.Message);
				}
			}
		}

		[TestMethod]
		public void TestEngineHasOutPort()
		{
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			var port = engine.OutPort();
			Assert.IsNotNull(port);
		}

		[TestMethod]
		public void TestOutPortRequestNotFailing()
		{
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			new EngineOnlyGearbox(vehicle);

			var port = engine.OutPort();

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();
			var torque = 400.SI<NewtonMeter>();
			var engineSpeed = 1500.RPMtoRad();

			port.Request(absTime, dt, torque, engineSpeed);
		}

		[TestMethod]
		public void TestSimpleModalData()
		{
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);
			var gearbox = new EngineOnlyGearbox(vehicle);
			var port = engine.OutPort();

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();

			var torque = 0.SI<NewtonMeter>();
			var engineSpeed = 600.RPMtoRad();
			var dataWriter = new MockModalDataWriter();

			for (var i = 0; i < 21; i++) {
				port.Request(absTime, dt, torque, engineSpeed);
				engine.CommitSimulationStep(dataWriter);
				if (i > 0) {
					dataWriter.CommitSimulationStep(absTime, dt);
				}
				absTime += dt;
			}

			engineSpeed = 644.4445.RPMtoRad();
			port.Request(absTime, dt, Formulas.PowerToTorque(2329.973.SI<Watt>(), engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);

			AssertHelper.AreRelativeEqual(1152.40304, ((SI)dataWriter[ModalResultField.PaEng]).Value());

			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			var power = new[] { 569.3641, 4264.177 };
			;
			for (var i = 0; i < 2; i++) {
				port.Request(absTime, dt, Formulas.PowerToTorque(power[i].SI<Watt>(), engineSpeed), engineSpeed);
				engine.CommitSimulationStep(dataWriter);
				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
			}

			engineSpeed = 869.7512.RPMtoRad();
			port.Request(absTime, dt, Formulas.PowerToTorque(7984.56.SI<Watt>(), engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);


			Assert.AreEqual(7108.32, ((SI)dataWriter[ModalResultField.PaEng]).Value(), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			engineSpeed = 644.4445.RPMtoRad();
			port.Request(absTime, dt, Formulas.PowerToTorque(1351.656.SI<Watt>(), engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);

			Assert.AreEqual(-7108.32, ((SI)dataWriter[ModalResultField.PaEng]).Value(), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			dataWriter.Data.WriteToFile(@"test1.csv");
		}


		[TestMethod]
		[DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\TestData\\EngineFullLoadJumps.csv",
			"EngineFullLoadJumps#csv", DataAccessMethod.Sequential)]
		public void TestEngineFullLoadJump()
		{
			var vehicleContainer = new VehicleContainer();
			var gearbox = new EngineOnlyGearbox(vehicleContainer);
			var engineData =
				EngineeringModeSimulationDataReader.CreateEngineDataFromFile(
					TestContext.DataRow["EngineFile"].ToString());
			var engine = new EngineOnlyCombustionEngine(vehicleContainer, engineData);

			gearbox.InPort().Connect(engine.OutPort());

			var expectedResults = VectoCSVFile.Read(TestContext.DataRow["ResultFile"].ToString());

			var requestPort = gearbox.OutPort();

			//var modalData = new ModalDataWriter(string.Format("load_jump_{0}.csv", TestContext.DataRow["TestName"].ToString()));
			var modalData = new MockModalDataWriter();

			var idlePower = double.Parse(TestContext.DataRow["initialIdleLoad"].ToString()).SI<Watt>();

			var angularSpeed = double.Parse(TestContext.DataRow["rpm"].ToString()).RPMtoRad();

			var t = 0.SI<Second>();
			var dt = 0.1.SI<Second>();

			for (; t < 2; t += dt) {
				requestPort.Request(t, dt, Formulas.PowerToTorque(idlePower, angularSpeed), angularSpeed);
				engine.CommitSimulationStep(modalData);
			}

			var i = 0;
			// dt = TimeSpan.FromSeconds(double.Parse(TestContext.DataRow["dt"].ToString(), CultureInfo.InvariantCulture));
			// dt = TimeSpan.FromSeconds(expectedResults.Rows[i].ParseDouble(0)) - t;
			var engineLoadPower = engineData.FullLoadCurve.FullLoadStationaryPower(angularSpeed);
			idlePower = double.Parse(TestContext.DataRow["finalIdleLoad"].ToString()).SI<Watt>();
			for (; t < 25; t += dt, i++) {
				dt = (expectedResults.Rows[i + 1].ParseDouble(0) - expectedResults.Rows[i].ParseDouble(0)).SI<Second>();
				if (t >= 10.SI<Second>()) {
					engineLoadPower = idlePower;
				}
				requestPort.Request(t, dt, Formulas.PowerToTorque(engineLoadPower, angularSpeed), angularSpeed);
				modalData[ModalResultField.time] = t;
				modalData[ModalResultField.simulationInterval] = dt;
				engine.CommitSimulationStep(modalData);
				// todo: compare results...
				Assert.AreEqual(expectedResults.Rows[i].ParseDouble(0), t.Value(), 0.001, "Time");
				Assert.AreEqual(expectedResults.Rows[i].ParseDouble(1), ((SI)modalData[ModalResultField.Pe_full]).Value(), 0.1,
					string.Format("Load in timestep {0}", t));
				modalData.CommitSimulationStep();
			}
			modalData.Finish();
		}

		[TestMethod]
		public void EngineIdleJump()
		{
			var container = new VehicleContainer();
			var gearbox = new MockGearbox(container);
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);

			var engine = new CombustionEngine(container, engineData);
			var clutch = new Clutch(container, engineData, engine.IdleController);

			var driver = new MockDriver(container);

			var aux = new Auxiliary(container);
			aux.AddConstant("", 5000.SI<Watt>());

			gearbox.Gear = 1;

			//gearbox.InPort().Connect(engine.OutPort());
			gearbox.InPort().Connect(clutch.OutPort());
			clutch.InPort().Connect(aux.OutPort());
			aux.InPort().Connect(engine.OutPort());
			engine.IdleController.RequestPort = clutch.IdleControlPort;

//			var expectedResults = VectoCSVFile.Read(TestContext.DataRow["ResultFile"].ToString());

			var requestPort = gearbox.OutPort();

			//vehicleContainer.DataWriter = new ModalDataWriter("engine_idle_test.csv");
			var dataWriter = new MockModalDataWriter();
			container.DataWriter = dataWriter;

			var torque = 1200.SI<NewtonMeter>();
			var angularVelocity = 800.RPMtoRad();

			// initialize engine...

			gearbox.Initialize(torque, angularVelocity);

			var absTime = 0.SI<Second>();
			var dt = Constants.SimulationSettings.TargetTimeInterval;

			var response = requestPort.Request(absTime, dt, torque, angularVelocity);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			container.CommitSimulationStep(absTime, dt);
			var row = dataWriter.Data.Rows.Cast<DataRow>().Last();
			Assert.AreEqual(105530.96491487339.SI<Watt>(), row[ModalResultField.Pe_eng.GetName()]);
			Assert.AreEqual(5000.SI<Watt>(), row[ModalResultField.Paux.GetName()]);
			Assert.AreEqual(800.RPMtoRad(), row[ModalResultField.n.GetName()]);

			absTime += dt;

			// actual test...

			gearbox.Gear = 0;
			torque = 0.SI<NewtonMeter>();

			response = gearbox.Request(absTime, dt, torque, angularVelocity);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			container.CommitSimulationStep(absTime, dt);
			row = dataWriter.Data.Rows.Cast<DataRow>().Last();

			Assert.AreEqual(5000.SI<Watt>(), row[ModalResultField.Pe_eng.GetName()]);
			Assert.AreEqual(5000.SI<Watt>(), row[ModalResultField.Paux.GetName()]);
			Assert.AreEqual(800.RPMtoRad(), row[ModalResultField.n.GetName()]);
		}

		[TestMethod]
		public void EngineIdleControllerTestCoach()
		{
			VehicleContainer container;
			CombustionEngine engine;
			ITnOutPort requestPort;
			VehicleContainer(CoachEngine, out container, out engine, out requestPort);


			var absTime = 0.SI<Second>();
			var dt = Constants.SimulationSettings.TargetTimeInterval;

			var angularVelocity = 800.RPMtoRad();
			var torque = 100000.SI<Watt>() / angularVelocity;

			var response = requestPort.Initialize(torque, angularVelocity);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = requestPort.Request(absTime, dt, torque, angularVelocity);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(105000, response.EnginePowerRequest.Value(), Tolerance);
			container.CommitSimulationStep(absTime, dt);
			absTime += dt;

			var engineSpeed = new PerSecond[] { 800.RPMtoRad(), 800.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad() };
			var enginePower = new Watt[] { 5000.SI<Watt>(), 5000.SI<Watt>(), -8601.6308.SI<Watt>(), 5000.SI<Watt>() };

			for (var i = 0; i < engineSpeed.Count(); i++) {
				torque = 0.SI<NewtonMeter>();

				response = requestPort.Request(absTime, dt, torque, null);
				container.CommitSimulationStep(absTime, dt);
				absTime += dt;

				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
				Assert.AreEqual(engineSpeed[i].Value(), engine.PreviousState.EngineSpeed.Value(), Tolerance);
				Assert.AreEqual(enginePower[i].Value(), engine.PreviousState.EnginePower.Value(), Tolerance);
			}
		}


/*
 * VECTO 2.2
| time [s] | Pe_eng [kW] | n [1/min] | Tq_eng [Nm] | Gear [-] |
| 59.5     | 349.981     | 1679.281  | 1990.181    | 8        |
| 60.5     | 5           | 1679.281  | 28.43269    | 0        |
| 61.5     | -19.47213   | 1397.271  | -133.0774   | 0        |
| 62.5     | -18.11888   | 1064.296  | -162.5699   | 0        |
| 63.5     | -11.11163   | 714.1923  | -148.571    | 0        |
| 64.5     | -0.5416708  | 560       | -9.236741   | 0        |
| 65.5     | 5           | 560       | 85.26157    | 0        |
| 66.5     | 5           | 560       | 85.26157    | 0        |
| 67.5     | 5           | 560       | 85.26157    | 0        |
| 68.5     | 5           | 560       | 85.26157    | 0        |
| 69.5     | 5           | 560       | 85.26157    | 0        |
| 70.5     | 308.729     | 1284.139  | 2295.815    | 9        |
		*/

		[TestMethod]
		public void EngineIdleControllerTestTruck()
		{
			VehicleContainer container;
			CombustionEngine engine;
			ITnOutPort requestPort;
			VehicleContainer(TruckEngine, out container, out engine, out requestPort);

			//var dataWriter = new ModalDataWriter("EngienIdle.vmod");
			//container.DataWriter = dataWriter;

			var absTime = 0.SI<Second>();
			var dt = Constants.SimulationSettings.TargetTimeInterval;

			var angularVelocity = 1680.RPMtoRad();
			var torque = 345000.SI<Watt>() / angularVelocity;

			var response = requestPort.Initialize(torque, angularVelocity);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = requestPort.Request(absTime, dt, torque, angularVelocity);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(350000, response.EnginePowerRequest.Value(), Tolerance);
			container.CommitSimulationStep(absTime, dt);
			absTime += dt;

			var engineSpeed = new PerSecond[] {
				1680.RPMtoRad(), 1680.RPMtoRad(), 1467.014.RPMtoRad(), 1272.8658.RPMtoRad(), 1090.989.RPMtoRad(),
				915.3533.RPMtoRad(), 738.599.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(),
				560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(),
				560.RPMtoRad()
			};
			var enginePower = new Watt[] {
				5000.SI<Watt>(), 5000.SI<Watt>(), -32832.8834.SI<Watt>(), -25025.1308.SI<Watt>(), -19267.0360.SI<Watt>(),
				-14890.1962.SI<Watt>(), -11500.7991.SI<Watt>(), -8091.0577.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(),
				5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(),
				5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>()
			};

			var engSpeedResults = new List<dynamic>();
			for (var i = 0; i < engineSpeed.Count(); i++) {
				torque = 0.SI<NewtonMeter>();

				response = requestPort.Request(absTime, dt, torque, null);
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

				container.CommitSimulationStep(absTime, dt);

				engSpeedResults.Add(new {
					absTime,
					engine.PreviousState.EngineSpeed,
					engine.PreviousState.EnginePower
				});
				Assert.AreEqual(engineSpeed[i].Value(), engine.PreviousState.EngineSpeed.Value(), Tolerance);
				Assert.AreEqual(enginePower[i].Value(), engine.PreviousState.EnginePower.Value(), Tolerance);

				absTime += dt;
			}
			//dataWriter.Finish();
		}

		[TestMethod]
		public void EngineIdleControllerTest2Truck()
		{
			VehicleContainer container;
			CombustionEngine engine;
			ITnOutPort requestPort;
			VehicleContainer(TruckEngine, out container, out engine, out requestPort);

			//var dataWriter = new ModalDataWriter("EngienIdle.vmod");
			//container.DataWriter = dataWriter;

			var absTime = 0.SI<Second>();
			var dt = Constants.SimulationSettings.TargetTimeInterval;

			var angularVelocity = 95.5596.SI<PerSecond>();
			var torque = (engine.Data.FullLoadCurve.DragLoadStationaryPower(angularVelocity) - 5000.SI<Watt>()) / angularVelocity;

			var response = requestPort.Initialize(torque, angularVelocity);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = requestPort.Request(absTime, dt, torque, angularVelocity);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(-14829.79713, response.EnginePowerRequest.Value(), Tolerance);
			container.CommitSimulationStep(absTime, dt);
			absTime += dt;

			var engineSpeed = new PerSecond[] {
				1680.RPMtoRad(), 1680.RPMtoRad(), 1467.014.RPMtoRad(), 1272.8658.RPMtoRad(), 1090.989.RPMtoRad(),
				915.3533.RPMtoRad(), 738.599.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(),
				560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(), 560.RPMtoRad(),
				560.RPMtoRad()
			};
			var enginePower = new Watt[] {
				5000.SI<Watt>(), 5000.SI<Watt>(), -32832.8834.SI<Watt>(), -25025.1308.SI<Watt>(), -19267.0360.SI<Watt>(),
				-14890.1962.SI<Watt>(), -11500.7991.SI<Watt>(), -8091.0577.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(),
				5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(),
				5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>(), 5000.SI<Watt>()
			};

			var engSpeedResults = new List<dynamic>();
			torque = 0.SI<NewtonMeter>();
			for (var i = 0; i < engineSpeed.Count(); i++) {
				response = requestPort.Request(absTime, dt, torque, null);
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

				container.CommitSimulationStep(absTime, dt);

				engSpeedResults.Add(new {
					absTime,
					engine.PreviousState.EngineSpeed,
					engine.PreviousState.EnginePower
				});
				//Assert.AreEqual(engineSpeed[i].Value(), engine.PreviousState.EngineSpeed.Value(), Tolerance);
				//Assert.AreEqual(enginePower[i].Value(), engine.PreviousState.EnginePower.Value(), Tolerance);

				absTime += dt;
			}
			//dataWriter.Finish();
		}

		[TestMethod, Ignore]
		public void TestWriteToFile()
		{
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			//engineData.WriteToFile("engineData test output.veng");
		}

		[TestMethod]
		public void Test_EngineData()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var motorway = engineData.WHTCMotorway;
			Assert.AreEqual(motorway.Value(), 0);
			Assert.IsTrue(motorway.HasEqualUnit(new SI().Kilo.Gramm.Per.Watt.Second.ConvertTo()));

			var rural = engineData.WHTCRural;
			Assert.AreEqual(rural.Value(), 0);
			Assert.IsTrue(rural.HasEqualUnit(new SI().Kilo.Gramm.Per.Watt.Second.ConvertTo()));

			var urban = engineData.WHTCUrban;
			Assert.AreEqual(urban.Value(), 0);
			Assert.IsTrue(urban.HasEqualUnit(new SI().Kilo.Gramm.Per.Watt.Second.ConvertTo()));

			var displace = engineData.Displacement;
			Assert.AreEqual(0.01273, displace.Value());
			Assert.IsTrue(displace.HasEqualUnit(new SI().Cubic.Meter));

			var inert = engineData.Inertia;
			Assert.AreEqual(3.8, inert.Value(), 0.00001);
			Assert.IsTrue(inert.HasEqualUnit(new SI().Kilo.Gramm.Square.Meter));

			var idle = engineData.IdleSpeed;
			Assert.AreEqual(58.6430628670095, idle.Value(), 0.000001);
			Assert.IsTrue(idle.HasEqualUnit(0.SI<PerSecond>()));
		}


		private static void VehicleContainer(string engineFile, out VehicleContainer container, out CombustionEngine engine,
			out ITnOutPort requestPort)
		{
			container = new VehicleContainer();
			var gearbox = new MockGearbox(container);
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(engineFile);

			engine = new CombustionEngine(container, engineData);
			var clutch = new Clutch(container, engineData, engine.IdleController);

			var driver = new MockDriver(container);

			var aux = new Auxiliary(container);
			aux.AddConstant("", 5000.SI<Watt>());

			gearbox.Gear = 1;

			//gearbox.InPort().Connect(engine.OutPort());
			gearbox.InPort().Connect(clutch.OutPort());
			clutch.InPort().Connect(aux.OutPort());
			aux.InPort().Connect(engine.OutPort());

			// has to be done after connecting components!
			engine.IdleController.RequestPort = clutch.IdleControlPort;

			requestPort = gearbox.OutPort();

			//vehicleContainer.DataWriter = new ModalDataWriter("engine_idle_test.csv");
			var dataWriter = new MockModalDataWriter();
			container.DataWriter = dataWriter;
		}
	}
}