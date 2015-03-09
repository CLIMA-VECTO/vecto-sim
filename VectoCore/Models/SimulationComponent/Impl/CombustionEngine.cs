using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class CombustionEngine : VectoSimulationComponent, ICombustionEngine
	{
		public CombustionEngine(CombustionEngineData data)
		{

		}

		public override InPort InPort()
		{
			throw new NotImplementedException();
		}

		public override OutPort OutPort()
		{
			throw new NotImplementedException();
		}
	}
}
