using System;
using System.Collections.Generic;
using System.Data;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public interface IModalDataWriter
	{
		/// <summary>
		///     Indexer for fields of the DataWriter. Accesses the data of the current step.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[ModalResultField key] { get; set; }

		/// <summary>
		///     Commits the data of the current simulation step.
		/// </summary>
		void CommitSimulationStep();

		void Finish();

		Object Compute(string expression, string filter);

		IEnumerable<object> GetValues(ModalResultField key);
	}
}