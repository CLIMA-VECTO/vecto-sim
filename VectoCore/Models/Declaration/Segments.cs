using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Segments : LookupData<VehicleCategory, AxleConfiguration, Kilogram, Kilogram, Segment>
	{
		public Segments()
		{
			ParseData(ReadCsvResource(RessourceHelper.Namespace + "SegmentTable.csv"));
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
			var row =
				SegmentTable.Rows.Cast<DataRow>().First(r => r.Field<string>("valid") == "1"
															&& r.Field<string>("vehiclecategory") == vehicleCategory.ToString()
															&& r.Field<string>("axleconf.") == axleConfiguration.GetName()
															&& r.ParseDouble("gvw_min").SI<Ton>() <= grossVehicleMassRating
															&& r.ParseDouble("gvw_max").SI<Ton>() > grossVehicleMassRating
					);
			var segment = new Segment {
				GrossVehicleWeightMin = row.ParseDouble("gvw_min").SI().Ton.Cast<Kilogram>(),
				GrossVehicleWeightMax = row.ParseDouble("gvw_max").SI().Ton.Cast<Kilogram>(),
				VehicleCategory = vehicleCategory,
				AxleConfiguration = axleConfiguration,
				VehicleClass = VehicleClassHelper.Parse(row.Field<string>("hdvclass")),
				AccelerationFile = RessourceHelper.ReadStream(RessourceHelper.Namespace + "VACC." + row.Field<string>(".vaccfile")),
				Missions = CreateMissions(grossVehicleMassRating, curbWeight, row).ToArray()
			};

			return segment;
		}

		private static IEnumerable<Mission> CreateMissions(Kilogram grossVehicleMassRating, Kilogram curbWeight, DataRow row)
		{
			var trailerOnlyInLongHaul = row.Field<string>("vehiclecategory") == VehicleCategory.RigidTruck.ToString() &&
										row.Field<string>("traileraxles-longhaul") != "-" &&
										row.Field<string>("traileraxles-other") == "-";

			var missionTypes = Enum.GetValues(typeof(MissionType)).Cast<MissionType>();
			foreach (var missionType in missionTypes.Where(m => row.Field<string>(m.ToString()) == "1")) {
				string vcdvField;
				string axleField;
				string trailerField;

				if (missionType == MissionType.LongHaul) {
					vcdvField = "crosswindcorrection-longhaul";
					axleField = "truckaxles-longhaul";
					trailerField = "traileraxles-longhaul";
				} else {
					vcdvField = "crosswindcorrection-other";
					axleField = "truckaxles-other";
					trailerField = "traileraxles-other";
				}

				var mission = new Mission {
					MissionType = missionType,
					CrossWindCorrection = row.Field<string>(vcdvField),
					MassExtra = row.ParseDouble("massextra-" + missionType.ToString().ToLower()).SI<Kilogram>(),
					CycleFile = RessourceHelper.ReadStream(RessourceHelper.Namespace + "MissionCycles." + missionType + ".vdri"),
					AxleWeightDistribution = row.Field<string>(axleField).Split('/').ToDouble().Select(x => x / 100.0).ToArray(),
					UseCdA2 = trailerOnlyInLongHaul && missionType != MissionType.LongHaul,
				};

				var trailerAxles = row.Field<string>(trailerField).Split('/');
				var count = 0;
				var weightPercent = 0.0;

				if (trailerAxles[0] != "-") {
					count = int.Parse(trailerAxles[1]);
					weightPercent = trailerAxles[0].ToDouble();
				}
				mission.TrailerAxleWeightDistribution = Enumerable.Repeat(weightPercent / count / 100.0, count).ToArray();

				mission.MinLoad = 0.SI<Kilogram>();
				mission.MaxLoad = grossVehicleMassRating - mission.MassExtra - curbWeight;

				var refLoadField = row.Field<string>("payload-" + missionType.ToString().ToLower());
				mission.RefLoad = CalculateRefLoad(grossVehicleMassRating, refLoadField, missionType);

				yield return mission;
			}
		}

		private static Kilogram CalculateRefLoad(Kilogram grossVehicleMassRating, string refLoadField,
			MissionType missionType)
		{
			const double longHaulFactor = 0.5882;
			const double otherFactor = 0.3941;
			var longHaulWeightDeduction = 2511.8.SI<Kilogram>();
			var otherWeightDeduction = 1705.9.SI<Kilogram>();

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