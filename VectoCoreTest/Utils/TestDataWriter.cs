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
        private readonly DataTable _data;
        private DataRow _currentRow;

        public TestDataWriter()
        {
            _data = ModalResult.getDataTable();
            _currentRow = _data.NewRow();
        }

        public void CommitSimulationStep()
        {
            _data.Rows.Add(_currentRow);
            _currentRow = _data.NewRow();
        }

        public object this[ModalResultFields key]
        {
            get { return _currentRow[key.ToString()]; }
            set { _currentRow[key.ToString()] = value; }
        }
    }
}
