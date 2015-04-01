using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
    public class ModalDataWriter : IModalDataWriter
    {
        private ModalResults Data { get; set; }

        private DataRow CurrentRow { get; set; }

        public ModalDataWriter(string fileName)
        {
            FileName = fileName;
            Data = new ModalResults();
            CurrentRow = Data.NewRow();
        }

        public string FileName { get; set; }

        public void CommitSimulationStep()
        {
            Data.Rows.Add(CurrentRow);
            CurrentRow = Data.NewRow();
        }

        public void Finish()
        {
            VectoCSVFile.Write(FileName, Data);
        }

        public object this[ModalResultField key]
        {
            get { return CurrentRow[(int)key]; }
            set { CurrentRow[(int)key] = value; }
        }
    }
}