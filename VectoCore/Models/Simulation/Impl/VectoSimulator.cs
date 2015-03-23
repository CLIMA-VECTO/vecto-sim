using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    //todo: add job tracking (state of jobs, iteration, ...)
    //todo: add job control (pause, stop)
    public class VectoSimulator : IVectoSimulator
    {
        private List<IVectoJob> _jobs = new List<IVectoJob>();

        public void AddJob(IVectoJob job)
        {
            _jobs.Add(job);
        }

        public void RunSimulation()
        {
            LogManager.GetLogger(GetType()).Info("VectoSimulator started running. Starting Jobs.");
            Task.WhenAll(_jobs.Select(job => Task.Factory.StartNew(job.Run)));
        }
    }
}