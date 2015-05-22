using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
    /// <summary>
    /// Defines interfaces for a  driver demand driving cycle.
    /// </summary>
    public interface IDrivingCycleDemandDrivingCycle : IDrivingCycleOutProvider, IDrivingCycleDemandInProvider {}
}