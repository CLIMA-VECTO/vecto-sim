using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	/// <summary>
	/// Fake Data Writer Class for Tests.
	/// </summary>
	internal class MockModalDataWriter : IModalDataWriter
	{
		public MockModalDataWriter()
		{
			Data = new ModalResults();
			CurrentRow = Data.NewRow();
			Auxiliaries = new Dictionary<string, DataColumn>();
		}

		public ModalResults Data { get; set; }
		public DataRow CurrentRow { get; set; }

		public object this[string auxId]
		{
			get { return CurrentRow[Auxiliaries[auxId]]; }
			set { CurrentRow[Auxiliaries[auxId]] = value; }
		}

		public bool HasTorqueConverter { get; set; }

		public void CommitSimulationStep()
		{
			Data.Rows.Add(CurrentRow);
			CurrentRow = Data.NewRow();
		}

		public void Finish() {}

		public IEnumerable<T> GetValues<T>(ModalResultField key)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x.Field<T>((int)key));
		}

		public IEnumerable<T> GetValues<T>(DataColumn col)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x.Field<T>(col));
		}

		public Dictionary<string, DataColumn> Auxiliaries { get; set; }

		public void AddAuxiliary(string id)
		{
			var auxColName = ModalResultField.Paux_ + id;
			if (!Data.Columns.Contains(auxColName)) {
				Auxiliaries[id] = Data.Columns.Add(auxColName, typeof(Watt));
			}
			Auxiliaries[id] = Data.Columns[auxColName];
		}

		public object this[ModalResultField key]
		{
			get { return CurrentRow[key.GetName()]; }
			set { CurrentRow[key.GetName()] = value; }
		}

		public void CommitSimulationStep(Second absTime, Second simulationInterval)
		{
			CurrentRow[ModalResultField.time.GetName()] = (absTime + simulationInterval / 2);
			CurrentRow[ModalResultField.simulationInterval.GetName()] = simulationInterval;
			CommitSimulationStep();
		}

		public T Field<T>(ModalResultField key)
		{
			return CurrentRow.Field<T>(key.GetName());
		}
	}
}