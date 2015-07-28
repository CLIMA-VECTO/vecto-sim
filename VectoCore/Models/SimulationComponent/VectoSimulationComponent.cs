using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Base class for all vecto simulation components.
	/// </summary>
	public abstract class VectoSimulationComponent
	{
		[NonSerialized] protected IDataBus DataBus;
		[NonSerialized] protected ILog Log;

		/// <summary>
		/// Constructor. Registers the component in the cockpit.
		/// </summary>
		/// <param name="dataBus">The vehicle container</param>
		protected VectoSimulationComponent(IVehicleContainer dataBus)
		{
			DataBus = dataBus;
			Log = LogManager.GetLogger(GetType());

			dataBus.AddComponent(this);
		}

		public void CommitSimulationStep(IModalDataWriter writer)
		{
			if (writer != null) {
				DoWriteModalResults(writer);
			}
			DoCommitSimulationStep();
		}

		protected abstract void DoWriteModalResults(IModalDataWriter writer);

		/// <summary>
		/// Commits the simulation step.
		/// Writes the moddata into the data writer.
		/// Commits the internal state of the object if needed.
		/// </summary>
		protected abstract void DoCommitSimulationStep();
	}
}