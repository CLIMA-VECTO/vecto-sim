using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader.Impl;
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
		private const string CoachEngine = @"TestData\Components\24t Coach.veng";
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

			Assert.AreEqual(1152.40304, ((SI)dataWriter[ModalResultField.PaEng]).Value(), 0.001);

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
			var engine = new CombustionEngine(vehicleContainer, engineData);

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
	}
}