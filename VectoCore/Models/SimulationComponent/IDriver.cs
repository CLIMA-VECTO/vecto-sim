using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Defines interfaces for a driver.
	/// </summary>
	public interface IDriver : IDriverDemandInProvider, IDriverDemandOutProvider {}
}