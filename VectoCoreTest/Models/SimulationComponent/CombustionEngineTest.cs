using System;
using System.IO;
using System.Reflection;
using System.Data;
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

            var gearbox = new EngineOnlyGearbox(vehicle);

            var port = engine.OutShaft();

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
            var torque = 400.0;
            var engineSpeed = 1500.0;

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

            var torque = 0.0;
            var engineSpeed = 600.0;
            var dataWriter = new TestModalDataWriter();

            for (var i = 0; i < 21; i++)
            {
                port.Request(absTime, dt, torque, engineSpeed);
                engine.CommitSimulationStep(dataWriter);
				if (i > 0)
					dataWriter.CommitSimulationStep(absTime, dt);
                absTime += dt;
            }

	        engineSpeed = 644.4445;
            port.Request(absTime, dt, VectoMath.ConvertPowerToTorque(2329.973, engineSpeed), engineSpeed);
            engine.CommitSimulationStep(dataWriter);

			Assert.AreEqual(1152.40304, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);

			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

	        torque = 4264.177;
	        for (var i = 0; i < 2; i++) {
		        port.Request(absTime, dt, torque, engineSpeed);
				engine.CommitSimulationStep(dataWriter);
				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
	        }

			engineSpeed = 869.7512;
			port.Request(absTime, dt, VectoMath.ConvertPowerToTorque(7984.56, engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);


			Assert.AreEqual(7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			engineSpeed = 644.4445;
			port.Request(absTime, dt, VectoMath.ConvertPowerToTorque(7984.56, engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);
			absTime += dt;

			Assert.AreEqual(-7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);

			dataWriter.Data.WriteToFile(@"test1.csv");
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\TestData\\EngineTests.csv", "EngineTests#csv", DataAccessMethod.Sequential)]
        [TestMethod]
        public void TestEngineOnlyDrivingCycle()
        {
            var vehicle = new VehicleContainer();
            var engineData = CombustionEngineData.ReadFromFile(TestContext.DataRow["EngineFile"].ToString());

            var gearbox = new EngineOnlyGearbox(vehicle);

            var data = DrivingCycleData.ReadFromFileEngineOnly(TestContext.DataRow["CycleFile"].ToString());
            var expectedResults = ModalResults.ReadFromFile(TestContext.DataRow["ModalResultFile"].ToString());

            var engine = new CombustionEngine(vehicle, engineData);
            var port = engine.OutShaft();


            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            var dataWriter = new TestModalDataWriter();

            foreach (var cycle in data.Entries)
            {
                port.Request(absTime, dt, cycle.EngineTorque, cycle.EngineSpeed);
                foreach (var sc in vehicle.SimulationComponents())
                {
                    sc.CommitSimulationStep(dataWriter);
                }
                absTime += dt;

                //todo: test with correct output values, add other fields to test
                Assert.AreEqual(13000, dataWriter[ModalResultField.FC]);
                Assert.AreEqual(14000, dataWriter[ModalResultField.FCAUXc]);
                Assert.AreEqual(15000, dataWriter[ModalResultField.FCWHTCc]);
            }

            //todo: test with correct output values, add other fields to test
            Assert.AreEqual(13000, dataWriter[ModalResultField.FC]);
            Assert.AreEqual(14000, dataWriter[ModalResultField.FCAUXc]);
            Assert.AreEqual(15000, dataWriter[ModalResultField.FCWHTCc]);
        }


        [TestMethod]
        public void TestEngineMemento()
        {
            var vehicle = new VehicleContainer();
            var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
            var origin = new CombustionEngine(vehicle, engineData);

            var data = Memento.Serialize(origin);
            var restored = Memento.Deserialize<CombustionEngine>(data);

            Assert.AreEqual(origin, restored, "Serialized with Memento, Deserialized with Memento");

            data = origin.Serialize();
            restored = Memento.Deserialize<CombustionEngine>(data);

            Assert.AreEqual(origin, restored, "Serialized with Object, Deserialized with Memento");


            data = origin.Serialize();
            restored = new CombustionEngine();
            restored.Deserialize(data);

            Assert.AreEqual(origin, restored, "Serialized with Object, Deserialized with Object");
        }
    }
}
