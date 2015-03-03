using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace VectoCoreTest
{
	[TestClass]
	public class CombustionEngineTest
	{
		[TestMethod]
		public void TestEngineHasOutPort()
		{
			var engineData = new CombustionEngineData();
			var engine = new CombustionEngine(engineData);

			var port = engine.OutPort();
			Assert.IsNotNull(port);
		}

	    [TestMethod]
	    public void TestOutPortRequestNotFailing()
	    {
            var engineData = new CombustionEngineData();
            var engine = new CombustionEngine(engineData);

            var port = (ITnOutPort)engine.OutPort();

            TimeSpan absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            TimeSpan dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
	        var torque = 400;
	        var engineSpeed = 1500;

            port.Request(absTime, dt, torque, engineSpeed);
	    }

        [TestMethod]
        public void TestSimpleModalData()
        {
            var engineData = new CombustionEngineData();
            var engine = new CombustionEngine(engineData);
            var port = (ITnOutPort)engine.OutPort();

            TimeSpan absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            TimeSpan dt = new TimeSpan(seconds:1, minutes:0, hours:0);
            float torque = 400;
            float engineSpeed = 1500;
            port.Request(absTime, dt, torque, engineSpeed);


            TestDataWriter dataWriter = new TestDataWriter();
            engine.CommitSimulationStep(dataWriter);

            Assert.Equals(dataWriter[ModalResultFields.FC], 13000);
            Assert.Equals(dataWriter[ModalResultFields.FC_AUXc], 14000);
            Assert.Equals(dataWriter[ModalResultFields.FC_WHTCc], 15000);
        }
	}
}
