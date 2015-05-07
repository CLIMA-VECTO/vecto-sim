using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
		private readonly ISummaryDataWriter _sumWriter;

		private static int _jobNumber;

		public JobContainer(ISummaryDataWriter sumWriter)
		{
			_sumWriter = sumWriter;
		}

		public JobContainer(VectoJobData data)
		{
			var sumFileName = Path.GetFileNameWithoutExtension(data.JobFileName);
			var sumFilePath = Path.GetDirectoryName(data.JobFileName);

			_sumWriter = new SummaryFileWriter(string.Format("{0}.vsum", Path.Combine(sumFilePath, sumFileName)));
			AddJobs(data);
		}

		public void AddJobs(VectoJobData data)
		{
			_jobNumber++;
			_simulators.AddRange(SimulatorFactory.CreateJobs(data, _sumWriter, _jobNumber));
		}

		public void AddJob(IVectoSimulator sim)
		{
			_jobNumber++;
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