using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	//todo: add job tracking (state of jobs, ...)
	//todo: add job control (pause, stop)


	/// <summary>
	/// Container for simulation jobs.
	/// </summary>
	public class JobContainer : LoggingObject
	{
		private readonly List<IVectoRun> _runs = new List<IVectoRun>();
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

		public void AddRun(IVectoRun run)
		{
			_jobNumber++;
			_runs.Add(run);
		}

		public void AddRuns(IEnumerable<IVectoRun> runs)
		{
			_jobNumber++;
			_runs.AddRange(runs);
		}

		public void AddRuns(SimulatorFactory factory)
		{
			factory.SumWriter = _sumWriter;
			factory.JobNumber = _jobNumber++;
			_runs.AddRange(factory.SimulationRuns());
		}

		/// <summary>
		/// Execute all runs, waits until finished.
		/// </summary>
		public void Execute()
		{
			Log.Info("VectoRun started running. Executing Runs.");

			Task.WaitAll(_runs.Select(r => Task.Factory.StartNew(r.Run)).ToArray());

			_sumWriter.Finish();
		}
	}
}