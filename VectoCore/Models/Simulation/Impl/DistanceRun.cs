using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class DistanceRun : VectoRun
	{
		public DistanceRun(IVehicleContainer container) : base(container) {}

		protected override Connector.Ports.IResponse DoSimulationStep()
		{
			_dt = TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(_dt.Milliseconds);

			var response = CyclePort.Request(_absTime, _dt);
			while (response is ResponseFailTimeInterval) {
				_dt = (response as ResponseFailTimeInterval).DeltaT;
				response = CyclePort.Request(_absTime, _dt);
			}

			if (response is ResponseCycleFinished) {
				return response;
			}

			var time = (_absTime + TimeSpan.FromTicks(_dt.Ticks / 2)).TotalSeconds;
			var simulationInterval = _dt.TotalSeconds;
			return response;
		}
	}
}