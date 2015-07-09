using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class WHTCCorrection : LookupData<MissionType, double, double, double, double>
	{
		private Dictionary<MissionType, WHTCCorrectionEntry> _data = new Dictionary<MissionType, WHTCCorrectionEntry>();

		public override double Lookup(MissionType mission, double rural, double urban, double motorway)
		{
			var entry = _data[mission];
			return rural * entry.Rural + urban * entry.Urban + motorway * entry.Motorway;
		}

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.WHTC-Weighting-Factors.csv"; }
		}

		protected override void ParseData(DataTable table)
		{
			_data.Clear();
			NormalizeTable(table);
			foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
				var values = table.Columns[mission.ToString().ToLower()].Values<string>().ToDouble().ToArray();
				_data[mission] = new WHTCCorrectionEntry { Rural = values[0], Urban = values[1], Motorway = values[2] };
			}
		}

		private class WHTCCorrectionEntry
		{
			public double Rural { get; set; }
			public double Urban { get; set; }
			public double Motorway { get; set; }
		}
	}
}