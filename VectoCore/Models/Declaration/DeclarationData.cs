using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
		private readonly ElectricSystem _electricSystem;
		private readonly DeclarationFan _fan;

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

		public static ElectricSystem ElectricSystem
		{
			get { return Instance()._electricSystem; }
		}

		public static DeclarationFan Fan
		{
			get { return Instance()._fan; }
		}

		private DeclarationData()
		{
			_wheels = new DeclarationWheels();
			_rims = new DeclarationRims();
			_segments = new DeclarationSegments();
			_pt1 = new DeclarationPT1();
			_electricSystem = new ElectricSystem();
			_fan = new DeclarationFan();
		}

		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}

	public class DeclarationFan : LookupData<MissionType, string, Watt>
	{
		private readonly Dictionary<Tuple<MissionType, string>, Watt> _data =
			new Dictionary<Tuple<MissionType, string>, Watt>();

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.Fan-Tech.csv"; }
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
			return _data[Tuple.Create(mission, technology)];
		}
	}
}