using System;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class AuxiliaryDemand : IAuxiliaryDemand
	{
		private readonly Func<Watt> _getPower;

		/// <summary>
		/// Creates a demand adapter which takes a specific aux column data from the cycle as base.
		/// </summary>
		/// <param name="drivingCycle"></param>
		/// <param name="auxiliaryId"></param>
		public AuxiliaryDemand(IDrivingCycleCockpit drivingCycle, string auxiliaryId)
		{
			if (!drivingCycle.CycleData().LeftSample.AuxiliarySupplyPower.ContainsKey(auxiliaryId)) {
				var error = string.Format("driving cycle does not contain column for auxiliary: {0}", auxiliaryId);
				LogManager.GetLogger(GetType()).ErrorFormat(error);
				throw new VectoException(error);
			}

			_getPower = () => drivingCycle.CycleData().LeftSample.AuxiliarySupplyPower[auxiliaryId];
		}

		/// <summary>
		/// Creates a demand adapter which takes the single Additional Aux Power Demand Data as base.
		/// </summary>
		/// <param name="drivingCycle"></param>
		public AuxiliaryDemand(IDrivingCycleCockpit drivingCycle)
		{
			_getPower = () => drivingCycle.CycleData().LeftSample.AdditionalAuxPowerDemand;
		}

		/// <summary>
		/// Creates a demand adapter which uses a constant power value as base.
		/// </summary>
		/// <param name="constantPowerDemand"></param>
		public AuxiliaryDemand(Watt constantPowerDemand)
		{
			_getPower = () => constantPowerDemand;
		}

		public Watt GetPowerDemand()
		{
			return _getPower();
		}
	}
}