using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
    [TestClass]
    public class CombustionEngineTest
    {
        [TestMethod]
        public void TestEngineHasOutPort()
        {
            var engineData = CombustionEngineData.ReadFromFile("24t Coach.veng");
            var engine = new CombustionEngine(engineData);

            var port = engine.OutPort();
            Assert.IsNotNull(port);
        }

        [TestMethod]
        public void TestOutPortRequestNotFailing()
        {
            var engineData = CombustionEngineData.ReadFromFile("24t Coach.veng");
            var engine = new CombustionEngine(engineData);

            var port = (ITnOutPort)engine.OutPort();

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
            var torque = 400.0;
            var engineSpeed = 1500.0;

            port.Request(absTime, dt, torque, engineSpeed);
        }

        [TestMethod]
        public void TestSimpleModalData()
        {
            var engineData = CombustionEngineData.ReadFromFile("24t Coach.veng");
            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            //todo: set correct input values to test
            var torque = 400.0;
            var engineSpeed = 1500.0;
            port.Request(absTime, dt, torque, engineSpeed);


            var dataWriter = new TestDataWriter(new ModalResults());
            engine.CommitSimulationStep(dataWriter);

            //todo: test with correct output values, add other fields to test
            Assert.Equals(dataWriter[ModalResult.FC], 13000);
            Assert.Equals(dataWriter[ModalResult.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResult.FC_WHTCc], 15000);
        }

        [TestMethod]
        public void TestEngineOnlyDrivingCycle()
        {
            var engineData = CombustionEngineData.ReadFromFile("24t Coach.veng");
            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            var data = EngineOnlyDrivingCycle.Read("Coach Engine Only.vdri");

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            var dataWriter = new TestDataWriter(new ModalResults());

            foreach (var cycle in data)
            {
                port.Request(absTime, dt, cycle.T, cycle.n);
                engine.CommitSimulationStep(dataWriter);
                absTime += dt;

                //todo: test with correct output values, add other fields to test
                Assert.Equals(dataWriter[ModalResult.FC], 13000);
                Assert.Equals(dataWriter[ModalResult.FC_AUXc], 14000);
                Assert.Equals(dataWriter[ModalResult.FC_WHTCc], 15000);
            }

            //todo: test with correct output values, add other fields to test
            Assert.Equals(dataWriter[ModalResult.FC], 13000);
            Assert.Equals(dataWriter[ModalResult.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResult.FC_WHTCc], 15000);
        }
    }
}
