using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Models.Simulation.Data;

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

		public object Compute(string expression, string filter)
		{
			return Data.Compute(expression, filter);
		}

		public IEnumerable<T> GetValues<T>(ModalResultField key)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x.Field<T>((int)key));
		}

		public Dictionary<string, DataColumn> Auxiliaries { get; set; }

		public void AddAuxiliary(string id)
		{
			Auxiliaries[id] = Data.Columns.Add(ModalResultField.Paux_ + id, typeof(double));
		}

		public object this[ModalResultField key]
		{
			get { return CurrentRow[key.GetName()]; }
			set { CurrentRow[key.GetName()] = value; }
		}

		public void CommitSimulationStep(TimeSpan absTime, TimeSpan simulationInterval)
		{
			CurrentRow[ModalResultField.time.GetName()] =
				(absTime + TimeSpan.FromTicks(simulationInterval.Ticks / 2)).TotalSeconds;
			CurrentRow[ModalResultField.simulationInterval.GetName()] = simulationInterval.TotalSeconds;
			CommitSimulationStep();
		}

		public double GetDouble(ModalResultField key)
		{
			return CurrentRow.Field<double>(key.GetName());
		}
	}
}