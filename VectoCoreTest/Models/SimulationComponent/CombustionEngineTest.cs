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
            var engineData = new CombustionEngineData("24t Coach.veng");
            var engine = new CombustionEngine(engineData);

            var port = engine.OutPort();
            Assert.IsNotNull(port);
        }

        [TestMethod]
        public void TestOutPortRequestNotFailing()
        {
            var engineData = new CombustionEngineData("24t Coach.veng");
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
            var engineData = new CombustionEngineData("24t Coach.veng");
            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            //todo: set correct input values to test
            var torque = 400.0;
            var engineSpeed = 1500.0;
            port.Request(absTime, dt, torque, engineSpeed);


            var dataWriter = new TestDataWriter();
            engine.CommitSimulationStep(dataWriter);

            //todo: test with correct output values, add other fields to test
            Assert.Equals(dataWriter[ModalResultFields.FC], 13000);
            Assert.Equals(dataWriter[ModalResultFields.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResultFields.FC_WHTCc], 15000);
        }

        [TestMethod]
        public void TestEngineOnlyDrivingCycle()
        {
            var engineData = new CombustionEngineData("24t Coach.veng");

            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            var data = EngineOnlyDrivingCycle.read("Coach Engine Only.vdri");

            var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            var dataWriter = new TestDataWriter();

            foreach (DataRow row in data.Rows)
            {
                var torque = (double)row[EngineOnlyDrivingCycleFields.Pe.ToString()];
                var engineSpeed = (double)row[EngineOnlyDrivingCycleFields.n.ToString()];
                port.Request(absTime, dt, torque, engineSpeed);

                engine.CommitSimulationStep(dataWriter);

                absTime += dt;
            }

            //todo: test with correct output values, add other fields to test
            Assert.Equals(dataWriter[ModalResultFields.FC], 13000);
            Assert.Equals(dataWriter[ModalResultFields.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResultFields.FC_WHTCc], 15000);
        }
    }
}
