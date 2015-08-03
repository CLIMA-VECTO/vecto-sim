using System;
using TUGraz.VectoCore.Configuration;
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
			var ds = (Container.VehicleSpeed() * Constants.SimulationSettings.TargetTimeInterval).Cast<Meter>();

			if (ds.IsEqual(0)) {
				// vehicle stands still, drive a certain distance...
				ds = Constants.SimulationSettings.DriveOffDistance;
			}

			IResponse response;
			var requestDone = false;

			do {
				response = CyclePort.Request(AbsTime, ds);
				switch (response.ResponseType) {
					case ResponseType.Success:
						requestDone = true;
						break;
					case ResponseType.CycleFinished:
						requestDone = true;
						break;
					case ResponseType.DrivingCycleDistanceExceeded:
						break;
				}
			} while (!requestDone);

			//while (response is ResponseFailTimeInterval) {
			//	_dt = (response as ResponseFailTimeInterval).DeltaT;
			//	response = CyclePort.Request(_absTime, _dt);
			//}

			if (response is ResponseCycleFinished) {
				return response;
			}

			AbsTime = AbsTime + response.SimulationInterval;
			dt = response.SimulationInterval;
			return response;
		}

		protected override IResponse Initialize()
		{
			return CyclePort.Initialize();
		}
	}
}