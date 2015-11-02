using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader
{
	public class DrivingCycleDataReader : LoggingObject
	{
		// --- Factory Methods
		public static DrivingCycleData ReadFromStream(Stream stream, CycleType type)
		{
			return DoReadCycleData(type, VectoCSVFile.ReadStream(stream));
		}

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
			var retVal = DoReadCycleData(type, VectoCSVFile.Read(fileName));
			retVal.Name = Path.GetFileNameWithoutExtension(fileName);
			return retVal;
		}

		private static DrivingCycleData DoReadCycleData(CycleType type, DataTable data)
		{
			var parser = CreateDataParser(type);
			var entries = parser.Parse(data).ToList();

			if (type == CycleType.DistanceBased) {
				entries = FilterDrivingCycleEntries(entries);
			}
			var cycle = new DrivingCycleData {
				Entries = entries,
				CycleType = type
			};
			return cycle;
		}


		// ----

		private static List<DrivingCycleData.DrivingCycleEntry> FilterDrivingCycleEntries(
			List<DrivingCycleData.DrivingCycleEntry> entries)
		{
			var filtered = new List<DrivingCycleData.DrivingCycleEntry>();
			var current = entries.First();
			current.Altitude = 0.SI<Meter>();
			filtered.Add(current);
			var distance = current.Distance;
			var altitude = current.Altitude;
			//foreach (var entry in entries) {
			for (var i = 0; i < entries.Count; i++) {
				var entry = entries[i];
				if (i > 0) {
					altitude += (entry.Distance - distance) * entries[i - 1].RoadGradientPercent / 100.0;
				}
				entry.Altitude = altitude;
				// if something changes in the cycle, add it to the filtered cycle but always add last entry
				if (!CycleEntriesAreEqual(current, entry) || i == entries.Count - 1) {
					entry.Altitude = altitude;
					filtered.Add(entry);
					current = entry;
				}
				if (!entry.StoppingTime.IsEqual(0) && entry.VehicleTargetSpeed.IsEqual(0)) {
					// vehicle stops. duplicate current distance entry with 0 waiting time
					var tmp = new DrivingCycleData.DrivingCycleEntry(entry) {
						StoppingTime = 0.SI<Second>(),
						VehicleTargetSpeed = i < entries.Count - 1 ? entries[i + 1].VehicleTargetSpeed : 0.SI<MeterPerSecond>()
					};
					filtered.Add(tmp);
					current = tmp;
				}

				distance = entry.Distance;
			}
			Logger<DrivingCycleDataReader>()
				.Info("Data loaded. Number of Entries: {0}, filtered Entries: {1}", entries.Count, filtered.Count);
			entries = filtered;

			AdjustDistanceAfterStop(entries);

			return entries;
		}

		private static void AdjustDistanceAfterStop(List<DrivingCycleData.DrivingCycleEntry> entries)
		{
			var currentIt = entries.GetEnumerator();
			var nextIt = entries.GetEnumerator();
			nextIt.MoveNext();
			while (currentIt.MoveNext() && nextIt.MoveNext()) {
				if (currentIt.Current != null && !currentIt.Current.StoppingTime.IsEqual(0)) {
					if (nextIt.Current != null) {
						nextIt.Current.Distance = currentIt.Current.Distance;
					}
				}
			}
		}

		// -----

		private static bool CycleEntriesAreEqual(DrivingCycleData.DrivingCycleEntry first,
			DrivingCycleData.DrivingCycleEntry second)
		{
			if (first.Distance.IsEqual(second.Distance)) {
				return true;
			}
			var retVal = first.VehicleTargetSpeed == second.VehicleTargetSpeed;
			retVal = retVal &&
					first.RoadGradient.IsEqual(second.RoadGradient, Constants.SimulationSettings.DrivingCycleRoadGradientTolerance);
			retVal = retVal && first.StoppingTime.IsEqual(0) && second.StoppingTime.IsEqual(0);
			retVal = retVal && first.AdditionalAuxPowerDemand == second.AdditionalAuxPowerDemand;
			retVal = retVal && first.AuxiliarySupplyPower.Count == second.AuxiliarySupplyPower.Count;

			foreach (var key in first.AuxiliarySupplyPower.Keys) {
				if (!second.AuxiliarySupplyPower.ContainsKey(key)) {
					return false;
				}
				retVal = retVal && first.AuxiliarySupplyPower[key] == second.AuxiliarySupplyPower[key];
			}

			return retVal;
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

		#region DataParser

		private interface IDataParser
		{
			IEnumerable<DrivingCycleData.DrivingCycleEntry> Parse(DataTable table);
		}

		internal class DistanceBasedDataParser : IDataParser
		{
			public IEnumerable<DrivingCycleData.DrivingCycleEntry> Parse(DataTable table)
			{
				ValidateHeader(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());

				return table.Rows.Cast<DataRow>().Select(row => new DrivingCycleData.DrivingCycleEntry {
					Distance = row.ParseDouble(Fields.Distance).SI<Meter>(),
					VehicleTargetSpeed =
						row.ParseDouble(Fields.VehicleSpeed).KMPHtoMeterPerSecond(),
					RoadGradientPercent = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
					RoadGradient =
						VectoMath.InclinationToAngle(row.ParseDoubleOrGetDefault(Fields.RoadGradient) / 100.0),
					StoppingTime =
						(row.ParseDouble(Fields.StoppingTime)).SI<Second>(),
					AdditionalAuxPowerDemand =
						row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt.Cast<Watt>(),
					EngineSpeed =
						row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute.Cast<PerSecond>(),
					Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
					AirSpeedRelativeToVehicle =
						row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle).KMPHtoMeterPerSecond(),
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
						header.Where(
							col => !(allowedCols.Contains(col) || col.StartsWith(Fields.AuxiliarySupplyPower)))
					) {
					throw new VectoException(string.Format("Column '{0}' is not allowed.", col));
				}

				if (!header.Contains(Fields.VehicleSpeed)) {
					throw new VectoException(string.Format("Column '{0}' is missing.",
						Fields.VehicleSpeed));
				}

				if (!header.Contains(Fields.Distance)) {
					throw new VectoException(string.Format("Column '{0}' is missing.", Fields.Distance));
				}

				if (header.Contains(Fields.AirSpeedRelativeToVehicle) ^
					header.Contains(Fields.WindYawAngle)) {
					throw new VectoException(
						string.Format("Both Columns '{0}' and '{1}' must be defined, or none of them.",
							Fields.AirSpeedRelativeToVehicle, Fields.WindYawAngle));
				}
			}
		}

		private class TimeBasedDataParser : IDataParser
		{
			public IEnumerable<DrivingCycleData.DrivingCycleEntry> Parse(DataTable table)
			{
				ValidateHeader(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());

				var entries = table.Rows.Cast<DataRow>().Select((row, index) => new DrivingCycleData.DrivingCycleEntry {
					Time = row.ParseDoubleOrGetDefault(Fields.Time, index).SI<Second>(),
					VehicleTargetSpeed =
						row.ParseDouble(Fields.VehicleSpeed).KMPHtoMeterPerSecond(),
					RoadGradientPercent = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
					RoadGradient =
						VectoMath.InclinationToAngle(row.ParseDoubleOrGetDefault(Fields.RoadGradient) / 100.0),
					AdditionalAuxPowerDemand =
						row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt.Cast<Watt>(),
					Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
					EngineSpeed =
						row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute.Cast<PerSecond>(),
					AirSpeedRelativeToVehicle =
						row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle).KMPHtoMeterPerSecond(),
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
						header.Where(
							col => !(allowedCols.Contains(col) || col.StartsWith(Fields.AuxiliarySupplyPower)))
					) {
					throw new VectoException(string.Format("Column '{0}' is not allowed.", col));
				}

				if (!header.Contains(Fields.VehicleSpeed)) {
					throw new VectoException(string.Format("Column '{0}' is missing.",
						Fields.VehicleSpeed));
				}

				if (header.Contains(Fields.AirSpeedRelativeToVehicle) ^
					header.Contains(Fields.WindYawAngle)) {
					throw new VectoException(
						string.Format("Both Columns '{0}' and '{1}' must be defined, or none of them.",
							Fields.AirSpeedRelativeToVehicle, Fields.WindYawAngle));
				}
			}
		}

		private class EngineOnlyDataParser : IDataParser
		{
			public IEnumerable<DrivingCycleData.DrivingCycleEntry> Parse(DataTable table)
			{
				ValidateHeader(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray());
				var absTime = 0.SI<Second>();

				foreach (DataRow row in table.Rows) {
					var entry = new DrivingCycleData.DrivingCycleEntry {
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
							entry.EngineTorque =
								row.ParseDouble(Fields.EngineTorque).SI<NewtonMeter>();
						}
					} else {
						if (row.Field<string>(Fields.EnginePower).Equals("<DRAG>")) {
							entry.Drag = true;
						} else {
							entry.EngineTorque = row.ParseDouble(Fields.EnginePower).SI().Kilo.Watt.Cast<Watt>() / entry.EngineSpeed;
						}
					}
					entry.Time = absTime;
					absTime += 1.SI<Second>();

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
					throw new VectoException(string.Format("Column '{0}' is missing.",
						Fields.EngineSpeed));
				}

				if (!(header.Contains(Fields.EngineTorque) || header.Contains(Fields.EnginePower))) {
					throw new VectoException(
						string.Format("Columns missing: Either column '{0}' or column '{1}' must be defined.",
							Fields.EngineTorque, Fields.EnginePower));
				}

				if (header.Contains(Fields.EngineTorque) && header.Contains(Fields.EnginePower)) {
					Logger<DrivingCycleDataReader>().Warn("Found column '{0}' and column '{1}': only column '{0}' will be used.",
						Fields.EngineTorque, Fields.EnginePower);
				}
			}
		}

		#endregion
	}
}