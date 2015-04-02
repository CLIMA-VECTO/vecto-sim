namespace TUGraz.VectoCore.Models.Connector.Ports
{
	public interface IDriverDemandInPort
	{
		void Connect(IDriverDemandOutPort other);
	}
}