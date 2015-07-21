using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	/// <summary>
	/// Interface for getting an power demand of an auxiliary.
	/// </summary>
	public interface IAuxiliaryDemand
	{
		/// <summary>
		/// Returns the current power demand
		/// </summary>
		Watt GetPowerDemand();
	}
}