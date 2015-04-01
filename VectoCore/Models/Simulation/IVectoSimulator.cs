using TUGraz.VectoCore.Models.Simulation.Impl;

namespace TUGraz.VectoCore.Models.Simulation
{
    public interface IVectoSimulator
    {
        void Run();

        IVehicleContainer GetContainer();
    }
}