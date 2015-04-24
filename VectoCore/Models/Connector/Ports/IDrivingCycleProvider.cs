namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an DriverCycle Demand out port.
	/// </summary>
	public interface IDrivingCycleOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		IDrivingCycleOutPort OutShaft();
	}
}