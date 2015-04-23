using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	//todo: add job tracking (state of jobs, iteration, ...)
	//todo: add job control (pause, stop)


	/// <summary>
	///     Container for simulation jobs.
	/// </summary>
	public class JobContainer
	{
		private readonly List<IVectoSimulator> _simulators = new List<IVectoSimulator>();

		public JobContainer() {}

		public JobContainer(VectoJobData data)
		{
			_simulators.AddRange(SimulatorFactory.CreateJobs(data));
		}


		public void AddJob(IVectoSimulator sim)
		{
			_simulators.Add(sim);
		}

		/// <summary>
		///     Runs all jobs, waits until finished.
		/// </summary>
		public void RunJobs()
		{
			LogManager.GetLogger(GetType()).Info("VectoSimulator started running. Starting Jobs.");
			Task.WaitAll(_simulators.Select(job => Task.Factory.StartNew(job.Run)).ToArray());
		}
	}
}