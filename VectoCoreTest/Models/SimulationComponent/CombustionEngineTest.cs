using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
			var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			var port = engine.OutShaft();
			Assert.IsNotNull(port);
		}

		[TestMethod]
		public void TestOutPortRequestNotFailing()
		{
			var vehicle = new VehicleContainer();
			var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			new EngineOnlyGearbox(vehicle);

			var port = engine.OutShaft();

			var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
			var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
			var torque = 400.SI<NewtonMeter>();
			var engineSpeed = 1500.0.RPMtoRad();

			port.Request(absTime, dt, torque, engineSpeed);
		}

		[TestMethod]
		public void TestSimpleModalData()
		{
			var vehicle = new VehicleContainer();
			var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);
			var gearbox = new EngineOnlyGearbox(vehicle);
			var port = engine.OutShaft();

			var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
			var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

			var torque = new NewtonMeter();
			var engineSpeed = 600.0.RPMtoRad();
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


			Assert.AreEqual(7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			engineSpeed = 644.4445.RPMtoRad();
			port.Request(absTime, dt, Formulas.PowerToTorque(1351.656.SI<Watt>(), engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);

			Assert.AreEqual(-7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			dataWriter.Data.WriteToFile(@"test1.csv");
		}

		[TestMethod]
		public void TestEngineMemento()
		{
			var vehicle = new VehicleContainer();
			var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
			var origin = new CombustionEngine(vehicle, engineData);

			var data = Memento.Serialize(origin);

			var restored = new CombustionEngine(vehicle, engineData);
			Memento.Deserialize(restored, data);

			Assert.AreEqual(origin, restored, "Serialized with Memento, Deserialized with Memento");

			data = origin.Serialize();
			restored = new CombustionEngine(vehicle, engineData);
			Memento.Deserialize(restored, data);

			Assert.AreEqual(origin, restored, "Serialized with Object, Deserialized with Memento");


			data = origin.Serialize();
			restored = new CombustionEngine(vehicle, engineData);
			restored.Deserialize(data);

			Assert.AreEqual(origin, restored, "Serialized with Object, Deserialized with Object");
		}

		[TestMethod]
		public void TestWriteToFile()
		{
			var vehicle = new VehicleContainer();
			var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
			var engine = new CombustionEngine(vehicle, engineData);

			engineData.WriteToFile("engineData test output.veng");
		}
	}
}