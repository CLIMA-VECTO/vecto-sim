using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public abstract class VectoSimulationComponent
	{
		[NonSerialized] protected ICockpit Cockpit;
		[NonSerialized] protected ILog Log;

		protected VectoSimulationComponent(IVehicleContainer cockpit)
		{
			Cockpit = cockpit;
			Log = LogManager.GetLogger(GetType());

			cockpit.AddComponent(this);
		}

		public abstract void CommitSimulationStep(IModalDataWriter writer);
	}
}