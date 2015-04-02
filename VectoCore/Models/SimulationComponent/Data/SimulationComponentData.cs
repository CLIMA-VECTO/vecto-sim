using System;
using Common.Logging;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class SimulationComponentData
	{
		[NonSerialized] protected ILog Log;

		public SimulationComponentData()
		{
			Log = LogManager.GetLogger(GetType());
		}
	}
}