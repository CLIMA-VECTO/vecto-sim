using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TUGraz.VectoCore.Models.Connector;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public abstract class VectoSimulationComponent
	{
		abstract public InPort InPort();
		abstract public OutPort OutPort();

	}

}
