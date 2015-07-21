using System.Collections.Generic;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockAuxiliaryDemand : IAuxiliaryDemand
	{
		private List<DrivingCycleData.DrivingCycleEntry>.Enumerator _it;

		public MockAuxiliaryDemand(DrivingCycleData data)
		{
			_it = data.Entries.GetEnumerator();
		}

		public Watt GetPowerDemand()
		{
			_it.MoveNext();
			return _it.Current.AdditionalAuxPowerDemand;
		}
	}
}