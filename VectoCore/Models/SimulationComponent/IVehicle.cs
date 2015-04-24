using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Defines interfaces for a vehicle.
	/// </summary>
	public interface IVehicle : IDriverDemandOutProvider, IRoadPortInProvider {}
}