using System.Collections.Generic;

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

		bool HasTorqueConverter { get; set; }

		/// <summary>
		///     Commits the data of the current simulation step.
		/// </summary>
		void CommitSimulationStep();

		void Finish();

		object Compute(string expression, string filter);

		IEnumerable<T> GetValues<T>(ModalResultField key);
	}
}