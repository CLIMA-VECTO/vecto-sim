namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	public abstract class InPort: IInPort
	{
		public abstract void Connect(IOutPort other);

	}

}
