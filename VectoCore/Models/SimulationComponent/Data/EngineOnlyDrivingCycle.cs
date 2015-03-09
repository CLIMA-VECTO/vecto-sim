﻿using System;
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
        private static class Fields
        {
            public const string Pe = "Pe";
            public const string Padd = "Padd";
            public const string Me = "Me";
            public const string n = "n";
        }

        /// <summary>
        /// Engine Speed
        /// </summary>
        public double EngineSpeed { get; set; }

        /// <summary>
        /// Torque
        /// </summary>
        /// <remarks>Column "Me" in data file.</remarks>
        public double Torque { get; set; }

        /// <summary>
        /// Engine power
        /// </summary>
        public double PowerEngine
        {
            get { return 2.0 * Math.PI / 60.0 * Torque * EngineSpeed; }
            set { Torque = 60.0 / (2.0 * Math.PI) * value / EngineSpeed; }
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
                cycle.EngineSpeed = row.GetDouble(Fields.n);

                if (data.Columns.Contains(Fields.Pe))
                    cycle.PowerEngine = row.GetDouble(Fields.Pe);
                else
                    cycle.Torque = row.GetDouble(Fields.Me);

                if (data.Columns.Contains(Fields.Padd))
                    cycle.Padd = row.GetDouble(Fields.Padd);

                cycles.Add(cycle);
            }
            return cycles;
        }
    }
}
