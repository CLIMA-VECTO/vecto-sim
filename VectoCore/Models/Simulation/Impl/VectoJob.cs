using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class VectoJob : IVectoJob
    {
        protected IDrivingCycle Cycle { get; set; }
        protected IModalDataWriter DataWriter { get; set; }
        protected IVehicleContainer Container { get; set; }

        public IVehicleContainer GetContainer()
        {
            return Container;
        }

        public VectoJob(IVehicleContainer container, IDrivingCycle cycle, IModalDataWriter dataWriter)
        {
            
            Container = container;
            Cycle = cycle;
            DataWriter = dataWriter;
        }

        public void Run()
        {
            LogManager.GetLogger(GetType()).Info("VectoJob started running.");
            while (Cycle.DoSimulationStep())
            {
                Container.CommitSimulationStep(DataWriter);
            }
            Container.FinishSimulation(DataWriter);
            LogManager.GetLogger(GetType()).Info("VectoJob finished.");
        }
    }
}