using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	/// <summary>
	/// Simulator for one vecto simulation job.
	/// </summary>
	public abstract class VectoRun : IVectoRun
	{
		protected TimeSpan _absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
		protected TimeSpan _dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

		public VectoRun(IVehicleContainer container)
		{
			Container = container;
			CyclePort = container.GetCycleOutPort();
		}

		protected SummaryFileWriter SumWriter { get; set; }

		protected string JobFileName { get; set; }

		protected string JobName { get; set; }

		protected ISimulationOutPort CyclePort { get; set; }
		protected IModalDataWriter DataWriter { get; set; }
		protected IVehicleContainer Container { get; set; }

		public IVehicleContainer GetContainer()
		{
			return Container;
		}

		public void Run()
		{
			LogManager.GetLogger(GetType()).Info("VectoJob started running.");
			IResponse response;
			do {
				response = DoSimulationStep();
				Container.CommitSimulationStep(_absTime.TotalSeconds, _dt.TotalSeconds);

				// set _dt to difference to next full second.
				_absTime += _dt;
			} while (response is ResponseSuccess);

			Container.FinishSimulation();

			LogManager.GetLogger(GetType()).Info("VectoJob finished.");
		}

		protected abstract IResponse DoSimulationStep();
	}
}