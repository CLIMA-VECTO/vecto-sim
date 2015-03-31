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
            port.Request(absTime, dt, VectoMath.ConvertPowerRpmToTorque(2329.973, engineSpeed), engineSpeed);
            engine.CommitSimulationStep(dataWriter);

			Assert.AreEqual(1152.40304, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);

			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

	        var power = new double[] {569.3641, 4264.177};
;	        for (var i = 0; i < 2; i++) {
				port.Request(absTime, dt, VectoMath.ConvertPowerRpmToTorque(power[i], engineSpeed), engineSpeed);
				engine.CommitSimulationStep(dataWriter);
				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
	        }

			engineSpeed = 869.7512;
			port.Request(absTime, dt, VectoMath.ConvertPowerRpmToTorque(7984.56, engineSpeed), engineSpeed);
			engine.CommitSimulationStep(dataWriter);


			Assert.AreEqual(7108.32, dataWriter.GetDouble(ModalResultField.PaEng), 0.001);
			dataWriter.CommitSimulationStep(absTime, dt);
			absTime += dt;

			engineSpeed = 644.4445;
			port.Request(absTime, dt, VectoMath.ConvertPowerRpmToTorque(1351.656, engineSpeed), engineSpeed);
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
			Memento.Deserialize<CombustionEngine>(restored, data);

            Assert.AreEqual(origin, restored, "Serialized with Memento, Deserialized with Memento");

            data = origin.Serialize();
			restored = new CombustionEngine(vehicle, engineData);
			Memento.Deserialize<CombustionEngine>(restored, data);

            Assert.AreEqual(origin, restored, "Serialized with Object, Deserialized with Memento");


            data = origin.Serialize();
            restored = new CombustionEngine(vehicle, engineData);
            restored.Deserialize(data);

            Assert.AreEqual(origin, restored, "Serialized with Object, Deserialized with Object");
        }
    }
}
