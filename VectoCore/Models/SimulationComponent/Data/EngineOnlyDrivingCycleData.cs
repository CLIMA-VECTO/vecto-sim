using System.Collections.Generic;
using System.Data;
using Common.Logging;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
    /// <summary>
    /// Class for storing Data for one Driving Cycle in EngineOnly Mode.
    /// </summary>
    /// <remarks>
    /// The driving cycle (.vdri) must contain:
    /// <n> Engine speed
    /// <Me>|<Pe> Engine torque or engine power at clutch.
    /// 
    /// Optional:
    /// <Padd> Additional power demand (aux) 
    /// 
    /// To explicitly define motoring operation use the <DRAG> keyword - VECTO replaces the keyword with the motoring torque/power from the .vfld file during calculation.
    /// </remarks>
    public class EngineOnlyDrivingCycleData : SimulationComponentData
    {
        private static class Fields
        {
            public const string EngineSpeed = "n";
            public const string EnginePower = "Pe";
            public const string Torque = "Me";
            public const string AuxilariesPower = "Padd";
        }

        public List<EngineOnlyDrivingCycleEntry> Entries { get; set; }

        public class EngineOnlyDrivingCycleEntry
        {
            public double EngineSpeed { get; set; }

            public double Torque { get; set; }

            public double AuxilariesPower { get; set; }

            //todo: handle drag mode when drag is set true!
            public bool Drag { get; set; }
        }


        public static EngineOnlyDrivingCycleData ReadFromFile(string fileName)
        {
            var data = VectoCSVFile.Read(fileName);
            var Log = LogManager.GetLogger<EngineOnlyDrivingCycleData>();

            DataColumn engineSpeedCol = null, torqueCol = null, powerCol = null, auxCol = null;

            if (HeaderIsValid(data))
            {
                engineSpeedCol = data.Columns[Fields.EngineSpeed];
                torqueCol = data.Columns[Fields.Torque];
                powerCol = data.Columns[Fields.EnginePower];
                auxCol = data.Columns[Fields.AuxilariesPower];
            }
            else
            {
                Log.Warn("EngineOnlyDrivingCycleData Header is not valid. Falling back to column index.");
                engineSpeedCol = data.Columns[0];
                torqueCol = data.Columns[1];
                if (data.Columns.Count > 2)
                    auxCol = data.Columns[2];
            }

            if (auxCol != null)
                Log.Info("EngineOnlyDrivingCycleData Column for Auxiliary Power Consumption found.");


            var entries = new List<EngineOnlyDrivingCycleEntry>();
            foreach (DataRow row in data.Rows)
            {
                var entry = new EngineOnlyDrivingCycleEntry();

                entry.EngineSpeed = row.GetDouble(engineSpeedCol);

                if (torqueCol != null)
                {
                    if (row.Field<string>(torqueCol).Equals("<DRAG>"))
                        entry.Drag = true;
                    else
                        entry.Torque = row.GetDouble(torqueCol);
                }
                else
                {
                    if (row.Field<string>(powerCol).Equals("<DRAG>"))
                        entry.Drag = true;
                    else
                        entry.Torque = VectoMath.ConvertPowerToTorque(row.GetDouble(powerCol), entry.EngineSpeed);
                }

                if (auxCol != null)
                    entry.AuxilariesPower = row.GetDouble(auxCol);

                entries.Add(entry);
            }

            var cycle = new EngineOnlyDrivingCycleData { Entries = entries };

            Log.Info(string.Format("EngineOnlyDrivingCycle Data loaded. Number of Entries: {0}", entries.Count));

            return cycle;
        }

        private static bool HeaderIsValid(DataTable data)
        {
            return (!(data.Columns[Fields.EngineSpeed] != null
                      && (data.Columns[Fields.Torque] == null || data.Columns[Fields.EnginePower] == null)));
        }
    }
}