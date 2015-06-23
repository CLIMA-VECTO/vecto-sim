using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class ElectricSystem : LookupData<MissionType, string[], Watt>
	{
		private const string BaseLine = "Baseline electric power consumption";

		private readonly Dictionary<Tuple<MissionType, string>, Watt> _data =
			new Dictionary<Tuple<MissionType, string>, Watt>();

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.VAUX.ES-Tech.csv"; }
		}

		protected override void ParseData(DataTable table)
		{
			foreach (DataColumn col in table.Columns) {
				table.Columns[col.ColumnName].ColumnName = col.ColumnName.ToLower().Replace(" ", "");
			}

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

			foreach (var s in technologies) {
				if (_data.ContainsKey(Tuple.Create(missionType, s))) {
					// todo: ask raphael why these technologies have to be subtracted?
					sum -= _data[Tuple.Create(missionType, s)];
				} else {
					Log.Warn("electric system technology not found.");
				}
			}
			return sum;
		}
	}
}