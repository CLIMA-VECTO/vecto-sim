using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
    /// <summary>
    /// Four columns
    /// One header line 
    /// At least two lines with numeric values (below file header)
    /// Columns:
    /// * n       engine speed [1/min]
    /// * Mfull   full load torque [Nm]
    /// * Mdrag   motoring torque [Nm]
    /// * PT1     PT1 time constant [s] 

    /// </summary>
    public class FullLoadCurve
    {
        private class FullLoadCurveEntry
        {
            public double EngineSpeed { get; set; }
            public double TorqueFullLoad { get; set; }
            public double TorqueDrag { get; set; }
            public double PT1 { get; set; }
        }

        private List<FullLoadCurveEntry> entries;

        public static FullLoadCurve ReadFromFile(string fileName)
        {
            var fullLoadCurve = new FullLoadCurve();
            var data = VectoCSVReader.Read(fileName);
            fullLoadCurve.entries = new List<FullLoadCurveEntry>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var entry = new FullLoadCurveEntry();
                entry.EngineSpeed = row.GetDouble("n");
                entry.TorqueFullLoad = row.GetDouble("Mfull");
                entry.TorqueDrag = row.GetDouble("Mdrag");
                entry.PT1 = row.GetDouble("PT1");
                fullLoadCurve.entries.Add(entry);
            }
            return fullLoadCurve;
        }
    }
}
