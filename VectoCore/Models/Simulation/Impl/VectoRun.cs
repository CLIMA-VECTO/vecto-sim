using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	/// <summary>
	/// Simulator for one vecto simulation job.
	/// </summary>
	public abstract class VectoRun : LoggingObject, IVectoRun
	{
		protected Second AbsTime = 0.SI<Second>();
		protected Second dt = 1.SI<Second>();
		protected SummaryFileWriter SumWriter { get; set; }
		protected string JobFileName { get; set; }
		protected string JobName { get; set; }
		protected ISimulationOutPort CyclePort { get; set; }
		//protected IModalDataWriter DataWriter { get; set; }
		protected IVehicleContainer Container { get; set; }

		protected VectoRun(IVehicleContainer container)
		{
			Container = container;
			CyclePort = container.GetCycleOutPort();
		}

		public IVehicleContainer GetContainer()
		{
			return Container;
		}


		public void Run()
		{
			Log.Info("VectoJob started running.");
			IResponse response;

			Initialize();
			try {
				do {
					response = DoSimulationStep();
					if (response is ResponseSuccess) {
						Container.CommitSimulationStep(AbsTime, dt);

						AbsTime += dt;
					}
				} while (response is ResponseSuccess);
			} catch (VectoSimulationException vse) {
				Container.FinishSimulation();
				throw new VectoSimulationException("absTime: {0}, distance: {1}, dt: {2}, v: {3}, Gear: {4}", vse, AbsTime,
					Container.Distance, dt, Container.VehicleSpeed, Container.Gear, vse.Message);
			} catch (VectoException ve) {
				Container.FinishSimulation();
				throw new VectoSimulationException("absTime: {0}, distance: {1}, dt: {2}, v: {3}, Gear: {4}", ve, AbsTime,
					Container.Distance, dt, Container.VehicleSpeed, Container.Gear, ve.Message);
			} catch (Exception e) {
				Container.FinishSimulation();
				throw new VectoSimulationException("absTime: {0}, distance: {1}, dt: {2}, v: {3}, Gear: {4}", e, AbsTime,
					Container.Distance, dt, Container.VehicleSpeed, Container.Gear, e.Message);
			}
			Container.FinishSimulation();
			Log.Info("VectoJob finished.");
		}

		protected abstract IResponse DoSimulationStep();

		protected abstract IResponse Initialize();
	}
}