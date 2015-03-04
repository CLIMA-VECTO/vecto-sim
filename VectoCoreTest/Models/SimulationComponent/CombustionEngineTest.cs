﻿using System;
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
        public void EngineHasOutPort()
        {
            var engineData = new CombustionEngineData();
            var engine = new CombustionEngine(engineData);

            var port = engine.OutPort();
            Assert.IsNotNull(port);
        }

        [TestMethod]
        public void OutPortRequestNotFailing()
        {
            var engineData = new CombustionEngineData();
            var engine = new CombustionEngine(engineData);

            var port = (ITnOutPort)engine.OutPort();

            TimeSpan absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            TimeSpan dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
            const int torque = 400;
            const int engineSpeed = 1500;

            port.Request(absTime, dt, torque, engineSpeed);
        }

        [TestMethod]
        public void SimpleModalData()
        {
            //todo: choose correct combustion engine data
            var engineData = new CombustionEngineData();
            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            TimeSpan absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            TimeSpan dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

            //todo: set correct input values to test
            const int torque = 400;
            const int engineSpeed = 1500;
            port.Request(absTime, dt, torque, engineSpeed);


            TestDataWriter dataWriter = new TestDataWriter();
            engine.CommitSimulationStep(dataWriter);

            //todo: test with correct output values, add other fields to test
            Assert.Equals(dataWriter[ModalResultFields.FC], 13000);
            Assert.Equals(dataWriter[ModalResultFields.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResultFields.FC_WHTCc], 15000);
        }

        [TestMethod]
        public void EngineOnlyDrivingCycle()
        {
            //todo: choose correct combustion engine data
            var engineData = new CombustionEngineData("24t Coach.veng");





            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            //todo: read engine only input file


            //todo: loop over all cycles

            TimeSpan absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            TimeSpan dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
            const int torque = 400;
            const int engineSpeed = 1500;
            port.Request(absTime, dt, torque, engineSpeed);


            TestDataWriter dataWriter = new TestDataWriter();
            engine.CommitSimulationStep(dataWriter);

            //todo: test with correct output values, add other fields to test
            Assert.Equals(dataWriter[ModalResultFields.FC], 13000);
            Assert.Equals(dataWriter[ModalResultFields.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResultFields.FC_WHTCc], 15000);
        }
    }
}
