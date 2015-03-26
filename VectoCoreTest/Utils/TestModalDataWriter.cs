using System;
using System.Data;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
    /// <summary>
    /// Fake Data Writer Class for Tests.
    /// </summary>
    class TestModalDataWriter : IModalDataWriter
    {
        public ModalResults Data { get; set; }
        public DataRow CurrentRow { get; set; }

        public TestModalDataWriter()
        {
            Data = new ModalResults();
            CurrentRow = Data.NewRow();
        }

	    public void CommitSimulationStep()
	    {
			Data.Rows.Add(CurrentRow);
			CurrentRow = Data.NewRow();		    
	    }

		public void CommitSimulationStep(TimeSpan absTime, TimeSpan simulationInterval)
		{
			CurrentRow[ModalResultField.time.GetName()] = (absTime - TimeSpan.FromTicks(simulationInterval.Ticks / 2)).TotalSeconds;
			CurrentRow[ModalResultField.simulationInterval.GetName()] = simulationInterval.TotalSeconds;
			CommitSimulationStep();
        }

        public void Finish()
        {
            
        }

        public object this[ModalResultField key]
        {
            get { return CurrentRow[key.GetName()]; }
            set { CurrentRow[key.GetName()] = value; }
        }

	    public double GetDouble(ModalResultField key)
	    {
			return CurrentRow.Field<double>(key.GetName());
	    }

    }
}
