using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Simulation
{
	/// <summary>
	/// Defines Methods for adding components, commiting a simulation step and finishing the simulation.
	/// Also defines interfaces for all cockpit access to data.
	/// </summary>
	public interface IVehicleContainer : ICockpit
	{
		ISimulationOutPort GetCycleOutPort();

		/// <summary>
		/// Adds a component to the vehicle container.
		/// </summary>
		/// <param name="component"></param>
		void AddComponent(VectoSimulationComponent component);

		/// <summary>
		/// Commits the current simulation step.
		/// </summary>
		void CommitSimulationStep(double time, double simulationInterval);

		/// <summary>
		/// Finishes the simulation.
		/// </summary>
		void FinishSimulation();
	}
}