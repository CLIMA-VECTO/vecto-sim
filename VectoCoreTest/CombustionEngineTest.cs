using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace VectoCoreTest
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

            var port = engine.OutPort();

	        var torque = 400;
	        var engineSpeed = 1500;

	        port.request(torque, engineSpeed);
	    }





	    interface IModalDataVisitor
	    {
	        object this[ResultFields key] { get; set; }
	    }

        enum ResultFields
        {
            SimulationStep,
            time,
            td,
            FC,
            FCAUXc,
            FCWHTCc
        }


        class TestModalDataVisitor : IModalDataVisitor
	    {


	        private DataTable data;
	        private DataRow currentRow;

	        public TestModalDataVisitor()
	        {
                data = new DataTable();
                data.Columns.Add(ResultFields.FC.ToString(), typeof(float));
                data.Columns.Add(ResultFields.FCAUXc.ToString(), typeof(float));
                data.Columns.Add(ResultFields.FCWHTCc.ToString(), typeof(float));
	            currentRow = data.NewRow();
	        }

            public object[] commitSimulationStep()
            {
                data.Rows.Add(currentRow);
                var itemArray = currentRow.ItemArray;
                currentRow = data.NewRow();
                return itemArray;
            }


            public object this[ResultFields key]
            {
                get { return currentRow[key.ToString()]; }
                set { currentRow[key.ToString()] = value; }
	        }

	    }




        [TestMethod]
        public void TestSimpleModalData()
        {
            var engineData = new CombustionEngineData();
            var engine = new CombustionEngine(engineData);
            var port = engine.OutPort();

            TimeSpan absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
            TimeSpan dt = new TimeSpan(seconds:1, minutes:0, hours:0);
            float torque = 400;
            float engineSpeed = 1500;
            // todo: add request(absTime:TimeSpan, dt:TimeSpan, torque:float, engineSpeed:float) in TnOutPort and ITnOutPort
            port.request(absTime, dt, torque, engineSpeed);


            TestModalDataVisitor dataVisitor = TestModalDataVisitor();
            // todo: add accept(IModalDataVisitor) to VehicleSimulationComponent
            engine.accept(dataVisitor);
            
            var itemArray = dataVisitor.commitSimulationStep();
            int[] testArray = {13000, 15000, 15500};
            Assert.Equals(itemArray, testArray);
        }
	}
}
