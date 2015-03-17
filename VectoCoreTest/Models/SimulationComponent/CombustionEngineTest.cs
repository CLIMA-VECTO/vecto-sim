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
        private const string CoachEngine = "TestData\\EngineOnly\\EngineMaps\\24t Coach.veng";

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
            var port = engine.OutShaft();

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            //todo: set correct input values to test
            var torque = 0.0;
            var engineSpeed = 600.0;
            var dataWriter = new TestModalDataWriter();

            for (var i = 0; i < 10; i++)
            {
                port.Request(absTime, dt, torque, engineSpeed);
                engine.CommitSimulationStep(dataWriter);
                absTime += dt;
            }

            port.Request(absTime, dt, VectoMath.ConvertPowerToTorque(2329.973, 644.4445), 644.4445);
            engine.CommitSimulationStep(dataWriter);

            //todo: test with correct output values, add other fields to test
            //Assert.AreEqual(dataWriter[ModalResultField.FC], 13000);
            //Assert.AreEqual(dataWriter[ModalResultField.FCAUXc], 14000);
            //Assert.AreEqual(dataWriter[ModalResultField.FCWHTCc], 15000);
            Assert.AreEqual(2.906175, dataWriter[ModalResultField.PaEng]);
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\TestData\\EngineTests.csv", "EngineTests#csv", DataAccessMethod.Sequential)]
        [TestMethod]
        public void TestEngineOnlyDrivingCycle()
        {
			var vehicle = new VehicleContainer();
			var engineData = CombustionEngineData.ReadFromFile(TestContext.DataRow["EngineFile"].ToString());
			var data = EngineOnlyDrivingCycle.ReadFromFile(TestContext.DataRow["CycleFile"].ToString());
			var expectedResults = ModalResults.ReadFromFile(TestContext.DataRow["ModalResultFile"].ToString());

			var engine = new CombustionEngine(vehicle, engineData);
            var port = engine.OutShaft();


            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            var dataWriter = new TestModalDataWriter();

            foreach (var cycle in data)
            {
                port.Request(absTime, dt, cycle.Torque, cycle.EngineSpeed);
	            foreach (var sc in vehicle.SimulationComponents()) {
		            sc.CommitSimulationStep(dataWriter);
	            }
	            absTime += dt;

                //todo: test with correct output values, add other fields to test
                Assert.AreEqual(dataWriter[ModalResultField.FC], 13000);
                Assert.AreEqual(dataWriter[ModalResultField.FCAUXc], 14000);
                Assert.AreEqual(dataWriter[ModalResultField.FCWHTCc], 15000);
            }

            //todo: test with correct output values, add other fields to test
            Assert.AreEqual(dataWriter[ModalResultField.FC], 13000);
            Assert.AreEqual(dataWriter[ModalResultField.FCAUXc], 14000);
            Assert.AreEqual(dataWriter[ModalResultField.FCWHTCc], 15000);
        }


        [TestMethod]
        public void TestEngineMemento()
        {
            var vehicle = new VehicleContainer();
            var engineData = CombustionEngineData.ReadFromFile(CoachEngine);
            var origin = new CombustionEngine(vehicle, engineData);

            var data = Memento.Serialize(origin);

            var restored = Memento.Deserialize<CombustionEngine>(data);

            Assert.AreEqual(origin, restored);
        }


    }
}
