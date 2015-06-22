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
		private readonly AccelerationCurve _accelerationCurve;
		private readonly ElectricSystem _electricSystem;

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

		public static AccelerationCurve AccelerationCurve
		{
			get { return Instance()._accelerationCurve; }
		}

		public static ElectricSystem ElectricSystem
		{
			get { return Instance()._electricSystem; }
		}

		private DeclarationData()
		{
			_wheels = new DeclarationWheels();
			_rims = new DeclarationRims();
			_segments = new DeclarationSegments();
			_pt1 = new DeclarationPT1();
			_accelerationCurve = new AccelerationCurve();
			_electricSystem = new ElectricSystem();
		}

		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}

	internal class ElectricSystem : LookupData<MissionType, string[], Watt>
	{
		private static string baseLine = "Baseline electric power consumption";
		private Dictionary<Tuple<MissionType, string>, Watt> _data = new Dictionary<Tuple<MissionType, string>, Watt>();

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.ES-Tech.csv"; }
		}

		protected override void ParseData(DataTable table)
		{
			foreach (DataRow row in table.Rows) {
				var name = row.Field<string>("Technology");
				foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
					_data[Tuple.Create(mission, name)] = row.ParseDouble(mission.ToString()).SI<Watt>();
				}
			}
		}

		public override Watt Lookup(MissionType key1, string[] key2)
		{
			var sum = _data[Tuple.Create(key1, baseLine)];

			foreach (var s in key2) {
				if (_data.ContainsKey(Tuple.Create(key1, s))) {
					sum -= _data[Tuple.Create(key1, s)];
				} else {
					Log.Warn("electric system technology not found.");
				}
			}
			return sum;
		}
	}
}