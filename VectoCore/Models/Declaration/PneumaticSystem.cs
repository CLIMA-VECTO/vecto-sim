using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class PneumaticSystem : LookupData<MissionType, VehicleClass, Watt>
	{
		private readonly Dictionary<Tuple<MissionType, VehicleClass>, Watt> _data =
			new Dictionary<Tuple<MissionType, VehicleClass>, Watt>();

		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.PS-Table.csv";

		public PneumaticSystem()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		public override Watt Lookup(MissionType mission, VehicleClass hdvClass)
		{
			return _data[Tuple.Create(mission, hdvClass)];
		}

		protected override void ParseData(DataTable table)
		{
			_data.Clear();
			NormalizeTable(table);

			foreach (DataRow row in table.Rows) {
				var hdvClass = VehicleClassHelper.Parse(row.Field<string>("hdvclass/power"));
				foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
					_data[Tuple.Create(mission, hdvClass)] = row.ParseDouble(mission.ToString().ToLower()).SI<Watt>();
				}
			}
		}
	}
}