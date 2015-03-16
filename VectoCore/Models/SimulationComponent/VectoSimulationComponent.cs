using Common.Logging;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public abstract class VectoSimulationComponent
	{
		protected ILog Log;

		protected VectoSimulationComponent()
		{
			Log = LogManager.GetLogger(this.GetType());
		}

	    abstract public void CommitSimulationStep(IModalDataWriter writer);

	}
}
