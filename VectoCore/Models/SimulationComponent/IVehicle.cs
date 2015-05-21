using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IVehicle : IRoadPortInProvider, IDriverDemandOutProvider, IFvInPort, IDriverDemandOutPort {}
}
	/// </summary>
	public interface IVehicle : IDriverDemandOutProvider, IRoadPortInProvider {}
}