using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Fan : LookupData<MissionType, string, Watt>
	{
		private readonly Dictionary<Tuple<MissionType, string>, Watt> _data =
			new Dictionary<Tuple<MissionType, string>, Watt>();

		private const string DefaultTechnology = "Crankshaft mounted - Electronically controlled visco clutch (Default)";

		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.Fan-Tech.csv";

		public Fan()
		{
			ParseData(ReadCsvResource(ResourceId));
		}


		protected override void ParseData(DataTable table)
		{
			NormalizeTable(table);

			_data.Clear();
			foreach (DataRow row in table.Rows) {
				foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
					_data[Tuple.Create(mission, row.Field<string>("Technology"))] =
						row.ParseDouble(mission.ToString().ToLower()).SI<Watt>();
				}
			}
		}

		public override Watt Lookup(MissionType mission, string technology)
		{
			if (string.IsNullOrWhiteSpace(technology.Trim())) {
				technology = DefaultTechnology;
			}
			return _data[Tuple.Create(mission, technology)];
		}
	}
}