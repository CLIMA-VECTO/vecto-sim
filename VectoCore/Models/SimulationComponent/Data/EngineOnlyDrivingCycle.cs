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
            public const string PowerEngine = "Pe";
            public const string PowerAuxilaries = "Padd";
            public const string Torque = "Me";
            public const string EngineSpeed = "n";
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
        public double PowerAuxilaries { get; set; }

        public static List<EngineOnlyDrivingCycle> ReadFromFile(string fileName)
        {
            var data = VectoCSVReader.Read(fileName);

            var cycles = new List<EngineOnlyDrivingCycle>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var cycle = new EngineOnlyDrivingCycle();
                cycle.EngineSpeed = row.GetDouble(Fields.EngineSpeed);

                if (data.Columns.Contains(Fields.PowerEngine))
                    cycle.PowerEngine = row.GetDouble(Fields.PowerEngine);
                else
                    cycle.Torque = row.GetDouble(Fields.Torque);

                if (data.Columns.Contains(Fields.PowerAuxilaries))
                    cycle.PowerAuxilaries = row.GetDouble(Fields.PowerAuxilaries);

                cycles.Add(cycle);
            }
            return cycles;
        }
    }
}
