using System.Collections.Generic;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockDrivingCycle : IDrivingCycleCockpit
	{
		private List<DrivingCycleData.DrivingCycleEntry>.Enumerator _left;
		private List<DrivingCycleData.DrivingCycleEntry>.Enumerator _right;

		public MockDrivingCycle(DrivingCycleData data)
		{
			_left = data.Entries.GetEnumerator();
			_right = data.Entries.GetEnumerator();
			_right.MoveNext();
		}

		public CycleData CycleData()
		{
			_left.MoveNext();
			_right.MoveNext();
			return new CycleData {
				AbsTime = 0.SI<Second>(),
				AbsDistance = 0.SI<Meter>(),
				LeftSample = _left.Current,
				RightSample = _right.Current
			};
		}
	}
}