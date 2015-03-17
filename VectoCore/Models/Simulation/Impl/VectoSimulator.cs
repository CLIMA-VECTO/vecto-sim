using System.Collections.Generic;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class VectoSimulator : IVectoSimulator
    {
        private List<IVectoJob> _jobs = new List<IVectoJob>();
        private List<Task> _runningTasks = new List<Task>();

        public void AddJob(IVectoJob job)
        {
            _jobs.Add(job);
        }

        public void RunSimulation()
        {
            foreach (var job in _jobs)
            {
                var task = Task.Factory.StartNew(() => job.Run());
                _runningTasks.Add(task);
            }
        }
    }
}