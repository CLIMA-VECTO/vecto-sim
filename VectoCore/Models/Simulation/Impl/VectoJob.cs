namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public abstract class VectoJob : IVectoJob
    {
        protected VehicleContainer Container { get; set; }

        protected VectoJob(VehicleContainer container)
        {
            Container = container;
        }

        public abstract void Run();
        public IVehicleContainer GetContainer()
        {
            return Container;
        }
    }

    public abstract class DistanceBasedVectoJob : VectoJob
    {
        protected DistanceBasedVectoJob(VehicleContainer container)
            : base(container)
        {

        }

    }

    public abstract class TimeBasedVectoJob : VectoJob
    {
        protected TimeBasedVectoJob(VehicleContainer container)
            : base(container)
        {
        }
    }
}