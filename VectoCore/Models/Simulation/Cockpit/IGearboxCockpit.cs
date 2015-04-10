namespace TUGraz.VectoCore.Models.Simulation.Cockpit
{
    /// <summary>
    /// Defines a method to access shared data of the gearbox.
    /// </summary>
    public interface IGearboxCockpit
    {
        /// <summary>
        /// Returns the current gear.
        /// </summary>
        /// <returns></returns>
        uint Gear();
    }
}