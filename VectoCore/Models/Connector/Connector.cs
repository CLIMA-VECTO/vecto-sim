using TUGraz.VectoCore.Models.Connector.Ports.Impl;

namespace TUGraz.VectoCore.Models.Connector
{
	class Connector<TI, TO, TP> 
		where TI : InPort, TP
		where TO : OutPort, TP
	{
		protected TI InPort;
		protected TO OutPort;

		public void Connect(TI inPort, TO outPort)
		{
			InPort = inPort;
			OutPort = outPort;

			InPort.Connect(OutPort);
		}
	}

}
