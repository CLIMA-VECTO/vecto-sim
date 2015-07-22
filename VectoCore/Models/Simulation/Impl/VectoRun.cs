using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	/// <summary>
	/// Simulator for one vecto simulation job.
	/// </summary>
	public abstract class VectoRun : IVectoRun
	{
		protected Second AbsTime = 0.SI<Second>();

		protected Second dt = 1.SI<Second>();

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
				if (response.ResponseType == ResponseType.Success) {
					Container.CommitSimulationStep(AbsTime, dt);
				}

				// set _dt to difference to next full second.
				AbsTime += dt;
			} while (response is ResponseSuccess);

			Container.FinishSimulation();

			LogManager.GetLogger(GetType()).Info("VectoJob finished.");
		}

		protected abstract IResponse DoSimulationStep();

		protected abstract IResponse Initialize();
	}
}