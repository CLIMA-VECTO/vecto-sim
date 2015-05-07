namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an Fv in port.
	/// </summary>
	public interface IRoadPortInProvider
	{
		/// <summary>
		/// Returns the inport to connect it to another outport.
		/// </summary>
		/// <returns></returns>
		IFvInPort InShaft();
	}

	/// <summary>
	/// Defines a method to acquire an Fv out port.
	/// </summary>
	public interface IRoadPortOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		IFvOutPort OutShaft();
	}
}