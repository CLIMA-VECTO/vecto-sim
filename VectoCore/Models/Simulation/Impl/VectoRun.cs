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
		public string Name { get; protected set; }

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

			Initialize();
			try {
				IResponse response;
				do {
					response = DoSimulationStep();
					if (response is ResponseSuccess) {
						Container.CommitSimulationStep(AbsTime, dt);
						AbsTime += dt;
					}
				} while (response is ResponseSuccess);
			} catch (VectoSimulationException vse) {
				Container.FinishSimulation();
				throw new VectoSimulationException("{6} - absTime: {0}, distance: {1}, dt: {2}, v: {3}, Gear: {4} | {5}", vse,
					AbsTime, Container.Distance, dt, Container.VehicleSpeed, Container.Gear, vse.Message, Name);
			} catch (VectoException ve) {
				Container.FinishSimulation();
				throw new VectoSimulationException("{6} - absTime: {0}, distance: {1}, dt: {2}, v: {3}, Gear: {4} | {5}", ve,
					AbsTime, Container.Distance, dt, Container.VehicleSpeed, Container.Gear, ve.Message, Name);
			} catch (Exception e) {
				Container.FinishSimulation();
				throw new VectoSimulationException("{6} - absTime: {0}, distance: {1}, dt: {2}, v: {3}, Gear: {4} | {5}", e, AbsTime,
					Container.Distance, dt, Container.VehicleSpeed, Container.Gear, e.Message, Name);
			}
			Container.FinishSimulation();
			Log.Info("VectoJob finished.");
		}

		protected abstract IResponse DoSimulationStep();

		protected abstract IResponse Initialize();
	}
}