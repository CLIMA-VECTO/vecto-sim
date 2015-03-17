namespace TUGraz.VectoCore.Models.Simulation
{
    public interface IVectoJob
    {
        void Run();

        IVehicleContainer GetContainer();
    }
}