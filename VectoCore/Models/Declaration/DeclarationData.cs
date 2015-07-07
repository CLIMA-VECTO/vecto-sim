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
		private DeclarationSegments _segments;
		private DeclarationRims _rims;
		private DeclarationWheels _wheels;
		private DeclarationPT1 _pt1;
		private ElectricSystem _electricSystem;
		private DeclarationFan _fan;
		private DeclarationHVAC _hvac;

		public static DeclarationWheels Wheels
		{
			get { return Instance()._wheels ?? (Instance()._wheels = new DeclarationWheels()); }
		}

		public static DeclarationRims Rims
		{
			get { return Instance()._rims ?? (Instance()._rims = new DeclarationRims()); }
		}

		public static DeclarationSegments Segments
		{
			get { return Instance()._segments ?? (Instance()._segments = new DeclarationSegments()); }
		}

		public static DeclarationPT1 PT1
		{
			get { return Instance()._pt1 ?? (Instance()._pt1 = new DeclarationPT1()); }
		}

		public static ElectricSystem ElectricSystem
		{
			get { return Instance()._electricSystem ?? (Instance()._electricSystem = new ElectricSystem()); }
		}

		public static DeclarationFan Fan
		{
			get { return Instance()._fan ?? (Instance()._fan = new DeclarationFan()); }
		}

		public static DeclarationHVAC HVAC
		{
			get { return Instance()._hvac ?? (Instance()._hvac = new DeclarationHVAC()); }
		}

		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}

	public class DeclarationHVAC : LookupData<MissionType, string, Watt>
	{
		private Dictionary<Tuple<MissionType, string>, Watt> _data = new Dictionary<Tuple<MissionType, string>, Watt>();

		public override Watt Lookup(MissionType mission, string hdvClass)
		{
			return _data[Tuple.Create(mission, hdvClass)];
		}

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.VAUX.HVAC-Table.csv"; }
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

	public class DeclarationFan : LookupData<MissionType, string, Watt>
	{
		private readonly Dictionary<Tuple<MissionType, string>, Watt> _data =
			new Dictionary<Tuple<MissionType, string>, Watt>();

		private const string DefaultTechnology = "Crankshaft mounted - Electronically controlled visco clutch (Default)";

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.VAUX.Fan-Tech.csv"; }
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