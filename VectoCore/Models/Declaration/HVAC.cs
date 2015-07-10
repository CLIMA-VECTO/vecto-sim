using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class HeatingVentilationAirConditioning : LookupData<MissionType, string, Watt>
	{
		private readonly Dictionary<Tuple<MissionType, string>, Watt> _data =
			new Dictionary<Tuple<MissionType, string>, Watt>();

		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.HVAC-Table.csv";

		public HeatingVentilationAirConditioning()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		public override Watt Lookup(MissionType mission, string hdvClass)
		{
			return _data[Tuple.Create(mission, hdvClass)];
		}

		protected override void ParseData(DataTable table)
		{
			_data.Clear();
			NormalizeTable(table);

			foreach (DataRow row in table.Rows) {
				var hdvClass = row.Field<string>("hdvclass/power");
				foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
					_data[Tuple.Create(mission, hdvClass)] = row.ParseDouble(mission.ToString().ToLower()).SI<Watt>();
				}
			}
		}
	}
}