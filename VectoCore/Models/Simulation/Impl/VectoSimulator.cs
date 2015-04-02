using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class VectoSimulator : IVectoSimulator
	{
		private TimeSpan _absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
		private TimeSpan _dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

		public VectoSimulator(IVehicleContainer container, IDrivingCycle cycle, IModalDataWriter dataWriter)
		{
			Container = container;
			Cycle = cycle;
			DataWriter = dataWriter;
		}

		protected IDrivingCycle Cycle { get; set; }
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
				response = Cycle.Request(_absTime, _dt);
				while (response is ResponseFailTimeInterval) {
					_dt = (response as ResponseFailTimeInterval).DeltaT;
					response = Cycle.Request(_absTime, _dt);
				}

				if (response is ResponseCycleFinished) {
					break;
				}


				DataWriter[ModalResultField.time] = (_absTime + TimeSpan.FromTicks(_dt.Ticks / 2)).TotalSeconds;
				DataWriter[ModalResultField.simulationInterval] = _dt.TotalSeconds;

				Container.CommitSimulationStep(DataWriter);

				// set _dt to difference to next full second.
				_absTime += _dt;
				_dt = TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(_dt.Milliseconds);
			} while (response is ResponseSuccess);

			Container.FinishSimulation(DataWriter);

			//todo: write vsum file

			LogManager.GetLogger(GetType()).Info("VectoJob finished.");
		}
	}
}