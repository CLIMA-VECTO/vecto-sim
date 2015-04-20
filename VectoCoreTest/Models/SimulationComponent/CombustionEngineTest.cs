using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
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

			var torque = 0.SI<NewtonMeter>();
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

		[TestMethod]
		public void Test_EngineData()
		{
			var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
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

			var flc0 = engineData.GetFullLoadCurve(0);

			AssertException<KeyNotFoundException>(() => { var flc10000 = engineData.GetFullLoadCurve(1000); });
		}
	}
}