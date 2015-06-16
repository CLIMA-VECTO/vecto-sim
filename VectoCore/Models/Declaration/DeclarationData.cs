using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationData
	{
		private static DeclarationData _instance;
		private readonly DeclarationSegments _segments;
		private readonly DeclarationRims _rims;
		private readonly DeclarationWheels _wheels;
		private readonly DeclarationPT1 _pt1;

		public static DeclarationWheels Wheels
		{
			get { return Instance()._wheels; }
		}

		public static DeclarationRims Rims
		{
			get { return Instance()._rims; }
		}

		public static DeclarationSegments Segments
		{
			get { return Instance()._segments; }
		}

		public static DeclarationPT1 PT1
		{
			get { return Instance()._pt1; }
		}

		private DeclarationData()
		{
			_wheels = new DeclarationWheels();
			_rims = new DeclarationRims();
			_segments = new DeclarationSegments();
			_pt1 = new DeclarationPT1();
		}

		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}

	public class DeclarationPT1 : LookupData<Second, PerSecond>
	{
		private List<KeyValuePair<PerSecond, Second>> _entries;

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.PT1.csv"; }
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
				Log.ErrorFormat("requested rpm below minimum rpm in pt1 - extrapolating. n: {0}, rpm_min: {1}",
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