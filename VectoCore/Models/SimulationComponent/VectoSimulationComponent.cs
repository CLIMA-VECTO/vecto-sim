using Common.Logging;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public abstract class VectoSimulationComponent : ICockpitComponent
	{
		protected ICockpit Cockpit;

		protected ILog Log;

		protected VectoSimulationComponent(IVehicleContainer cockpit)
		{
			Cockpit = cockpit;
			Log = LogManager.GetLogger(this.GetType());

			cockpit.AddComponent(this);
		}

	    abstract public void CommitSimulationStep(IModalDataWriter writer);

	}
}
