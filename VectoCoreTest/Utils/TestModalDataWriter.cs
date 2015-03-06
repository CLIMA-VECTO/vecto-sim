using System;
using System.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Tests.Utils
{
    /// <summary>
    /// Fake Data Writer Class for Tests.
    /// </summary>
    class TestDataWriter : IDataWriter
    {
        public DataTable Data;
        public DataRow CurrentRow;

        public TestDataWriter(DataTable data)
        {
            Data = data;
            CurrentRow = Data.NewRow();
        }

        public void CommitSimulationStep()
        {
            Data.Rows.Add(CurrentRow);
            CurrentRow = Data.NewRow();
        }

        public object this[Enum key]
        {
            get { return CurrentRow[key.ToString()]; }
            set { CurrentRow[key.ToString()] = value; }
        }
    }
}
