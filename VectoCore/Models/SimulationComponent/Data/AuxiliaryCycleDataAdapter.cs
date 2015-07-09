using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class AuxiliaryCycleDataAdapter : IAuxiliaryCycleData
	{
		private readonly string _auxiliaryId;
		private readonly DrivingCycleData _drivingCycle;
		private readonly IEnumerator<DrivingCycleData.DrivingCycleEntry> _nextCycleEntry;

		private readonly ILog Log;

		public AuxiliaryCycleDataAdapter(DrivingCycleData inputData, string column = null)
		{
			Log = LogManager.GetLogger(GetType());
			_drivingCycle = inputData;
			_nextCycleEntry = _drivingCycle.Entries.GetEnumerator();
			_nextCycleEntry.MoveNext();
			CurrentCycleEntry = _drivingCycle.Entries.First();
			_auxiliaryId = column;
			if (_auxiliaryId != null && !_drivingCycle.Entries.First().AuxiliarySupplyPower.ContainsKey(_auxiliaryId)) {
				Log.ErrorFormat("driving cycle data does not contain column {0}", column);
				throw new VectoException(string.Format("driving cycle does not contain column {0}", column));
			}
		}

		protected DrivingCycleData.DrivingCycleEntry CurrentCycleEntry { get; set; }

		public Watt GetPowerDemand(TimeSpan absTime, TimeSpan dt)
		{
			if (_nextCycleEntry.Current.Time <= absTime.TotalSeconds) {
				CurrentCycleEntry = _nextCycleEntry.Current;
				_nextCycleEntry.MoveNext();
			}
			return string.IsNullOrEmpty(_auxiliaryId)
				? CurrentCycleEntry.AdditionalAuxPowerDemand
				: CurrentCycleEntry.AuxiliarySupplyPower[_auxiliaryId];
		}
	}
}