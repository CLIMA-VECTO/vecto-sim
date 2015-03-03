using System.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace VectoCoreTest
{
    class TestDataWriter : IDataWriter
    {
        private DataTable data;
        private DataRow currentRow;

        public TestDataWriter()
        {
            data = ModalResult.getDataTable();
            currentRow = data.NewRow();
        }

        public void CommitSimulationStep()
        {
            data.Rows.Add(currentRow);
            currentRow = data.NewRow();
        }

        public object this[ModalResultFields key]
        {
            get { return currentRow[key.ToString()]; }
            set { currentRow[key.ToString()] = value; }
        }
    }
}
