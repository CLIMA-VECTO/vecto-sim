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
			var response = CyclePort.Request(AbsTime, dt);
			return response;
		}

		protected override IResponse Initialize()
		{
			return CyclePort.Initialize();
		}
	}
}