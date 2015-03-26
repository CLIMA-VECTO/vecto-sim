using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public enum CycleType
    {
        EngineOnly,
        TimeBased,
        DistanceBased
    }

    public class DrivingCycleData : SimulationComponentData
    {
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
            /// [km/h]	Required except for Engine Only Mode calculations.
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
            ///  [kW]	"Aux_xxx" Supply Power input for each auxiliary defined in the .vecto file , where xxx matches the ID of the corresponding Auxiliary. ID's are not case sensitive and must not contain space or special characters.
            /// </summary>
            // todo: implement additional aux as dictionary!
            public const string AuxiliarySupplyPower = "Aux_";

            /// <summary>
            ///  [rpm]	If n is defined VECTO uses that instead of the calculated engine speed value.
            /// </summary>
            public const string EngineSpeed = "n";

            /// <summary>
            ///  [-]	Gear input. Overwrites the gear shift model.
            /// </summary>
            public const string Gear = "gear";

            /// <summary>
            ///  [kW]	This power input will be directly added to the engine power in addition to possible other auxiliaries. Also used in Engine Only Mode.
            /// </summary>
            public const string AdditionalAuxPowerDemand = "Padd";

            /// <summary>
            ///  [km/h]	Only required if Cross Wind Correction is set to Vair and Beta Input.
            /// </summary>
            public const string AirSpeedRelativeToVehicle = "vair_res";

            /// <summary>
            ///  [∞]	Only required if Cross Wind Correction is set to Vair and Beta Input.
            /// </summary>
            public const string WindYawAngle = "vair_beta";

            /// <summary>
            ///  [kW]	Effective engine power at clutch. Only required in Engine Only Mode. Alternatively torque Me can be defined. Use DRAG to define motoring operation.
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
            /// [km/h]	Required except for Engine Only Mode calculations.
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
            ///  [kW]	Supply Power input for each auxiliary defined in the .vecto file where xxx matches the ID of the corresponding Auxiliary. ID's are not case sensitive and must not contain space or special characters.
            /// </summary>
            public Dictionary<string, double> AuxiliarySupplyPower { get; set; }

            /// <summary>
            ///  [rpm]	If <n> is defined VECTO uses that instead of the calculated engine speed value.
            /// </summary>
            public double EngineSpeed { get; set; }

            /// <summary>
            ///  [-]	Gear input. Overwrites the gear shift model.
            /// </summary>
            public double Gear { get; set; }

            /// <summary>
            ///  [kW]	This power input will be directly added to the engine power in addition to possible other auxiliaries. Also used in Engine Only Mode.
            /// </summary>
            public double AdditionalAuxPowerDemand { get; set; }

            /// <summary>
            ///  [km/h]	Only required if Cross Wind Correction is set to Vair & Beta Input.
            /// </summary>
            public double AirSpeedRelativeToVehicle { get; set; }

            /// <summary>
            ///  [°]	Only required if Cross Wind Correction is set to Vair & Beta Input.
            /// </summary>
            public double WindYawAngle { get; set; }

            /// <summary>
            ///  [kW]	Effective engine power at clutch. Only required in Engine Only Mode. Alternatively torque <Me> can be defined. Use <DRAG> to define motoring operation.
            /// </summary>
            public double EnginePower { get; set; }

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
            var Log = LogManager.GetLogger<DrivingCycleData>();

            var data = VectoCSVFile.Read(fileName);

            var header = data.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToList();

            var validator = GetHeaderValidator(type);
            if (!validator(header))
                throw new VectoException("Header is not valid.");

            var reader = GetDataRowReader(type);
            var entries = data.Rows.Cast<DataRow>().Select(r => reader(r)).ToList();

            // update time field in 1Hz steps, if time field is missing and cycle type is timebase
            if (!data.Columns.Contains(Fields.Time) && type == CycleType.TimeBased)
            {
                for (var i = 0; i < entries.Count; i++)
                    entries[i].Time = i;
            }

            var cycle = new DrivingCycleData { Entries = entries };

            Log.Info(string.Format("Data loaded. Number of Entries: {0}", entries.Count));

            return cycle;
        }

        private delegate bool HeaderValidator(ICollection<string> header);
        private delegate DrivingCycleEntry DataRowReader(DataRow row);

        private static HeaderValidator GetHeaderValidator(CycleType type)
        {
            switch (type)
            {
                case CycleType.EngineOnly:
                    return HeaderIsValidEngineOnly;
                case CycleType.TimeBased:
                    return HeaderIsValidTimeBased;
                case CycleType.DistanceBased:
                    return HeaderIsValidDistanceBased;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private static DataRowReader GetDataRowReader(CycleType type)
        {
            switch (type)
            {
                case CycleType.EngineOnly:
                    return ParseEngineOnly;
                case CycleType.TimeBased:
                    return ParseTimeBased;
                case CycleType.DistanceBased:
                    return ParseDistanceBased;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private static bool HeaderIsValidDistanceBased(ICollection<string> header)
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


            var allColumnsAllowed = header.All(h => allowedCols.Contains(h) || h.StartsWith(Fields.AuxiliarySupplyPower));
            var requiredColumnsExist = header.Contains(Fields.Distance)
                                       && header.Contains(Fields.VehicleSpeed)
                                       && !(header.Contains(Fields.AirSpeedRelativeToVehicle) ^ header.Contains(Fields.WindYawAngle));

            return allColumnsAllowed && requiredColumnsExist;
        }

        private static DrivingCycleEntry ParseDistanceBased(DataRow row)
        {
            var entry = new DrivingCycleEntry
            {
                Distance = row.ParseDouble(Fields.Distance),
                VehicleSpeed = row.ParseDouble(Fields.VehicleSpeed),
                RoadGradient = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
                AdditionalAuxPowerDemand = row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand),
                EngineSpeed = row.ParseDoubleOrGetDefault(Fields.EngineSpeed),
                Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
                AirSpeedRelativeToVehicle = row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle),
                WindYawAngle = row.ParseDoubleOrGetDefault(Fields.WindYawAngle),
                AuxiliarySupplyPower = ReadAuxSupplyPowerColumns(row)
            };



            return entry;
        }

        private static Dictionary<string, double> ReadAuxSupplyPowerColumns(DataRow row)
        {
            var entries = new Dictionary<string, double>();
            foreach (DataColumn c in row.Table.Columns)
            {
                if (c.ColumnName.StartsWith(Fields.AuxiliarySupplyPower))
                {
                    var auxName = c.ColumnName.Substring(Fields.AuxiliarySupplyPower.Length - 1);
                    entries[auxName] = row.ParseDouble(c);
                }
            }
            return entries;
        }

        private static bool HeaderIsValidTimeBased(ICollection<string> header)
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

            var allColumnsAllowed = header.All(h => allowedCols.Contains(h) || h.StartsWith(Fields.AuxiliarySupplyPower));
            var requiredColumnsExist = header.Contains(Fields.VehicleSpeed)
                && !(header.Contains(Fields.AirSpeedRelativeToVehicle) ^ header.Contains(Fields.WindYawAngle));

            return allColumnsAllowed && requiredColumnsExist;
        }

        private static DrivingCycleEntry ParseTimeBased(DataRow row)
        {
            var entry = new DrivingCycleEntry
            {
                Time = row.ParseDoubleOrGetDefault(Fields.Time),
                VehicleSpeed = row.ParseDouble(Fields.VehicleSpeed),
                RoadGradient = row.ParseDoubleOrGetDefault(Fields.RoadGradient),
                AdditionalAuxPowerDemand = row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand),
                Gear = row.ParseDoubleOrGetDefault(Fields.Gear),
                EngineSpeed = row.ParseDoubleOrGetDefault(Fields.EngineSpeed),
                AirSpeedRelativeToVehicle = row.ParseDoubleOrGetDefault(Fields.AirSpeedRelativeToVehicle),
                WindYawAngle = row.ParseDoubleOrGetDefault(Fields.WindYawAngle),
                AuxiliarySupplyPower = ReadAuxSupplyPowerColumns(row)
            };

            return entry;
        }

        private static bool HeaderIsValidEngineOnly(ICollection<string> header)
        {
            var allowedCols = new[]
            { 
                Fields.EngineTorque, 
                Fields.EnginePower, 
                Fields.EngineSpeed, 
                Fields.AdditionalAuxPowerDemand 
            };

            var allColumnsAllowed = header.All(h => allowedCols.Contains(h));

            var requiredColumnsExist = header.Contains(Fields.EngineSpeed)
                                       && (header.Contains(Fields.EngineTorque) || header.Contains(Fields.EnginePower));


            return allColumnsAllowed && requiredColumnsExist;
        }

        private static DrivingCycleEntry ParseEngineOnly(DataRow row)
        {
            var entry = new DrivingCycleEntry();

            entry.EngineSpeed = row.ParseDouble(Fields.EngineSpeed);

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
                    entry.EngineTorque = VectoMath.ConvertPowerRpmToTorque(row.ParseDouble(Fields.EnginePower), entry.EngineSpeed);
            }

            entry.AdditionalAuxPowerDemand = row.ParseDoubleOrGetDefault(Fields.AdditionalAuxPowerDemand);

            entry.AuxiliarySupplyPower = ReadAuxSupplyPowerColumns(row);

            return entry;
        }
    }
}