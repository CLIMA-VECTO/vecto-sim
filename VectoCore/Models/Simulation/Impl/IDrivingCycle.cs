using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public interface IDrivingCycle: IInShaft
    {
        bool DoSimulationStep();
    }
}