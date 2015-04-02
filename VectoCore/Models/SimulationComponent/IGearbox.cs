using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Cockpit;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IGearbox : IInShaft, IOutShaft, IGearboxCockpit {}
}