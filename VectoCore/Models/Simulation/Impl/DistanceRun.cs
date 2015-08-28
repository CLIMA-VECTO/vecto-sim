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
			var ds = Container.VehicleSpeed() * Constants.SimulationSettings.TargetTimeInterval;

			if (ds.IsEqual(0)) {
				// vehicle stands still, drive a certain distance...
				ds = Constants.SimulationSettings.DriveOffDistance;
			}

			IResponse response = null;
			var requestDone = false;
			while (!requestDone) {
				response = CyclePort.Request(AbsTime, ds);

				response.Switch().
					Case<ResponseSuccess>(r => {
						AbsTime = AbsTime + r.SimulationInterval;
						dt = r.SimulationInterval;
						requestDone = true;
					}).
					Case<ResponseCycleFinished>(() => requestDone = true).
					Case<ResponseDrivingCycleDistanceExceeded>(r => { ds = r.MaxDistance; });
			}

			//while (response is ResponseFailTimeInterval) {
			//	_dt = (response as ResponseFailTimeInterval).DeltaT;
			//	response = CyclePort.Request(_absTime, _dt);
			//}

			return response;
		}

		protected override IResponse Initialize()
		{
			return CyclePort.Initialize();
		}
	}
}