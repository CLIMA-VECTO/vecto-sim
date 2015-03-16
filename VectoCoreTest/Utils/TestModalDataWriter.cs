using System.Data;
using TUGraz.VectoCore.Models.Simulation.Data;

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

        public object this[ModalResultField key]
        {
            get { return CurrentRow[key.GetName()]; }
            set { CurrentRow[key.GetName()] = value; }
        }
    }
}
