using System;
using System.Collections.Generic;
using System.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    /// <summary>
    /// Class for representation of one EngineOnly Driving Cycle
    /// </summary>
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
            get { return 2 * Math.PI / 60 * T * n; }
            set { T = 60 / (2 * Math.PI) * value / n; }
        }

        public static List<EngineOnlyDrivingCycle> Read(string fileName)
        {
            var data = VectoCSVReader.Read(fileName);

            var cycles = new List<EngineOnlyDrivingCycle>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var cycle = new EngineOnlyDrivingCycle();
                cycle.n = double.Parse(row.Field<string>("n"));

                if (data.Columns.Contains("Pe"))
                    cycle.Pe = double.Parse(row.Field<string>("Pe"));
                else
                    cycle.T = double.Parse(row.Field<string>("Me"));
                cycles.Add(cycle);
            }

            return cycles;
        }
    }
}
