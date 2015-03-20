using System.Collections.Generic;
using System.Data;
using Common.Logging;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public class DrivingCycleData : SimulationComponentData
    {
        private static class Fields
        {
            /// <summary>
            /// [m]	Travelled distance used for distance-based cycles. If <t> is also defined this column will be ignored.
            /// </summary>
            public const string Distance = "s";

            /// <summary>
            /// [s]	Used for time-based cycles. If neither this nor the distance <s> is defined the data will be interpreted as 1Hz.
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
            ///  [s]	Required for distance-based cycles. Not used in time based cycles. <stop> defines the time the vehicle spends in stop phases.
            /// </summary>
            public const string StoppingTime = "stop";

            /// <summary>
            ///  [kW]	Supply Power input for each auxiliary defined in the .vecto file where xxx matches the ID of the corresponding Auxiliary. ID's are not case sensitive and must not contain space or special characters.
            /// </summary>
            // todo: implement additional aux as dictionary!
            public const string AuxiliarySupplyPower = "Aux_xxx";

            /// <summary>
            ///  [rpm]	If <n> is defined VECTO uses that instead of the calculated engine speed value.
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
            ///  [km/h]	Only required if Cross Wind Correction is set to Vair & Beta Input.
            /// </summary>
            public const string AirSpeedRelativeToVehicle = "vair_res";

            /// <summary>
            ///  [°]	Only required if Cross Wind Correction is set to Vair & Beta Input.
            /// </summary>
            public const string WindYawAngle = "vair_beta";

            /// <summary>
            ///  [kW]	Effective engine power at clutch. Only required in Engine Only Mode. Alternatively torque <Me> can be defined. Use <DRAG> to define motoring operation.
            /// </summary>
            public const string EnginePower = "Pe";

            /// <summary>
            ///  [Nm]	Effective engine torque at clutch. Only required in Engine Only Mode. Alternatively power <Pe> can be defined. Use <DRAG> to define motoring operation.
            /// </summary>
            public const string EngineTorque = "Me";
        }

        public List<DrivingCycleEntry> Entries { get; set; }

        public class DrivingCycleEntry
        {
            public double Distance { get; set; }
            public double Time { get; set; }
            public double VehicleSpeed { get; set; }
            public double RoadGradient { get; set; }
            public double StoppingTime { get; set; }
            public double AuxiliarySupplyPower { get; set; }
            public double EngineSpeed { get; set; }
            public double Gear { get; set; }
            // todo: implement additional aux as dictionary!
            public double AdditionalAuxPowerDemand { get; set; }
            public double AirSpeedRelativeToVehicle { get; set; }
            public double WindYawAngle { get; set; }
            public double EnginePower { get; set; }
            public double EngineTorque { get; set; }
        }

        public static DrivingCycleData ReadFromFile(string fileName)
        {
            var data = VectoCSVFile.Read(fileName);
            var Log = LogManager.GetLogger<DrivingCycleData>();

            var entries = new List<DrivingCycleEntry>();
            foreach (DataRow row in data.Rows)
            {
                var entry = new DrivingCycleEntry
                {
                    VehicleSpeed = row.GetDouble(Fields.VehicleSpeed),
                    RoadGradient = row.GetDouble(Fields.RoadGradient),

                    // todo: implement additional aux as dictionary!
                    //AuxiliarySupplyPower = row.GetDouble(Fields.AuxiliarySupplyPower),



                    AdditionalAuxPowerDemand = row.GetDouble(Fields.AdditionalAuxPowerDemand),

                    //EnginePower = row.GetDouble(Fields.EnginePower),
                    //EngineTorque = row.GetDouble(Fields.EngineTorque)
                };

                if (data.Columns.Contains(Fields.Gear))
                    entry.Gear = row.GetDouble(Fields.Gear);

                if (data.Columns.Contains(Fields.Distance))
                {
                    entry.Distance = row.GetDouble(Fields.Distance);
                    entry.StoppingTime = row.GetDouble(Fields.StoppingTime);
                }
                else
                    entry.Time = row.GetDouble(Fields.Time);

                if (data.Columns.Contains(Fields.EngineSpeed))
                    entry.EngineSpeed = row.GetDouble(Fields.EngineSpeed);

                if (data.Columns.Contains(Fields.AirSpeedRelativeToVehicle) ||
                    data.Columns.Contains(Fields.WindYawAngle))
                {
                    entry.AirSpeedRelativeToVehicle = row.GetDouble(Fields.AirSpeedRelativeToVehicle);
                    entry.WindYawAngle = row.GetDouble(Fields.WindYawAngle);
                }


                entries.Add(entry);
            }

            var cycle = new DrivingCycleData { Entries = entries };

            Log.Info(string.Format("Data loaded. Number of Entries: {0}", entries.Count));

            return cycle;
        }
    }
}