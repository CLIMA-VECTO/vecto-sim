using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.Simulation
{
    public interface IDrivingCycle: IInShaft
    {
        bool DoSimulationStep();
    }
}