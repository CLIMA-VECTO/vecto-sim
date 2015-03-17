using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface IInPort
    {
	    void Connect(IOutPort other);
    }
}