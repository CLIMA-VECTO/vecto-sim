using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class PT1 : LookupData<PerSecond, Second>
	{
		private List<KeyValuePair<PerSecond, Second>> _entries;

		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.PT1.csv";

		public PT1()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		protected override void ParseData(DataTable table)
		{
			_entries = table.Rows.Cast<DataRow>()
				.Select(r => new KeyValuePair<PerSecond, Second>(r.ParseDouble("rpm").RPMtoRad(), r.ParseDouble("PT1").SI<Second>()))
				.OrderBy(x => x.Key)
				.ToList();
		}

		public override Second Lookup(PerSecond key)
		{
			var index = 1;
			if (key < _entries[0].Key) {
				Log.Error("requested rpm below minimum rpm in pt1 - extrapolating. n: {0}, rpm_min: {1}",
					key.ConvertTo().Rounds.Per.Minute, _entries[0].Key.ConvertTo().Rounds.Per.Minute);
			} else {
				index = _entries.FindIndex(x => x.Key > key);
				if (index <= 0) {
					index = (key > _entries[0].Key) ? _entries.Count - 1 : 1;
				}
			}

			var pt1 = VectoMath.Interpolate(_entries[index - 1].Key, _entries[index].Key, _entries[index - 1].Value,
				_entries[index].Value, key);
			if (pt1 < 0) {
				throw new VectoException("The calculated value must not be smaller than 0. Value: " + pt1);
			}
			return pt1;
		}
	}
}