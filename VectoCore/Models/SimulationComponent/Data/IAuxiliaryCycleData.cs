using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public interface IAuxiliaryCycleData
	{
		Watt GetPowerDemand(TimeSpan absTime, TimeSpan dt);
	}
}