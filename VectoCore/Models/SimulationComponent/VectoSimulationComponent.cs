using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public abstract class VectoSimulationComponent
	{
		abstract public InPort InPort();

		abstract public OutPort OutPort();

	    abstract public void CommitSimulationStep(IDataWriter writer);

	}

}
