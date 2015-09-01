using System.Collections.Generic;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	public interface IRoadLookAhead
	{
		IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Meter lookaheadDistance);

		IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Second time);
	}
}