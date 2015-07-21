using System.Collections.Generic;
using System.Data;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public interface IModalDataWriter
	{
		/// <summary>
		/// Indexer for fields of the DataWriter. Accesses the data of the current step.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[ModalResultField key] { get; set; }

		/// <summary>
		/// Indexer for auxiliary fields of the DataWriter.
		/// </summary>
		/// <param name="auxId"></param>
		/// <returns></returns>
		object this[string auxId] { get; set; }

		bool HasTorqueConverter { get; set; }

		/// <summary>
		/// Commits the data of the current simulation step.
		/// </summary>
		void CommitSimulationStep();

		/// <summary>
		/// Finishes the writing of the DataWriter.
		/// </summary>
		void Finish();

		object Compute(string expression, string filter);

		IEnumerable<T> GetValues<T>(ModalResultField key);

		Dictionary<string, DataColumn> Auxiliaries { get; set; }

		void AddAuxiliary(string id);
	}
}