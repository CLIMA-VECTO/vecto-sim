﻿using System;
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
				ds = Constants.SimulationSettings.DriveOffDistance;
			}

			var response = CyclePort.Request(AbsTime, ds);

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
			throw new NotImplementedException();
		}
	}
}