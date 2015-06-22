using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class AccelerationCurve : LookupData<MeterPerSecond, AccelerationCurve.AccelerationEntry>
	{
		private List<KeyValuePair<MeterPerSecond, AccelerationEntry>> _entries;

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.AccelerationFile.vacc"; }
		}

		protected override void ParseData(DataTable table)
		{
			_entries = table.Rows.Cast<DataRow>()
				.Select(r => new KeyValuePair<MeterPerSecond, AccelerationEntry>(
					r.ParseDouble("v").SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>(),
					new AccelerationEntry(r.ParseDouble("acc").SI<MeterPerSquareSecond>(),
						r.ParseDouble("dec").SI<MeterPerSquareSecond>())))
				.OrderBy(x => x.Key)
				.ToList();
		}

		public override AccelerationEntry Lookup(MeterPerSecond key)
		{
			var index = 1;
			if (key < _entries[0].Key) {
				Log.ErrorFormat("requested rpm below minimum rpm in pt1 - extrapolating. n: {0}, rpm_min: {1}",
					key.ConvertTo().Rounds.Per.Minute, _entries[0].Key.ConvertTo().Rounds.Per.Minute);
			} else {
				index = _entries.FindIndex(x => x.Key > key);
				if (index <= 0) {
					index = (key > _entries[0].Key) ? _entries.Count - 1 : 1;
				}
			}

			var acc = VectoMath.Interpolate(_entries[index - 1].Key, _entries[index].Key, _entries[index - 1].Value.Acceleration,
				_entries[index].Value.Acceleration, key);
			var dec = VectoMath.Interpolate(_entries[index - 1].Key, _entries[index].Key, _entries[index - 1].Value.Deceleration,
				_entries[index].Value.Deceleration, key);

			return new AccelerationEntry(acc, dec);
		}

		public class AccelerationEntry
		{
			public AccelerationEntry(MeterPerSquareSecond acceleration, MeterPerSquareSecond deceleration)
			{
				Acceleration = acceleration;
				Deceleration = deceleration;
			}

			public MeterPerSquareSecond Acceleration { get; set; }
			public MeterPerSquareSecond Deceleration { get; set; }
		}
	}
}