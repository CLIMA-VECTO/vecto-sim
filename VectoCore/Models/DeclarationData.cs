using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models
{
	public class DeclarationData
	{
		private static DeclarationData _instance;

		public Segments Segments;

		public Wheels Wheels { get; private set; }

		public Rims Rims { get; private set; }

		private DeclarationData()
		{
			Wheels = new Wheels();
			Rims = new Rims();
			Segments = new Segments();
		}

		public static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}

		public Segment GetSegment(VehicleCategory vehicleCategory, AxleConfiguration axleConfiguration,
			Kilogram grossVehicleMassRating, Kilogram curbWeight)
		{
			return Segments.Lookup(vehicleCategory, axleConfiguration, grossVehicleMassRating, curbWeight);
		}
	}

	public enum MissionType
	{
		LongHaul,
		RegionalDelivery,
		UrbanDelivery,
		MunicipalDelivery,
		Construction,
		HeavyUrban,
		Urban,
		Suburban,
		Interurban,
		Coach
	}

	public class Segments
	{
		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.SegmentTable.csv";

		private readonly DataTable _segmentTable;

		internal Segments()
		{
			var myAssembly = Assembly.GetExecutingAssembly();
			var file = myAssembly.GetManifestResourceStream(ResourceId);
			_segmentTable = VectoCSVFile.ReadStream(file);

			// normalize column names, remove whitespaces and lowercase
			foreach (DataColumn col in _segmentTable.Columns) {
				_segmentTable.Columns[col.ColumnName].ColumnName = col.ColumnName.ToLower().Replace(" ", "");
			}
		}

		public Segment Lookup(VehicleCategory vehicleCategory, AxleConfiguration axleConfiguration,
			Kilogram grossVehicleMassRating, Kilogram curbWeight)
		{
			var row = _segmentTable.Rows.Cast<DataRow>().First(r => r.Field<string>("tvehcat") == vehicleCategory.ToString()
																	&& r.Field<string>("taxleconf") == axleConfiguration.ToString()
																	&& r.Field<double>("ggw_min").SI<Ton>() <= grossVehicleMassRating.Double()
																	&& r.Field<double>("ggw_max").SI<Ton>() > grossVehicleMassRating.Double());
			var segment = new Segment();
			segment.HDVClass = row.Field<string>("hdv_class");
			segment.VACC = row.Field<string>("vacc");

			var missions = new List<Mission>();

			foreach (var missionType in Enum.GetValues(typeof(MissionType)).Cast<MissionType>()) {
				if (row.Field<bool>(missionType.ToString())) {
					var mission = new Mission();
					mission.MissionType = missionType;

					List<double> axles;
					string[] trailerAxles;

					if (missionType == MissionType.LongHaul) {
						mission.VCDV = row.Field<string>("vcdv-" + missionType.ToString().ToLower());
						axles = row.Field<string>("rigid/truckaxles-longhaul").Split('/').Select(double.Parse).ToList();
						trailerAxles = row.Field<string>("traileraxles-" + missionType.ToString().ToLower()).Split('/');
					} else {
						mission.VCDV = row.Field<string>("vcdv-other");
						axles = row.Field<string>("rigid/truckaxles-other").Split('/').Select(double.Parse).ToList();
						trailerAxles = row.Field<string>("traileraxles-" + missionType.ToString().ToLower()).Split('/');
					}

					var weightPercent = double.Parse(trailerAxles[0]);
					var count = int.Parse(trailerAxles[1]);
					if (count > 0) {
						axles.AddRange(Enumerable.Repeat(weightPercent / count, count));
					}

					mission.AxleWeightDistribution = axles.ToArray();

					mission.MassExtra = row.Field<double>("massextra-" + missionType.ToString().ToLower()).SI<Kilogram>();

					mission.MinLoad = 0.SI<Kilogram>();

					if (row.Field<string>("refload-" + missionType.ToString().ToLower()) == "f") {
						if (mission.MissionType == MissionType.LongHaul) {
							mission.RefLoad = 588.2 * grossVehicleMassRating - 2511.8;
						} else {
							mission.RefLoad = 394.1 * grossVehicleMassRating - 1705.9;
						}
					} else {
						mission.RefLoad = row.Field<double>("refload-" + missionType.ToString().ToLower()).SI<Kilogram>();
					}

					mission.MaxLoad = grossVehicleMassRating - mission.MassExtra - curbWeight;
					//todo: rename cycle files to equal the enumeration
					mission.CycleFile = missionType + ".vdri";
					missions.Add(mission);
				}
			}
			segment.Missions = missions.ToArray();
			return segment;
		}
	}


	public class Segment
	{
		public string HDVClass { get; internal set; }
		public string VACC { get; internal set; }
		public Mission[] Missions { get; internal set; }
	}

	public class Mission
	{
		public MissionType MissionType { get; set; }
		public string VCDV { get; set; }
		public double[] AxleWeightDistribution { get; set; }
		public Kilogram MassExtra { get; set; }
		public Kilogram RefLoad { get; set; }
		public Kilogram MinLoad { get; set; }
		public Kilogram MaxLoad { get; set; }
		public string CycleFile { get; set; }
	}
}