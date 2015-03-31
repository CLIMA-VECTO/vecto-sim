using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class AuxiliariesDemandAdapter
	{
		private readonly DrivingCycleData _drivingCycle;
		private readonly string _auxiliaryId;

		private ILog Log;

		public AuxiliariesDemandAdapter(DrivingCycleData inputData, string column = null)
		{
			Log = LogManager.GetLogger(this.GetType());
			_drivingCycle = inputData;
			_auxiliaryId = column;
			if (_auxiliaryId != null && !_drivingCycle.Entries.First().AuxiliarySupplyPower.ContainsKey(_auxiliaryId)) {
				Log.ErrorFormat("driving cycle data does not contain column {0}", column);
				throw new VectoException(String.Format("driving cycle does not contain column {0}", column));
			}
		}

		public double GetPowerDemand(TimeSpan absTime, TimeSpan dt)
		{
			var entry = _drivingCycle.Entries.FindIndex(x => x.Time > absTime.TotalSeconds);
			//if (entry == null) {
			//	Log.ErrorFormat("could not find entry in driving cycle for time {0}", absTime.TotalSeconds);
			//	return 0;
			//}
			Log.ErrorFormat("Found Entry at index {0}", entry);
			return _auxiliaryId == null ? _drivingCycle.Entries[entry].AdditionalAuxPowerDemand * 1000 : _drivingCycle.Entries[entry].AuxiliarySupplyPower[_auxiliaryId] * 1000;
		}

	}
}