namespace TUGraz.VectoCore.Models.Simulation
{
    /// <summary>
    /// Defines the methods for the vecto simulator.
    /// </summary>
    public interface IVectoSimulator
    {
        /// <summary>
        /// Run the simulation.
        /// </summary>
        void Run();

        /// <summary>
        /// Return the vehicle container.
        /// </summary>
        /// <returns></returns>
        IVehicleContainer GetContainer();
    }
}