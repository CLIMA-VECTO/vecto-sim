namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	/// <summary>
	/// Defines a method to access shared data of the gearbox.
	/// </summary>
	public interface IGearboxInfo
	{
		/// <summary>
		/// Returns the current gear.
		/// </summary>
		/// <returns></returns>
		uint Gear { get; }
	}
}