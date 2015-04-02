namespace TUGraz.VectoCore.Models.Connector.Ports
{
	public interface ITnInPort : ITnPort, IInPort
	{
		void Connect(ITnOutPort other);
	}
}