using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Common.Logging;
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

        private static class Fields
        {
            /// <summary>
            /// [m]	Travelled distance used for distance-based cycles. If t is also defined this column will be ignored.
            /// </summary>
            public const string Distance = "s";

            /// <summary>
            /// [s]	Used for time-based cycles. If neither this nor the distance s is defined the data will be interpreted as 1Hz.
            /// </summary>
            public const string Time = "t";

            /// <summary>
            /// [m/s]	Required except for Engine Only Mode calculations.
            /// </summary>
            public const string VehicleSpeed = "v";

            /// <summary>
            /// [%]	Optional.
            /// </summary>
            public const string RoadGradient = "grad";

            /// <summary>
            ///  [s]	Required for distance-based cycles. Not used in time based cycles. stop defines the time the vehicle spends in stop phases.
            /// </summary>
            public const string StoppingTime = "stop";

            /// <summary>
            ///  [W]	"Aux_xxx" Supply Power input for each auxiliary defined in the .vecto file , where xxx matches the ID of the corresponding Auxiliary. ID's are not case sensitive and must not contain space or special characters.
            /// </summary>
            // todo: implement additional aux as dictionary!
            public const string AuxiliarySupplyPower = "Aux_";

            /// <summary>
            ///  [rad/s]	If n is defined VECTO uses that instead of the calculated engine speed value.
            /// </summary>
            public const string EngineSpeed = "n";

            /// <summary>
            ///  [-]	Gear input. Overwrites the gear shift model.
            /// </summary>
            public const string Gear = "gear";

            /// <summary>
            ///  [W]	This power input will be directly added to the engine power in addition to possible other auxiliaries. Also used in Engine Only Mode.
            /// </summary>
            public const string AdditionalAuxPowerDemand = "Padd";

            /// <summary>
            ///  [m/s]	Only required if Cross Wind Correction is set to Vair and Beta Input.
            /// </summary>
            public const string AirSpeedRelativeToVehicle = "vair_res";

            /// <summary>
            ///  [°]	Only required if Cross Wind Correction is set to Vair and Beta Input.
            /// </summary>
            public const string WindYawAngle = "vair_beta";

            /// <summary>
            ///  [W]	Effective engine power at clutch. Only required in Engine Only Mode. Alternatively torque Me can be defined. Use DRAG to define motoring operation.
            /// </summary>
            public const string EnginePower = "Pe";

            /// <summary>
            ///  [Nm]	Effective engine torque at clutch. Only required in Engine Only Mode. Alternatively power Pe can be defined. Use DRAG to define motoring operation.
            /// </summary>
            public const string EngineTorque = "Me";
        }

        public List<DrivingCycleEntry> Entries { get; set; }

        public class DrivingCycleEntry
        {
            /// <summary>
            /// [m]	Travelled distance used for distance-based cycles. If <t> is also defined this column will be ignored.
            /// </summary>
            public double Distance { get; set; }

            /// <summary>
            /// [s]	Used for time-based cycles. If neither this nor the distance <s> is defined the data will be interpreted as 1Hz.
            /// </summary>
            public double Time { get; set; }

            /// <summary>
            /// [m/s]	Required except for Engine Only Mode calculations.
            /// </summary>
            public double VehicleSpeed { get; set; }

            /// <summary>
            /// [%]	Optional.
            /// </summary>
            public double RoadGradient { get; set; }

            /// <summary>
            ///  [s]	Required for distance-based cycles. Not used in time based cycles. <stop> defines the time the vehicle spends in stop phases.
            /// </summary>
            public double StoppingTime { get; set; }

            /// <summary>
            ///  [W]	Supply Power input for each auxiliary defined in the .vecto file where xxx matches the ID of the corresponding Auxiliary. ID's are not case sensitive and must not contain space or special characters.
            /// </summary>
            public Dictionary<string, double> AuxiliarySupplyPower { get; set; }

            /// <summary>
            ///  [rad/s]	If <n> is defined VECTO uses that instead of the calculated engine speed value.
            /// </summary>
            public double EngineSpeed { get; set; }

            /// <summary>
            ///  [-]	Gear input. Overwrites the gear shift model.
            /// </summary>
            public double Gear { get; set; }

            /// <summary>
            ///  [W]	This power input will be directly added to the engine power in addition to possible other auxiliaries. Also used in Engine Only Mode.
            /// </summary>
            public double AdditionalAuxPowerDemand { get; set; }

            /// <summary>
            ///  [m/s]	Only required if Cross Wind Correction is set to Vair & Beta Input.
            /// </summary>
            public double AirSpeedRelativeToVehicle { get; set; }

            /// <summary>
            ///  [°]	Only required if Cross Wind Correction is set to Vair & Beta Input.
            /// </summary>
            public double WindYawAngle { get; set; }

            /// <summary>
            ///  [Nm]	Effective engine torque at clutch. Only required in Engine Only Mode. Alternatively power <Pe> can be defined. Use <DRAG> to define motoring operation.
            /// </summary>
            public double EngineTorque { get; set; }

            public bool Drag { get; set; }

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
            var log = LogManager.GetLogger<DrivingCycleData>();

            var data = VectoCSVFile.Read(fileName);

            var header = data.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToList();

            var parser = CreateDataParser(type);
            parser.ValidateHeader(header);
            var entries = data.Rows.Cast<DataRow>().Select(parser.Parse).ToList();

            // update time field in 1Hz steps, if time field is missing and cycle type is timebase
            if (!data.Columns.Contains(Fields.Time) && type == CycleType.TimeBased)
            {
                for (var i = 0; i < entries.Count; i++)
                    entries[i].Time = i;
            }

            var cycle = new DrivingCycleData { Entries = entries };

            log.Info(string.Format("Data loaded. Number of Entries: {0}", entries.Count));

            return cycle;
        }

        #region private IDataParser
        private static IDataParser CreateDataParser(CycleType type)
        {
            switch (type)
            {
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
        private static Dictionary<string, double> ReadAuxSupplyPowerColumns(DataRow row)
        {
            var entries = new Dictionary<string, double>();
            var auxColumns = row.Table.Columns.Cast<DataColumn>().Where(c => c.ColumnName.StartsWith(Fields.AuxiliarySupplyPower));
            foreach (var c in auxColumns)
            {
                var auxName = c.ColumnName.Substring(Fields.AuxiliarySupplyPower.Length - 1);
                entries[auxName] = row.ParseDouble(c).SI().Kilo.Watt;
            }
            return entries;
        }
        private interface IDataParser
        {
            void ValidateHeader(ICollection<string> header);

            DrivingCycleEntry Parse(DataRow row);
        }
        private class DistanceBasedDataParser : IDataParser
        {
            public void ValidateHeader(ICollection<string> header)
            {
                var allowedCols = new[]
            { 
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

                foreach (var col in header.Where(col => !(allowedCols.Contains(col) || col.StartsWith(Fields.AuxiliarySupplyPower))))
                    throw new VectoException(string.Format("Column '{0}' is not allowed.", col));

                if (!header.Contains(Fields.VehicleSpeed))
                    throw new VectoException(string.Format("Column '{0}' is missing.", Fields.VehicleSpeed));

                if (!header.Contains(Fields.Distance))
                    throw new VectoException(string.Format("Column '{0}' is missing.", Fields.Distance));

                if (header.Contains(Fields.AirSpeedRelativeToVehicle) ^ header.Contains(Fields.WindYawAngle))
                    throw new VectoException(string.Format("Both Columns '{0}' and '{1}' must be defined, or none of them.",
                        Fields.AirSpeedRelativeToVehicle, Fields.WindYawAngle));
            }

            public DrivingCycleEntry Parse(DataRow row)
            {
                var entry = new DrivingCycleEntry
                {
                    Distance = row.ParseDouble(Fields.Distance),
                    VehicleSpeed = row.ParseDouble(Fields.VehicleSpeed).SI().Kilo.Meter.Per.Hour,
                    RoadGradient = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
                    AdditionalAuxPowerDemand = row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt,
                    EngineSpeed = row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute,
                    Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
                    AirSpeedRelativeToVehicle = row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle).SI().Kilo.Meter.Per.Hour,
                    WindYawAngle = row.ParseDoubleOrGetDefault(Fields.WindYawAngle),
                    AuxiliarySupplyPower = ReadAuxSupplyPowerColumns(row)
                };



                return entry;
            }
        }
        private class TimeBasedDataParser : IDataParser
        {
            public void ValidateHeader(ICollection<string> header)
            {
                var allowedCols = new[]
                { 
                    Fields.Time,
                    Fields.VehicleSpeed,
                    Fields.RoadGradient,
                    Fields.EngineSpeed,
                    Fields.Gear,
                    Fields.AdditionalAuxPowerDemand,
                    Fields.AirSpeedRelativeToVehicle,
                    Fields.WindYawAngle
                };

                foreach (var col in header.Where(col => !(allowedCols.Contains(col) || col.StartsWith(Fields.AuxiliarySupplyPower))))
                    throw new VectoException(string.Format("Column '{0}' is not allowed.", col));

                if (!header.Contains(Fields.VehicleSpeed))
                    throw new VectoException(string.Format("Column '{0}' is missing.", Fields.VehicleSpeed));

                if (header.Contains(Fields.AirSpeedRelativeToVehicle) ^ header.Contains(Fields.WindYawAngle))
                    throw new VectoException(string.Format("Both Columns '{0}' and '{1}' must be defined, or none of them.",
                        Fields.AirSpeedRelativeToVehicle, Fields.WindYawAngle));
            }

            public DrivingCycleEntry Parse(DataRow row)
            {
                var entry = new DrivingCycleEntry
                {
                    Time = row.ParseDoubleOrGetDefault(Fields.Time),
                    VehicleSpeed = row.ParseDouble(Fields.VehicleSpeed).SI().Kilo.Meter.Per.Hour,
                    RoadGradient = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
                    AdditionalAuxPowerDemand = row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt,
                    Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
                    EngineSpeed = row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute,
                    AirSpeedRelativeToVehicle = row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle).SI().Kilo.Meter.Per.Hour,
                    WindYawAngle = row.ParseDoubleOrGetDefault(Fields.WindYawAngle),
                    AuxiliarySupplyPower = ReadAuxSupplyPowerColumns(row)
                };

                return entry;
            }
        }
        private class EngineOnlyDataParser : IDataParser
        {
            public void ValidateHeader(ICollection<string> header)
            {
                var allowedCols = new[]
                {
                    Fields.EngineTorque,
                    Fields.EnginePower,
                    Fields.EngineSpeed,
                    Fields.AdditionalAuxPowerDemand
                };

                foreach (var col in header.Where(col => !allowedCols.Contains(col)))
                    throw new VectoException(string.Format("Column '{0}' is not allowed.", col));

                if (!header.Contains(Fields.EngineSpeed))
                    throw new VectoException(string.Format("Column '{0}' is missing.", Fields.EngineSpeed));

                if (!(header.Contains(Fields.EngineTorque) || header.Contains(Fields.EnginePower)))
                    throw new VectoException(string.Format("Columns missing: Either column '{0}' or column '{1}' must be defined.",
                        Fields.EngineTorque, Fields.EnginePower));

                if (header.Contains(Fields.EngineTorque) && header.Contains(Fields.EnginePower))
                    LogManager.GetLogger<DrivingCycleData>()
                        .WarnFormat("Found column '{0}' and column '{1}': only column '{0}' will be used.",
                            Fields.EngineTorque, Fields.EnginePower);
            }

            public DrivingCycleEntry Parse(DataRow row)
            {
                var entry = new DrivingCycleEntry
                {
                    EngineSpeed = row.ParseDoubleOrGetDefault(Fields.EngineSpeed).SI().Rounds.Per.Minute,
                    AdditionalAuxPowerDemand = row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand).SI().Kilo.Watt,
                    AuxiliarySupplyPower = ReadAuxSupplyPowerColumns(row)
                };

                if (row.Table.Columns.Contains(Fields.EngineTorque))
                {
                    if (row.Field<string>(Fields.EngineTorque).Equals("<DRAG>"))
                        entry.Drag = true;
                    else
                        entry.EngineTorque = row.ParseDouble(Fields.EngineTorque);
                }
                else
                {
                    if (row.Field<string>(Fields.EnginePower).Equals("<DRAG>"))
                        entry.Drag = true;
                    else
                        entry.EngineTorque = Formulas.PowerToTorque(row.ParseDouble(Fields.EnginePower).SI().Kilo.Watt,
                                                                    entry.EngineSpeed.SI().Radiant.Per.Second);
                }

                return entry;
            }
        }
        #endregion
    }
}