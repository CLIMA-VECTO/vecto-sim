using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Simulation
{
	public interface IVehicleContainer : ICockpit
	{
		void AddComponent(VectoSimulationComponent component);
	}
}