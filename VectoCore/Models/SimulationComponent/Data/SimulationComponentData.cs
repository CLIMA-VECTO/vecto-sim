using Common.Logging;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class SimulationComponentData
	{
		protected ILog Log;

		public SimulationComponentData()
		{
			Log = LogManager.GetLogger(this.GetType());
		}

	}
}
