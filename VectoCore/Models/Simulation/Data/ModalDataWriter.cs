using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public class ModalDataWriter : IModalDataWriter
	{
		private ModalResults Data { get; set; }
		private DataRow CurrentRow { get; set; }
		private string ModFileName { get; set; }


		public ModalDataWriter(string modFileName)
		{
			ModFileName = modFileName;
			Data = new ModalResults();
			CurrentRow = Data.NewRow();
		}

		public void CommitSimulationStep()
		{
			Data.Rows.Add(CurrentRow);
			CurrentRow = Data.NewRow();
		}

		public void Finish()
		{
			VectoCSVFile.Write(ModFileName, Data);
		}

		public object Compute(string expression, string filter)
		{
			return Data.Compute(expression, filter);
		}

		public IEnumerable<T> GetValues<T>(ModalResultField key)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x.Field<T>((int)key));
		}


		public object this[ModalResultField key]
		{
			get { return CurrentRow[(int)key]; }
			set { CurrentRow[(int)key] = value; }
		}
	}
}