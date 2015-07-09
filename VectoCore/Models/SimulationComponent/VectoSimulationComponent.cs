using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Base class for all vecto simulation components.
	/// </summary>
	public abstract class VectoSimulationComponent
	{
		[NonSerialized] protected ICockpit Cockpit;
		[NonSerialized] protected ILog Log;

		/// <summary>
		/// Constructor. Registers the component in the cockpit.
		/// </summary>
		/// <param name="cockpit">The vehicle container</param>
		protected VectoSimulationComponent(IVehicleContainer cockpit)
		{
			Cockpit = cockpit;
			Log = LogManager.GetLogger(GetType());

			cockpit.AddComponent(this);
		}

		/// <summary>
		/// Commits the simulation step.
		/// Writes the moddata into the data writer.
		/// Commits the internal state of the object if needed.
		/// </summary>
		/// <param name="writer">a data writer to write the data into.</param>
		public abstract void CommitSimulationStep(IModalDataWriter writer);
	}
}