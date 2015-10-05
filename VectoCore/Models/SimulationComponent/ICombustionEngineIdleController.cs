using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface ICombustionEngineIdleController : ITnOutPort
	{
		ITnOutPort RequestPort { set; }

		void Reset();
	}
}