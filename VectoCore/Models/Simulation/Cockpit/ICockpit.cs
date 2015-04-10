namespace TUGraz.VectoCore.Models.Simulation.Cockpit
{
    /// <summary>
    /// Defines interfaces for all different cockpits to access shared data of the powertrain.
    /// </summary>
    public interface ICockpit : IGearboxCockpit, IEngineCockpit, IVehicleCockpit {}
}