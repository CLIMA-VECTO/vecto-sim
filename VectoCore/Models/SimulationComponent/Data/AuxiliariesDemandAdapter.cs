using System;
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

		public Watt GetPowerDemand(TimeSpan absTime, TimeSpan dt)
		{
			var entry = _drivingCycle.Entries.FindLastIndex(x => x.Time <= absTime.TotalSeconds);
			//if (entry == null) {
			//	Log.ErrorFormat("could not find entry in driving cycle for time {0}", absTime.TotalSeconds);
			//	return 0;
			//}
			if (entry == -1) {
				Log.ErrorFormat("time: {0} could not find power demand for auxiliary {1}", absTime.TotalSeconds, _auxiliaryId);
				throw new VectoSimulationException(String.Format("time: {0} could not find power demand for auxiliary {1}", absTime.TotalSeconds, _auxiliaryId));
			}
			return _auxiliaryId == null ? _drivingCycle.Entries[entry].AdditionalAuxPowerDemand : _drivingCycle.Entries[entry].AuxiliarySupplyPower[_auxiliaryId];
		}

	}
}