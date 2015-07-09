using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		[TestMethod]
		public void TestEngineHasOutPort()
		{
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			var port = engine.OutShaft();
			Assert.IsNotNull(port);
		}

		[TestMethod]
		public void TestOutPortRequestNotFailing()
		{
			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			new EngineOnlyGearbox(vehicle);

			var port = engine.OutShaft();

			var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
			var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
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
			new EngineOnlyGearbox(vehicle);
			var port = engine.OutShaft();

			var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
			var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

			var torque = 0.SI<NewtonMeter>();
			var engineSpeed = 600.RPMtoRad();
			var dataWriter = new TestModalDataWriter();

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

			Assert.AreEqual(1152.40304, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);

			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			var power = new[] { 569.3641, 4264.177 };
			for (var i = 0; i < 2; i++) {
				port.Request(absTime, dt, Formulas.PowerToTorque(power[i].SI<Watt>(), engineSpeed), engineSpeed);
				engine.CommitSimulationStep(dataWriter);
				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
			}

			engineSpeed = 869.7512.RPMtoRad();
			port.Request(absTime, dt, Formulas.PowerToTorque(7984.56.SI<Watt>(), engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);


			Assert.AreEqual(7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			engineSpeed = 644.4445.RPMtoRad();
			port.Request(absTime, dt, Formulas.PowerToTorque(1351.656.SI<Watt>(), engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);

			Assert.AreEqual(-7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);

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

			gearbox.InShaft().Connect(engine.OutShaft());

			var expectedResults = VectoCSVFile.Read(TestContext.DataRow["ResultFile"].ToString());

			var requestPort = gearbox.OutShaft();

			//var modalData = new ModalDataWriter(string.Format("load_jump_{0}.csv", TestContext.DataRow["TestName"].ToString()));
			var modalData = new TestModalDataWriter();

			var idlePower = double.Parse(TestContext.DataRow["initialIdleLoad"].ToString()).SI<Watt>();

			var angularSpeed = double.Parse(TestContext.DataRow["rpm"].ToString()).RPMtoRad();

			var t = TimeSpan.FromSeconds(0);
			var dt = TimeSpan.FromSeconds(0.1);

			for (; t.TotalSeconds < 2; t += dt) {
				requestPort.Request(t, dt, Formulas.PowerToTorque(idlePower, angularSpeed), angularSpeed);
				engine.CommitSimulationStep(modalData);
			}

			var i = 0;
			// dt = TimeSpan.FromSeconds(double.Parse(TestContext.DataRow["dt"].ToString(), CultureInfo.InvariantCulture));
			// dt = TimeSpan.FromSeconds(expectedResults.Rows[i].ParseDouble(0)) - t;
			var engineLoadPower = engineData.GetFullLoadCurve(0).FullLoadStationaryPower(angularSpeed);
			idlePower = double.Parse(TestContext.DataRow["finalIdleLoad"].ToString()).SI<Watt>();
			for (; t.TotalSeconds < 25; t += dt, i++) {
				dt = TimeSpan.FromSeconds(expectedResults.Rows[i + 1].ParseDouble(0) - expectedResults.Rows[i].ParseDouble(0));
				if (t >= TimeSpan.FromSeconds(10)) {
					engineLoadPower = idlePower;
				}
				requestPort.Request(t, dt, Formulas.PowerToTorque(engineLoadPower, angularSpeed), angularSpeed);
				modalData[ModalResultField.time] = t.TotalSeconds;
				modalData[ModalResultField.simulationInterval] = dt.TotalSeconds;
				engine.CommitSimulationStep(modalData);
				// todo: compare results...
				Assert.AreEqual(expectedResults.Rows[i].ParseDouble(0), t.TotalSeconds, 0.001, "Time");
				Assert.AreEqual(expectedResults.Rows[i].ParseDouble(1), modalData.GetDouble(ModalResultField.Pe_full), 0.1,
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
			Assert.AreEqual(motorway.Double(), 0);
			Assert.IsTrue(motorway.HasEqualUnit(new SI().Kilo.Gramm.Per.Watt.Second.ConvertTo()));

			var rural = engineData.WHTCRural;
			Assert.AreEqual(rural.Double(), 0);
			Assert.IsTrue(rural.HasEqualUnit(new SI().Kilo.Gramm.Per.Watt.Second.ConvertTo()));

			var urban = engineData.WHTCUrban;
			Assert.AreEqual(urban.Double(), 0);
			Assert.IsTrue(urban.HasEqualUnit(new SI().Kilo.Gramm.Per.Watt.Second.ConvertTo()));

			var displace = engineData.Displacement;
			Assert.AreEqual(0.01273, displace.Double());
			Assert.IsTrue(displace.HasEqualUnit(new SI().Cubic.Meter));

			var inert = engineData.Inertia;
			Assert.AreEqual(3.8, inert.Double(), 0.00001);
			Assert.IsTrue(inert.HasEqualUnit(new SI().Kilo.Gramm.Square.Meter));

			var idle = engineData.IdleSpeed;
			Assert.AreEqual(58.6430628670095, idle.Double(), 0.000001);
			Assert.IsTrue(idle.HasEqualUnit(0.SI<PerSecond>()));

			engineData.GetFullLoadCurve(0);
			AssertHelper.Exception<KeyNotFoundException>(() => engineData.GetFullLoadCurve(1000));
		}
	}
}