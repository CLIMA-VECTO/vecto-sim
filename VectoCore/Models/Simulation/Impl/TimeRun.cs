using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class TimeRun : VectoRun
	{
		public TimeRun(IVehicleContainer container) : base(container) {}

		protected override IResponse DoSimulationStep()
		{
			var dt = 1.SI<Second>();

			var response = CyclePort.Request(AbsTime, dt);

			if (response is ResponseCycleFinished) {
				return response;
			}

			AbsTime = AbsTime + dt;
			return response;
		}

		protected override IResponse Initialize()
		{
			throw new System.NotImplementedException();
		}
	}
}