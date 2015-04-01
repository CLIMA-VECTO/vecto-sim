using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    //todo: add job tracking (state of jobs, iteration, ...)
    //todo: add job control (pause, stop)
    public class JobContainer
    {
        private List<IVectoSimulator> _simulators = new List<IVectoSimulator>();

        public void AddJob(IVectoSimulator sim)
        {
            _simulators.Add(sim);
        }

        public void RunSimulation()
        {
            LogManager.GetLogger(GetType()).Info("VectoSimulator started running. Starting Jobs.");
            Task.WaitAll(_simulators.Select(job => Task.Factory.StartNew(job.Run)).ToArray());
        }
    }
}