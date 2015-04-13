using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Cockpit
{
	/// <summary>
	/// Defines a method to access shared data of the engine.
	/// </summary>
	public interface IEngineCockpit
	{
		/// <summary>
		/// [rad/s] The current engine speed.
		/// </summary>
		RadianPerSecond EngineSpeed();
	}
}