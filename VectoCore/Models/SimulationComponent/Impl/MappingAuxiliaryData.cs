using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class MappingAuxiliaryData
	{
		public double EfficiencyToSupply { get; set; }
		public double TransitionRatio { get; set; }
		public double EfficiencyToEngine { get; set; }

		public Watt GetPowerDemand(PerSecond nAuxiliary, Watt powerAuxOut)
		{
			throw new NotImplementedException();
		}

		public static MappingAuxiliaryData ReadFromFile(string filePath)
		{
			throw new NotImplementedException();
		}
	}
}