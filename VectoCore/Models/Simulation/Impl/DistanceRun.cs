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
			var ds = Container.VehicleSpeed() * Constants.SimulationSettings.TargetTimeInterval;

			if (ds.IsEqual(0)) {
				// vehicle stands still, drive a certain distance...
				ds = Constants.SimulationSettings.DriveOffDistance;
			}

			IResponse response;
			do {
				response = CyclePort.Request(AbsTime, ds);
				response.Switch().
					Case<ResponseSuccess>(r => {
						ds = Container.VehicleSpeed().IsEqual(0)
							? Constants.SimulationSettings.DriveOffDistance
							: Constants.SimulationSettings.TargetTimeInterval * Container.VehicleSpeed();
					}).
					Case<ResponseDrivingCycleDistanceExceeded>(r => ds = r.MaxDistance).
					Case<ResponseCycleFinished>(r => { }).
					Default(r => { throw new VectoException("DistanceRun got an unexpected response: {0}", r); });
			} while (!(response is ResponseSuccess || response is ResponseCycleFinished));

			return response;
		}

		protected override IResponse Initialize()
		{
			return CyclePort.Initialize();
		}
	}
}