using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Tests.Utils
{
	/// <summary>
	///     Fake Data Writer Class for Tests.
	/// </summary>
	internal class TestModalDataWriter : IModalDataWriter
	{
		public TestModalDataWriter()
		{
			Data = new ModalResults();
			CurrentRow = Data.NewRow();
		}

		public ModalResults Data { get; set; }
		public DataRow CurrentRow { get; set; }

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