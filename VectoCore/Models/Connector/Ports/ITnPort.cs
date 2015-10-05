using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
	/// <summary>
	/// Defines a method to acquire an Tn in port.
	/// </summary>
	public interface ITnInProvider
	{
		/// <summary>
		/// Returns the inport to connect it to another outport.
		/// </summary>
		/// <returns></returns>
		ITnInPort InPort();
	}

	/// <summary>
	/// Defines a method to acquire an Tn out port.
	/// </summary>
	public interface ITnOutProvider
	{
		/// <summary>
		/// Returns the outport to send requests to.
		/// </summary>
		/// <returns></returns>
		ITnOutPort OutPort();
	}


	//========================================================================


	/// <summary>
	/// Defines a connect method to connect the inport to an outport.
	/// </summary>
	public interface ITnInPort
	{
		/// <summary>
		/// Connects the inport to another outport.
		/// </summary>
		void Connect(ITnOutPort other);
	}

	/// <summary>
	/// Defines a request method for a Tn-Out-Port.
	/// </summary>
	public interface ITnOutPort
	{
		/// <summary>
		/// Requests the Outport with the given torque [Nm] and angularVelocity [rad/s].
		/// </summary>
		IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false);

		IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity);
	}
}