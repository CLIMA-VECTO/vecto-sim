using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
    /// <summary>
    /// Defines interfaces for a engine only driving cycle.
    /// </summary>
    public interface IEngineOnlyDrivingCycle : IDrivingCycleOutProvider, IInShaft {}
}