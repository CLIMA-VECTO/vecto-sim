using System;
using System.Data;
using System.IO;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	/// <summary>
	/// Simulator for one vecto simulation job.
	/// </summary>
	public class VectoRun : IVectoRun
	{
		private TimeSpan _absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
		private TimeSpan _dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

		public VectoRun(IVehicleContainer container, IDrivingCycleOutPort cyclePort)
		{
			Container = container;
			CyclePort = cyclePort;
		}

		protected SummaryFileWriter SumWriter { get; set; }

		protected string JobFileName { get; set; }

		protected string JobName { get; set; }

		protected IDrivingCycleOutPort CyclePort { get; set; }
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
				response = CyclePort.Request(_absTime, _dt);
				while (response is ResponseFailTimeInterval) {
					_dt = (response as ResponseFailTimeInterval).DeltaT;
					response = CyclePort.Request(_absTime, _dt);
				}

				if (response is ResponseCycleFinished) {
					break;
				}

				var time = (_absTime + TimeSpan.FromTicks(_dt.Ticks / 2)).TotalSeconds;
				var simulationInterval = _dt.TotalSeconds;


				Container.CommitSimulationStep(time, simulationInterval);

				// set _dt to difference to next full second.
				_absTime += _dt;
				_dt = TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(_dt.Milliseconds);
			} while (response is ResponseSuccess);

			Container.FinishSimulation();

			LogManager.GetLogger(GetType()).Info("VectoJob finished.");
		}
	}
}