using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Tests.Utils
{
	public static class Port
	{
		public static IDriver AddComponent(IDrivingCycle prev, IDriver next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		public static IVehicle AddComponent(IDriver prev, IVehicle next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		public static IWheels AddComponent(IFvInProvider prev, IWheels next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}


		public static ITnOutProvider AddComponent(IWheels prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		public static IPowerTrainComponent AddComponent(IPowerTrainComponent prev, IPowerTrainComponent next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		public static void AddComponent(IPowerTrainComponent prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
		}
	}
}