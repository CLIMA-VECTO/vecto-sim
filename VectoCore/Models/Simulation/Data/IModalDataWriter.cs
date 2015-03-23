using System;

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
        /// Commits the data of the current simulation step.
        /// </summary>
        void CommitSimulationStep(TimeSpan absTime, TimeSpan simulationInterval);

        void Finish();
    }
}
