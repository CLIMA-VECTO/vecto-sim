using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

namespace TUGraz.VectoCore.Models.SimulationComponent
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
