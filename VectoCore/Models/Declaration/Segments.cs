using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Segments : LookupData<VehicleCategory, AxleConfiguration, Kilogram, Kilogram, Segment>
	{
		private const string ResourceNamespace = "TUGraz.VectoCore.Resources.Declaration.";

		protected override string ResourceId
		{
			get { return ResourceNamespace + "SegmentTable.csv"; }
		}

		protected override void ParseData(DataTable table)
		{
			// normalize column names, remove whitespaces and lowercase
			foreach (DataColumn col in table.Columns) {
				table.Columns[col.ColumnName].ColumnName = col.ColumnName.ToLower().Replace(" ", "");
			}
			SegmentTable = table.Copy();
		}

		private DataTable SegmentTable { get; set; }

		public override Segment Lookup(VehicleCategory vehicleCategory, AxleConfiguration axleConfiguration,
			Kilogram grossVehicleMassRating, Kilogram curbWeight)
		{
			var row = SegmentTable.Rows.Cast<DataRow>().First(r => r.Field<string>("tvehcat") == vehicleCategory.ToString()
																	&& r.Field<string>("taxleconf") == axleConfiguration.GetName()
																	&& r.ParseDouble("gvw_min").SI<Ton>() <= grossVehicleMassRating
																	&& r.ParseDouble("gvw_max").SI<Ton>() > grossVehicleMassRating);
			var segment = new Segment {
				GrossVehicleWeightMin = row.ParseDouble("gvw_min").SI().Ton.Cast<Kilogram>(),
				GrossVehicleWeightMax = row.ParseDouble("gvw_max").SI().Ton.Cast<Kilogram>(),
				VehicleCategory = vehicleCategory,
				AxleConfiguration = axleConfiguration,
				VehicleClass = row.Field<string>("hdv_class"),
				AccelerationFile = RessourceHelper.ReadStream(ResourceNamespace + "VACC." + row.Field<string>("vacc")),
				Missions = CreateMissions(grossVehicleMassRating, curbWeight, row).ToArray()
			};
			return segment;
		}

		private static IEnumerable<Mission> CreateMissions(Kilogram grossVehicleMassRating, Kilogram curbWeight, DataRow row)
		{
			var missionTypes = Enum.GetValues(typeof(MissionType)).Cast<MissionType>();
			foreach (var missionType in missionTypes.Where(m => row.Field<string>(m.ToString()) == "1")) {
				string vcdvField;
				string axleField;
				string trailerField;

				if (missionType == MissionType.LongHaul) {
					vcdvField = "vcdv-longhaul";
					axleField = "rigid/truckaxles-longhaul";
					trailerField = "traileraxles-longhaul";
				} else {
					vcdvField = "vcdv-other";
					axleField = "rigid/truckaxles-other";
					trailerField = "traileraxles-other";
				}

				var mission = new Mission {
					MissionType = missionType,
					CrossWindCorrectionFile = RessourceHelper.ReadStream(ResourceNamespace + "VCDV." + row.Field<string>(vcdvField)),
					MassExtra = row.ParseDouble("massextra-" + missionType.ToString().ToLower()).SI<Kilogram>(),
					CycleFile = RessourceHelper.ReadStream(ResourceNamespace + "MissionCycles." + missionType + ".vdri"),
					AxleWeightDistribution = row.Field<string>(axleField).Split('/').ToDouble().Select(x => x / 100.0).ToArray()
				};

				var trailerAxles = row.Field<string>(trailerField).Split('/');
				var count = int.Parse(trailerAxles[1]);
				var weightPercent = trailerAxles[0].ToDouble();
				mission.TrailerAxleWeightDistribution = Enumerable.Repeat(weightPercent / count / 100.0, count).ToArray();

				mission.MinLoad = 0.SI<Kilogram>();
				mission.MaxLoad = grossVehicleMassRating - mission.MassExtra - curbWeight;

				var refLoadField = row.Field<string>("refload-" + missionType.ToString().ToLower());
				mission.RefLoad = CalculateRefLoad(grossVehicleMassRating, refLoadField, missionType);

				yield return mission;
			}
		}

		private static Kilogram CalculateRefLoad(Kilogram grossVehicleMassRating, string refLoadField,
			MissionType missionType)
		{
			const double longHaulFactor = 0.5882;
			const double otherFactor = 0.3941;
			const double longHaulWeightDeduction = 2511.8;
			const double otherWeightDeduction = 1705.9;

			Kilogram refLoad;
			if (refLoadField == "f") {
				if (missionType == MissionType.LongHaul) {
					refLoad = longHaulFactor * grossVehicleMassRating - longHaulWeightDeduction;
				} else {
					refLoad = otherFactor * grossVehicleMassRating - otherWeightDeduction;
				}
			} else {
				refLoad = refLoadField.ToDouble().SI<Kilogram>();
			}

			return VectoMath.Min(refLoad, grossVehicleMassRating);
		}
	}
}