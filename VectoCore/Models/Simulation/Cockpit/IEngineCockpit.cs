using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Cockpit
{
	public interface IEngineCockpit
	{
		/// <summary>
		///     [rad/s]
		/// </summary>
		PerSecond EngineSpeed();
	}
}