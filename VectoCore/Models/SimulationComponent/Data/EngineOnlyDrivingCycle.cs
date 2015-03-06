using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    /// <summary>
    /// Class for representation of one EngineOnly Driving Cycle
    /// </summary>
    /// <remarks>
    /// The driving cylce (.vdri) must contain:
    /// <n> Engine speed
    /// <Me>|<Pe> Engine torque or engine power at clutch.
    /// 
    /// Optional:
    /// <Padd> Additional power demand (aux) 
    /// 
    /// To explicitly define motoring operation use the <DRAG> keyword, see below. 
    /// VECTO replaces the keyword with the motoring torque/power from the .vfld file during calculation.
    /// </remarks>
    public class EngineOnlyDrivingCycle
    {
        /// <summary>
        /// Engine Speed
        /// </summary>
        public double n { get; set; }

        /// <summary>
        /// Torque
        /// </summary>
        /// <remarks>Column "Me" in data file.</remarks>
        public double T { get; set; }

        /// <summary>
        /// Engine power
        /// </summary>
        public double Pe
        {
            get { return 2.0 * Math.PI / 60.0 * T * n; }
            set { T = 60.0 / (2.0 * Math.PI) * value / n; }
        }

        /// <summary>
        /// Additional power demand (aux) (Optional).
        /// </summary>
        public double Padd { get; set; }

        public static List<EngineOnlyDrivingCycle> ReadFromFile(string fileName)
        {
            var data = VectoCSVReader.Read(fileName);

            var cycles = new List<EngineOnlyDrivingCycle>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var cycle = new EngineOnlyDrivingCycle();
                cycle.n = row.GetDouble("n");

                if (data.Columns.Contains("Pe"))
                    cycle.Pe = row.GetDouble("Pe");
                else
                    cycle.T = row.GetDouble("Me");

                if (data.Columns.Contains("Padd"))
                    cycle.Padd = row.GetDouble("Padd");

                cycles.Add(cycle);
            }
            return cycles;
        }
    }
}
