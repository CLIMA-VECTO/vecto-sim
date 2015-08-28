using System.Collections.Generic;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockDrivingCycle : VectoSimulationComponent, IDrivingCycleInfo
	{
		private List<DrivingCycleData.DrivingCycleEntry>.Enumerator _left;
		private List<DrivingCycleData.DrivingCycleEntry>.Enumerator _right;

		public MockDrivingCycle(IVehicleContainer container, DrivingCycleData data) : base(container)
		{
			_left = data.Entries.GetEnumerator();
			_right = data.Entries.GetEnumerator();
			_left.MoveNext();
			_right.MoveNext();
			_right.MoveNext();
		}


		public CycleData CycleData()
		{
			return new CycleData {
				AbsTime = 0.SI<Second>(),
				AbsDistance = 0.SI<Meter>(),
				LeftSample = _left.Current,
				RightSample = _right.Current
			};
		}

		protected override void DoWriteModalResults(IModalDataWriter writer) {}

		protected override void DoCommitSimulationStep()
		{
			_left.MoveNext();
			_right.MoveNext();
		}
	}
}