using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Simulation
{
	public interface IVehicleContainer : ICockpit
	{
		void AddComponent<T>(T component) where T : VectoSimulationComponent, ICockpitComponent;

		//void AddComponent<T>(T component) where T : VectoSimulationComponent, IEngineCockpit;

		//void AddComponent<T>(T component) where T : VectoSimulationComponent, IGearboxCockpit;

	}
}