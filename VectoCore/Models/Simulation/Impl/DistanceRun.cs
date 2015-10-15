using System;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class DistanceRun : VectoRun
	{
		public DistanceRun(IVehicleContainer container) : base(container) {}

		protected override IResponse DoSimulationStep()
		{
			// estimate distance to be traveled within the next TargetTimeInterval
			var ds = Container.VehicleSpeed.IsEqual(0)
				? Constants.SimulationSettings.DriveOffDistance
				: Constants.SimulationSettings.TargetTimeInterval * Container.VehicleSpeed;

			var loopCount = 0;
			IResponse response;
			do {
				response = CyclePort.Request(AbsTime, ds);
				response.Switch().
					Case<ResponseSuccess>(r => {
						dt = r.SimulationInterval;
					}).
					Case<ResponseDrivingCycleDistanceExceeded>(r => {
						if (r.MaxDistance.IsSmallerOrEqual(0)) {
							throw new VectoSimulationException("DistanceExceeded, MaxDistance is invalid: {0}", r.MaxDistance);
						}
						ds = r.MaxDistance;
					}).
					Case<ResponseCycleFinished>(r => {
						Log.Info("========= Driving Cycle Finished");
					}).
					Default(r => {
						throw new VectoException("DistanceRun got an unexpected response: {0}", r);
					});
				if (loopCount++ > Constants.SimulationSettings.MaximumIterationCountForSimulationStep) {
					throw new VectoSimulationException("Maximum iteration count for a single simulation interval reached! Aborting!");
				}
			} while (!(response is ResponseSuccess || response is ResponseCycleFinished));

			return response;
		}

		protected override IResponse Initialize()
		{
			return CyclePort.Initialize();
		}
	}
}