using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
    /// <summary>
    /// Three columns
    /// One header line 
    /// At least four lines with numeric values (below file header)
    /// The map must cover the full engine range between full load and motoring curve. Extrapolation is not possible! 
    /// Columns:
    /// * engine speed [1/min]
    /// * engine torque [Nm]
    /// * Fuel Consumption [g/h] 
    /// </summary>
    public class FuelConsumptionMap
    {
        private static class Fields
        {
            public const string EngineSpeed = "engine speed";
            public const string Torque = "torque";
            public const string FuelConsumption = "fuel consumption";
        };

        private class FuelConsumptionEntry
        {
            public double EngineSpeed { get; set; }
            public double Torque { get; set; }
            public double FuelConsumption { get; set; }
        }

        private IList<FuelConsumptionEntry> entries;

        public FuelConsumptionMap(string fileName)
        {
            var data = VectoCSVReader.Read(fileName);
            entries = new List<FuelConsumptionEntry>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var entry = new FuelConsumptionEntry();
                entry.EngineSpeed = row.GetDouble(Fields.EngineSpeed);
                entry.Torque = row.GetDouble(Fields.Torque);
                entry.FuelConsumption = row.GetDouble(Fields.FuelConsumption);
                entries.Add(entry);
            }
        }

        public static FuelConsumptionMap ReadFromFile(string fileName)
        {
            return new FuelConsumptionMap(fileName);
        }
    }
}
