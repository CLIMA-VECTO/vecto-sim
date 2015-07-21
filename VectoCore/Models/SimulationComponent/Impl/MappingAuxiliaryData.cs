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
			//todo GetPowerDemand
			throw new NotImplementedException();
		}

		public static MappingAuxiliaryData ReadFromFile(string filePath)
		{
			//todo ReadFromFile
			throw new NotImplementedException();
		}
	}
}