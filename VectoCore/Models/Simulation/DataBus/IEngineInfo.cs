using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	/// <summary>
	/// Defines a method to access shared data of the engine.
	/// </summary>
	public interface IEngineInfo
	{
		/// <summary>
		/// [rad/s] The current engine speed.
		/// </summary>
		PerSecond EngineSpeed { get; }
	}
}