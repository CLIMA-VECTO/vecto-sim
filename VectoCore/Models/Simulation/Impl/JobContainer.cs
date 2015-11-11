using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
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

			//_sumWriter.Finish();
        }

        public void Cancel()
        {
            foreach (var job in Runs) {
                if (job.Worker != null && job.Worker.WorkerSupportsCancellation) {
                    job.Worker.CancelAsync();
                }
            }
        }

        public void CancelCurrent()
        {
            foreach (var job in Runs) {
                if (job.Worker != null && job.Worker.IsBusy && job.Worker.WorkerSupportsCancellation) {
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

        public Dictionary<string, ProgressEntry> GetProgress()
        {
            return Runs.ToDictionary(jobEntry => jobEntry.Run.Name, entry => new ProgressEntry() {
                Progress = entry.Progress,
                Done = entry.Done,
                ExecTime = entry.ExecTime,
                Success = entry.Success,
                Canceled = entry.Canceled,
                Error = entry.ExecException
            });
        }

        public bool AllCompleted
        {
            get { return (Runs.Count(x => x.Done == true) == Runs.Count()); }
        }

        public class ProgressEntry
        {
            public double Progress;
            public double ExecTime;
            public Exception Error;
            public bool Canceled;
            public bool Success;
            public bool Done;
        }

        internal class JobEntry : LoggingObject
        {
            public IVectoRun Run;
            public JobContainer Container;
            public double Progress;
            public bool Done;
            public bool Started;
            public bool Success;
            public bool Canceled;
            public double ExecTime;
            public Exception ExecException;

            public BackgroundWorker Worker;

            public void DoWork(object sender, DoWorkEventArgs e)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var worker = sender as BackgroundWorker;
                try {
                    Run.Run(worker);
                } catch (Exception ex) {
                    Log.Error(ex, "Error during simulation run!");
                    ExecException = ex;
                }
                if (worker != null && worker.CancellationPending) {
                    e.Cancel = true;
                    Canceled = true;
                }
                stopWatch.Stop();
                Success = Run.FinishedWithoutErrors;
                Done = true;
                ExecTime = stopWatch.Elapsed.TotalMilliseconds;
                Container.JobCompleted(this);
            }

            public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {}

            public void ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                Progress = e.ProgressPercentage / 10000.0;
            }
        }
    }
}