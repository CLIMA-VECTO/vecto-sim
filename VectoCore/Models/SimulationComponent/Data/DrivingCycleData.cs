using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Common.Logging;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class DrivingCycleData : SimulationComponentData
	{
		public enum CycleType
		{
			EngineOnly,
			TimeBased,
			DistanceBased
		}

		public List<DrivingCycleEntry> Entries { get; internal set; }

		public string Name { get; internal set; }

		public static DrivingCycleData ReadFromFileEngineOnly(string fileName)
		{
			return ReadFromFile(fileName, CycleType.EngineOnly);
		}

		public static DrivingCycleData ReadFromFileDistanceBased(string fileName)
		{
			return ReadFromFile(fileName, CycleType.DistanceBased);
		}

		public static DrivingCycleData ReadFromFileTimeBased(string fileName)
		{
			return ReadFromFile(fileName, CycleType.TimeBased);
		}

		public static DrivingCycleData ReadFromFile(string fileName, CycleType type)
		{
			var log = LogManager.GetLogger<DrivingCycleData>();

			var parser = CreateDataParser(type);
			var data = VectoCSVFile.Read(fileName);
			var entries = parser.Parse(data).ToList();

			log.Info(string.Format("Data loaded. Number of Entries: {0}", entries.Count));

			var cycle = new DrivingCycleData {
				Entries = entries,
				Name = Path.GetFileNameWithoutExtension(fileName)
			};
			return cycle;
		}

		private static IDataParser CreateDataParser(CycleType type)
		{
			switch (type) {
				case CycleType.EngineOnly:
					return new EngineOnlyDataParser();
				case CycleType.TimeBased:
					return new TimeBasedDataParser();
				case CycleType.DistanceBased:
					return new DistanceBasedDataParser();
				default:
					throw new ArgumentOutOfRangeException("type");
			}
		}

		private static class Fields
		{
			/// <summary>
			///     [m]	Travelled distance used for distance-based cycles. If t is also defined this column will be ignored.
			/// </summary>
			public const string Distance = "s";

			/// <summary>
			///     [s]	Used for time-based cycles. If neither this nor the distance s is defined the data will be interpreted as 1Hz.
			/// </summary>
			public const string Time = "t";

			/// <summary>
			///     [km/h]	Required except for Engine Only Mode calculations.
			/// </summary>
			public const string VehicleSpeed = "v";

			/// <summary>
			///     [%]	Optional.
			/// </summary>
			public const string RoadGradient = "grad";

			/// <summary>
			///     [s]	Required for distance-based cycles. Not used in time based cycles. stop defines the time the vehicle spends in
			///     stop phases.
			/// </summary>
			public const string StoppingTime = "stop";

			/// <summary>
			///     [kW]	"Aux_xxx" Supply Power input for each auxiliary defined in the .vecto file , where xxx matches the ID of the
			///     corresponding Auxiliary. ID's are not case sensitive and must not contain space or special characters.
			/// </summary>
			// todo: implement additional aux as dictionary!
			public const string AuxiliarySupplyPower = "Aux_";

			/// <summary>
			///     [rpm]	If n is defined VECTO uses that instead of the calculated engine speed value.
			/// </summary>
			public const string EngineSpeed = "n";

			/// <summary>
			///     [-]	Gear input. Overwrites the gear shift model.
			/// </summary>
			public const string Gear = "gear";

			/// <summary>
			///     [kW]	This power input will be directly added to the engine power in addition to possible other auxiliaries. Also
			///     used in Engine Only Mode.
			/// </summary>
			public const string AdditionalAuxPowerDemand = "Padd";

			/// <summary>
			///     [km/h]	Only required if Cross Wind Correction is set to Vair and Beta Input.
			/// </summary>
			public const string AirSpeedRelativeToVehicle = "vair_res";

			/// <summary>
			///     [°]	Only required if Cross Wind Correction is set to Vair and Beta Input.
			/// </summary>
			public const string WindYawAngle = "vair_beta";

			/// <summary>
			///     [kW]	Effective engine power at clutch. Only required in Engine Only Mode. Alternatively torque Me can be defined.
			///     Use DRAG to define motoring operation.
			/// </summary>
			public const string EnginePower = "Pe";

			/// <summary>
			///     [Nm]	Effective engine torque at clutch. Only required in Engine Only Mode. Alternatively power Pe can be defined.
			///     Use DRAG to define motoring operation.
			/// </summary>
			public const string EngineTorque = "Me";
		}

		public class DrivingCycleEntry
		{
			/// <summary>
			///     [m]	Travelled distance used for distance-based cycles. If "t"
			///     is also defined this column will be ignored.
			/// </summary>
			public double Distance { get; set; }

			/// <summary>
			///     [s]	Used for time-based cycles. If neither this nor the distance
			///     "s" is defined the data will be interpreted as 1Hz.
			/// </summary>
			public double Time { get; set; }

			/// <summary>
			///     [m/s]	Required except for Engine Only Mode calculations.
			/// </summary>
			public MeterPerSecond VehicleSpeed { get; set; }

			/// <summary>
			///     [%]	Optional.
			/// </summary>
			public double RoadGradient { get; set; }

			/// <summary>
			///     [s]	Required for distance-based cycles. Not used in time based
			///     cycles. "stop" defines the time the vehicle spends in stop phases.
			/// </summary>
			public double StoppingTime { get; set; }

			/// <summary>
			///     [W]	Supply Power input for each auxiliary defined in the
			///     .vecto file where xxx matches the ID of the corresponding
			///     Auxiliary. ID's are not case sensitive and must not contain
			///     space or special characters.
			/// </summary>
			public Dictionary<string, Watt> AuxiliarySupplyPower { get; set; }

			/// <summary>
			///     [rad/s]	If "n" is defined VECTO uses that instead of the
			///     calculated engine speed value.
			/// </summary>
			public PerSecond EngineSpeed { get; set; }

			/// <summary>
			///     [-]	Gear input. Overwrites the gear shift model.
			/// </summary>
			public double Gear { get; set; }

			/// <summary>
			///     [W]	This power input will be directly added to the engine
			///     power in addition to possible other auxiliaries. Also used in
			///     Engine Only Mode.
			/// </summary>
			public Watt AdditionalAuxPowerDemand { get; set; }

			/// <summary>
			///     [m/s] Only required if Cross Wind Correction is set to Vair and Beta Input.
			/// </summary>
			public MeterPerSecond AirSpeedRelativeToVehicle { get; set; }

			/// <summary>
			///     [°] Only required if Cross Wind Correction is set to Vair and Beta Input.
			/// </summary>
			public double WindYawAngle { get; set; }

			/// <summary>
			///     [Nm] Effective engine torque at clutch. Only required in
			///     Engine Only Mode. Alternatively power "Pe" can be defined.
			///     Use "DRAG" to define motoring operation.
			/// </summary>
			public NewtonMeter EngineTorque { get; set; }

			public bool Drag { get; set; }
		}

		#region DataParser

		private interface IDataParser
		{
			IEnumerable<DrivingCycleEntry> Parse(DataTable table);
		}

		/// <summary>
		///     Reader for Auxiliary Supply Power.
		/// </summary>
		private static class AuxSupplyPowerReader
		{
			/// <summary>
			///     [W]. Reads Auxiliary Supply Power (defined by Fields.AuxiliarySupplyPower-Prefix).
			/// </summary>
			public static Dictionary<string, Watt> Read(DataRow row)
			{
				return row.Table.Columns.Cast<DataColumn>().
					Where(col => col.ColumnName.StartsWith(Fields.AuxiliarySupplyPower)).
					ToDictionary(col => col.ColumnName.Substring(Fields.AuxiliarySupplyPower.Length - 1),
						col => row.ParseDouble(col).SI().Kilo.Watt.Cast<Watt>());
			}
		}

		internal class DistanceBasedDataParser : IDataParser
		{
			public IEnumerable<DrivingCycleEntry> Parse(DataTable table)
			{
				ValidateHeader(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());

				return table.Rows.Cast<DataRow>().Select(row => new DrivingCycleEntry {
					Distance = row.ParseDouble(Fields.Distance),
					VehicleSpeed = row.ParseDouble(Fields.VehicleSpeed).SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>(),
					RoadGradient = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
					AdditionalAuxPowerDemand =
						row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt.Cast<Watt>(),
					EngineSpeed =
						row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute.Cast<PerSecond>(),
					Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
					AirSpeedRelativeToVehicle =
						row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle)
							.SI()
							.Kilo.Meter.Per.Hour.Cast<MeterPerSecond>(),
					WindYawAngle = row.ParseDoubleOrGetDefault(Fields.WindYawAngle),
					AuxiliarySupplyPower = AuxSupplyPowerReader.Read(row)
				});
			}

			private static void ValidateHeader(string[] header)
			{
				var allowedCols = new[] {
					Fields.Distance,
					Fields.VehicleSpeed,
					Fields.RoadGradient,
					Fields.StoppingTime,
					Fields.EngineSpeed,
					Fields.Gear,
					Fields.AdditionalAuxPowerDemand,
					Fields.AirSpeedRelativeToVehicle,
					Fields.WindYawAngle
				};

				foreach (
					var col in
						header.Where(col => !(allowedCols.Contains(col) || col.StartsWith(Fields.AuxiliarySupplyPower)))
					) {
					throw new VectoException(string.Format("Column '{0}' is not allowed.", col));
				}

				if (!header.Contains(Fields.VehicleSpeed)) {
					throw new VectoException(string.Format("Column '{0}' is missing.", Fields.VehicleSpeed));
				}

				if (!header.Contains(Fields.Distance)) {
					throw new VectoException(string.Format("Column '{0}' is missing.", Fields.Distance));
				}

				if (header.Contains(Fields.AirSpeedRelativeToVehicle) ^ header.Contains(Fields.WindYawAngle)) {
					throw new VectoException(
						string.Format("Both Columns '{0}' and '{1}' must be defined, or none of them.",
							Fields.AirSpeedRelativeToVehicle, Fields.WindYawAngle));
				}
			}
		}

		private class TimeBasedDataParser : IDataParser
		{
			public IEnumerable<DrivingCycleEntry> Parse(DataTable table)
			{
				ValidateHeader(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());

				var entries = table.Rows.Cast<DataRow>().Select((row, index) => new DrivingCycleEntry {
					Time = row.ParseDoubleOrGetDefault(Fields.Time, index),
					VehicleSpeed = row.ParseDouble(Fields.VehicleSpeed).SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>(),
					RoadGradient = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
					AdditionalAuxPowerDemand =
						row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt.Cast<Watt>(),
					Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
					EngineSpeed =
						row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute.Cast<PerSecond>(),
					AirSpeedRelativeToVehicle =
						row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle)
							.SI()
							.Kilo.Meter.Per.Hour.Cast<MeterPerSecond>(),
					WindYawAngle = row.ParseDoubleOrGetDefault(Fields.WindYawAngle),
					AuxiliarySupplyPower = AuxSupplyPowerReader.Read(row)
				}).ToArray();

				return entries;
			}

			private static void ValidateHeader(string[] header)
			{
				var allowedCols = new[] {
					Fields.Time,
					Fields.VehicleSpeed,
					Fields.RoadGradient,
					Fields.EngineSpeed,
					Fields.Gear,
					Fields.AdditionalAuxPowerDemand,
					Fields.AirSpeedRelativeToVehicle,
					Fields.WindYawAngle
				};

				foreach (
					var col in
						header.Where(col => !(allowedCols.Contains(col) || col.StartsWith(Fields.AuxiliarySupplyPower)))
					) {
					throw new VectoException(string.Format("Column '{0}' is not allowed.", col));
				}

				if (!header.Contains(Fields.VehicleSpeed)) {
					throw new VectoException(string.Format("Column '{0}' is missing.", Fields.VehicleSpeed));
				}

				if (header.Contains(Fields.AirSpeedRelativeToVehicle) ^ header.Contains(Fields.WindYawAngle)) {
					throw new VectoException(
						string.Format("Both Columns '{0}' and '{1}' must be defined, or none of them.",
							Fields.AirSpeedRelativeToVehicle, Fields.WindYawAngle));
				}
			}
		}

		private class EngineOnlyDataParser : IDataParser
		{
			public IEnumerable<DrivingCycleEntry> Parse(DataTable table)
			{
				ValidateHeader(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());
				var absTime = new TimeSpan(0, 0, 0);

				foreach (DataRow row in table.Rows) {
					var entry = new DrivingCycleEntry {
						EngineSpeed =
							row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute.Cast<PerSecond>(),
						AdditionalAuxPowerDemand =
							row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt.Cast<Watt>(),
						AuxiliarySupplyPower = AuxSupplyPowerReader.Read(row)
					};
					if (row.Table.Columns.Contains(Fields.EngineTorque)) {
						if (row.Field<string>(Fields.EngineTorque).Equals("<DRAG>")) {
							entry.Drag = true;
						} else {
							entry.EngineTorque = row.ParseDouble(Fields.EngineTorque).SI<NewtonMeter>();
						}
					} else {
						if (row.Field<string>(Fields.EnginePower).Equals("<DRAG>")) {
							entry.Drag = true;
						} else {
							entry.EngineTorque =
								Formulas.PowerToTorque(row.ParseDouble(Fields.EnginePower).SI().Kilo.Watt.Cast<Watt>(),
									entry.EngineSpeed);
						}
					}
					entry.Time = absTime.TotalSeconds;
					absTime += new TimeSpan(0, 0, 1);

					yield return entry;
				}
			}

			private static void ValidateHeader(string[] header)
			{
				var allowedCols = new[] {
					Fields.EngineTorque,
					Fields.EnginePower,
					Fields.EngineSpeed,
					Fields.AdditionalAuxPowerDemand
				};

				foreach (var col in header.Where(col => !allowedCols.Contains(col))) {
					throw new VectoException(string.Format("Column '{0}' is not allowed.", col));
				}

				if (!header.Contains(Fields.EngineSpeed)) {
					throw new VectoException(string.Format("Column '{0}' is missing.", Fields.EngineSpeed));
				}

				if (!(header.Contains(Fields.EngineTorque) || header.Contains(Fields.EnginePower))) {
					throw new VectoException(
						string.Format("Columns missing: Either column '{0}' or column '{1}' must be defined.",
							Fields.EngineTorque, Fields.EnginePower));
				}

				if (header.Contains(Fields.EngineTorque) && header.Contains(Fields.EnginePower)) {
					LogManager.GetLogger<DrivingCycleData>()
						.WarnFormat("Found column '{0}' and column '{1}': only column '{0}' will be used.",
							Fields.EngineTorque, Fields.EnginePower);
				}
			}
		}

		#endregion
	}
}