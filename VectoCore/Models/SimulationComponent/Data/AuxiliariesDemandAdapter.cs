using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class AuxiliariesDemandAdapter
	{
		private readonly DrivingCycleData _drivingCycle;
		private IEnumerator<DrivingCycleData.DrivingCycleEntry> _nextCycleEntry; 
		private readonly string _auxiliaryId;

		protected DrivingCycleData.DrivingCycleEntry CurrentCycleEntry { get; set; }
		//protected DrivingCycleData.DrivingCycleEntry NextCycleEntry { get { return _nextCycleEntry.Current; } }

		private ILog Log;

		public AuxiliariesDemandAdapter(DrivingCycleData inputData, string column = null)
		{
			Log = LogManager.GetLogger(this.GetType());
			_drivingCycle = inputData;
			_nextCycleEntry = _drivingCycle.Entries.GetEnumerator();
			_nextCycleEntry.MoveNext();
			CurrentCycleEntry = _drivingCycle.Entries.First();
			_auxiliaryId = column;
			if (_auxiliaryId != null && !_drivingCycle.Entries.First().AuxiliarySupplyPower.ContainsKey(_auxiliaryId)) {
				Log.ErrorFormat("driving cycle data does not contain column {0}", column);
				throw new VectoException(String.Format("driving cycle does not contain column {0}", column));
			}
		}

		public Watt GetPowerDemand(TimeSpan absTime, TimeSpan dt)
		{
			if (_nextCycleEntry.Current.Time <= absTime.TotalSeconds) {
				CurrentCycleEntry = _nextCycleEntry.Current;
				_nextCycleEntry.MoveNext();
			}
			return String.IsNullOrEmpty(_auxiliaryId) ? CurrentCycleEntry.AdditionalAuxPowerDemand : CurrentCycleEntry.AuxiliarySupplyPower[_auxiliaryId];
		}

	}
}