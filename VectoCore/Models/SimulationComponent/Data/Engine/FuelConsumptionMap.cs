using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
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

        private IList<FuelConsumptionEntry> _entries = new List<FuelConsumptionEntry>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FuelConsumptionMap ReadFromFile(string fileName)
        {
            var fuelConsumptionMap = new FuelConsumptionMap();
            var data = VectoCSVReader.Read(fileName);

            try
            {
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        var entry = new FuelConsumptionEntry
                        {
                            EngineSpeed = row.GetDouble(Fields.EngineSpeed),
                            Torque = row.GetDouble(Fields.Torque),
                            FuelConsumption = row.GetDouble(Fields.FuelConsumption)
                        };
                        if (entry.FuelConsumption < 0)
                            throw new ArgumentOutOfRangeException("FuelConsumption < 0" + data.Rows.IndexOf(row));
                        fuelConsumptionMap._entries.Add(entry);
                    }
                    catch (Exception e)
                    {
                        throw new VectoException(string.Format("Line {0}: {1}", data.Rows.IndexOf(row), e.Message), e);
                    }
                }
            }
            catch (Exception e)
            {
                throw new VectoException(string.Format("File {0}: {1}", fileName, e.Message), e);
            }

            return fuelConsumptionMap;
        }
    }
}
