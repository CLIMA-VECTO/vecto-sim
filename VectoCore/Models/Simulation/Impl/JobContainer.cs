using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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
		internal readonly List<JobEntry> Runs = new List<JobEntry>();
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
			Runs.Add(new JobEntry() {
				Run = run,
				Container = this,
			});
		}

		public void AddRuns(IEnumerable<IVectoRun> runs)
		{
			_jobNumber++;
			//Runs.AddRange(runs);
			foreach (var run in runs) {
				Runs.Add(new JobEntry() {
					Run = run,
					Container = this,
				});
			}
		}

		public void AddRuns(SimulatorFactory factory)
		{
			factory.SumWriter = _sumWriter;
			factory.JobNumber = _jobNumber++;
			AddRuns(factory.SimulationRuns());
		}

		/// <summary>
		/// Execute all runs, waits until finished.
		/// </summary>
		public void Execute(bool multithreaded = true)
		{
			Log.Info("VectoRun started running. Executing Runs.");

			foreach (var job in Runs) {
				job.Worker = new BackgroundWorker() {
					WorkerSupportsCancellation = true,
					WorkerReportsProgress = true,
				};
				job.Worker.DoWork += job.DoWork;
				job.Worker.ProgressChanged += job.ProgressChanged;
				job.Worker.RunWorkerCompleted += job.RunWorkerCompleted;
				if (multithreaded) {
					job.Started = true;
					job.Worker.RunWorkerAsync();
				}
			}
			if (!multithreaded) {
				var entry = Runs.First();
				entry.Started = true;
				entry.Worker.RunWorkerAsync();
			}
			//Task.WaitAll(_runs.Select(r => Task.Factory.StartNew(r.Run)).ToArray());

			_sumWriter.Finish();
		}

		public void Cancel()
		{
			foreach (var job in Runs) {
				if (job.Worker != null && job.Worker.WorkerSupportsCancellation) {
					job.Worker.CancelAsync();
				}
			}
		}

		private static AutoResetEvent resetEvent = new AutoResetEvent(false);

		public void WaitFinished()
		{
			resetEvent.WaitOne();
		}


		private void JobCompleted(JobEntry jobEntry)
		{
			var next = Runs.FirstOrDefault(x => x.Started == false);
			if (next != null) {
				next.Started = true;
				next.Worker.RunWorkerAsync();
			}
			if (Runs.Count(x => x.Done == true) == Runs.Count()) {
				_sumWriter.Finish();
				resetEvent.Set();
			}
		}

		public Dictionary<string, double> GetProgress()
		{
			return Runs.ToDictionary(jobEntry => jobEntry.Run.Name, jobEntry => jobEntry.Progress);
		}

		public bool AllCompleted
		{
			get { return (Runs.Count(x => x.Done == true) == Runs.Count()); }
		}

		internal class JobEntry
		{
			public IVectoRun Run;
			public JobContainer Container;
			public double Progress;
			public bool Done;
			public bool Started;
			public bool Success;
			public bool Canceled;

			public BackgroundWorker Worker;

			public void DoWork(object sender, DoWorkEventArgs e)
			{
				var worker = sender as BackgroundWorker;
				Run.Run(worker);
				if (worker != null && worker.CancellationPending) {
					e.Cancel = true;
					Canceled = true;
				}
				Success = Run.FinishedWithoutErrors;
				Done = true;
				Container.JobCompleted(this);
			}

			public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {}

			public void ProgressChanged(object sender, ProgressChangedEventArgs e)
			{
				Progress = e.ProgressPercentage / 1000.0;
			}
		}
	}
}