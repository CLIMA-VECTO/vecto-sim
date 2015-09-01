using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class ElectricSystem : LookupData<MissionType, string[], Watt>
	{
		private readonly Alternator _alternator = new Alternator();

		private const string BaseLine = "Baseline electric power consumption";

		private readonly Dictionary<Tuple<MissionType, string>, Watt> _data =
			new Dictionary<Tuple<MissionType, string>, Watt>();

		protected string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.ES-Tech.csv";


		public ElectricSystem()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		protected override void ParseData(DataTable table)
		{
			NormalizeTable(table);

			foreach (DataRow row in table.Rows) {
				var name = row.Field<string>("Technology");
				foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
					_data[Tuple.Create(mission, name)] = row.ParseDouble(mission.ToString().ToLower()).SI<Watt>();
				}
			}
		}

		public override Watt Lookup(MissionType missionType, string[] technologies)
		{
			var sum = _data[Tuple.Create(missionType, BaseLine)];

			if (technologies != null) {
				foreach (var s in technologies) {
					Watt w;
					if (_data.TryGetValue(Tuple.Create(missionType, s), out w)) {
						sum += w;
					} else {
						Log.Error(string.Format("electric system technology not found: {0}", s));
					}
				}
			}
			return sum / _alternator.Lookup(missionType, null);
		}

		private class Alternator : LookupData<MissionType, string, double>
		{
			private const string Default = "Standard alternator";

			private readonly Dictionary<Tuple<MissionType, string>, double> _data =
				new Dictionary<Tuple<MissionType, string>, double>();

			protected string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.ALT-Tech.csv";


			public Alternator()
			{
				ParseData(ReadCsvResource(ResourceId));
			}

			protected override void ParseData(DataTable table)
			{
				NormalizeTable(table);

				foreach (DataRow row in table.Rows) {
					var name = row.Field<string>("Technology");
					foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
						_data[Tuple.Create(mission, name)] = row.ParseDouble(mission.ToString().ToLower());
					}
				}
			}

			public override double Lookup(MissionType missionType, string technology)
			{
				if (string.IsNullOrWhiteSpace(technology)) {
					technology = Default;
				}

				return _data[Tuple.Create(missionType, technology)];
			}
		}
	}
}