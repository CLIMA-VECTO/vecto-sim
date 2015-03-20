using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
    public interface IDrivingCycle : IDriverDemandInProvider
    {
        bool DoSimulationStep();
    }

    public interface IEngineOnlyDrivingCycle: IInShaft
    {
        bool DoSimulationStep();
    }
}