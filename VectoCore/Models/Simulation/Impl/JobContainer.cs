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
	/// Container for simulation jobs.
	/// </summary>
	public class JobContainer
	{
		private readonly List<IVectoSimulator> _simulators = new List<IVectoSimulator>();
		private readonly SummaryFileWriter _sumWriter;

		private static int _jobNumber;

		/// <summary>
		/// Initializes a new empty instance of the <see cref="JobContainer"/> class.
		/// </summary>
		/// <param name="sumWriter">The sum writer.</param>
		public JobContainer(SummaryFileWriter sumWriter)
		{
			_sumWriter = sumWriter;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JobContainer"/> class from a VectoJobData object.
		/// </summary>
		/// <param name="data">The data.</param>
		public JobContainer(VectoJobData data)
		{
			var sumFileName = Path.GetFileNameWithoutExtension(data.JobFileName);
			var sumFilePath = Path.GetDirectoryName(data.JobFileName);

			_sumWriter = new SummaryFileWriter(string.Format("{0}.vsum", Path.Combine(sumFilePath, sumFileName)));
			AddJobs(data);
		}

		/// <summary>
		/// Creates and Adds jobs from the VectoJobData object.
		/// </summary>
		/// <param name="data">The data.</param>
		public void AddJobs(VectoJobData data)
		{
			_jobNumber++;
			_simulators.AddRange(SimulatorFactory.CreateJobs(data, _sumWriter, _jobNumber));
		}

		/// <summary>
		/// Adds a custom created job.
		/// </summary>
		/// <param name="sim">The sim.</param>
		public void AddJob(IVectoSimulator sim)
		{
			_jobNumber++;
			_simulators.Add(sim);
		}

		/// <summary>
		/// Runs all jobs, waits until finished.
		/// </summary>
		public void RunJobs()
		{
			LogManager.GetLogger(GetType()).Info("VectoSimulator started running. Starting Jobs.");

			Task.WaitAll(_simulators.Select(job => Task.Factory.StartNew(job.Run)).ToArray());

			_sumWriter.Finish();
		}
	}
}