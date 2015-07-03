using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Cockpit;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IVehicle : IRoadPortInProvider, IDriverDemandOutProvider, IVehicleCockpit {}
}

/// </summary>
//	public interface IVehicle : IDriverDemandOutProvider, IRoadPortInProvider {}
//}