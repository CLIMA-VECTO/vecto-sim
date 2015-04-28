using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	//todo: add job tracking (state of jobs, ...)
	//todo: add job control (pause, stop)


	/// <summary>
	///     Container for simulation jobs.
	/// </summary>
	public class JobContainer
	{
		private readonly List<IVectoSimulator> _simulators = new List<IVectoSimulator>();
		private readonly SummaryFileWriter _sumWriter;

		public JobContainer() {}

		public JobContainer(VectoJobData data)
		{
			_sumWriter = new SummaryFileWriter(Path.GetFileNameWithoutExtension(data.FileName) + ".vsum", data.FileName);
			_simulators.AddRange(SimulatorFactory.CreateJobs(data, _sumWriter));
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

			_sumWriter.Finish();
		}
	}
}