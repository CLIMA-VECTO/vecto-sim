using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
    [JsonObject(MemberSerialization.Fields)]
    public class FuelConsumptionMap : SimulationComponentData
    {
        private static class Fields
        {
            public const string EngineSpeed = "engine speed";
            public const string Torque = "torque";
            public const string FuelConsumption = "fuel consumption";
        };

        private class FuelConsumptionEntry
        {
            /// <summary>
            /// engine speed [rad/s]
            /// </summary>
            public double EngineSpeed { get; set; }

            /// <summary>
            /// Torque [Nm]
            /// </summary>
            public double Torque { get; set; }

            /// <summary>
            /// Fuel consumption [g/s]
            /// </summary>
            public double FuelConsumption { get; set; }

            #region Equality members
            protected bool Equals(FuelConsumptionEntry other)
            {
                return EngineSpeed.Equals(other.EngineSpeed) && Torque.Equals(other.Torque) &&
                       FuelConsumption.Equals(other.FuelConsumption);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((FuelConsumptionEntry)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = EngineSpeed.GetHashCode();
                    hashCode = (hashCode * 397) ^ Torque.GetHashCode();
                    hashCode = (hashCode * 397) ^ FuelConsumption.GetHashCode();
                    return hashCode;
                }
            }
            #endregion
        }

        private readonly IList<FuelConsumptionEntry> _entries = new List<FuelConsumptionEntry>();
        private readonly DelauneyMap _fuelMap = new DelauneyMap();

        private FuelConsumptionMap() { }

        public static FuelConsumptionMap ReadFromFile(string fileName)
        {
            var fuelConsumptionMap = new FuelConsumptionMap();
            var data = VectoCSVFile.Read(fileName);

            try
            {
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        var entry = new FuelConsumptionEntry
                        {
                            EngineSpeed = row.ParseDouble(Fields.EngineSpeed) / Units.RPMPerRadiant,
                            Torque = row.ParseDouble(Fields.Torque),
                            FuelConsumption = row.ParseDouble(Fields.FuelConsumption) * 1 / Units.SecondsPerHour
                        };

                        if (entry.FuelConsumption < 0)
                            throw new ArgumentOutOfRangeException("FuelConsumption < 0");

                        fuelConsumptionMap._entries.Add(entry);

                        // the delauney map works as expected, when the original engine speed field is used.
                        fuelConsumptionMap._fuelMap.AddPoint(entry.EngineSpeed * Units.RPMPerRadiant, entry.Torque, entry.FuelConsumption);
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

            fuelConsumptionMap._fuelMap.Triangulate();
            return fuelConsumptionMap;
        }

        /// <summary>
        /// Calculates the fuel consumption based on the given fuel map.
        /// </summary>
        /// <param name="engineSpeed">Engine speed (n) in [rad/sec].</param>
        /// <param name="torque">Torque (T) in [Nm].</param>
        /// <returns></returns>
        public double GetFuelConsumption(double engineSpeed, double torque)
        {
            return _fuelMap.Interpolate(engineSpeed * Units.RPMPerRadiant, torque);
        }

        #region Equality members

        protected bool Equals(FuelConsumptionMap other)
        {
            return _entries.SequenceEqual(other._entries)
                   && Equals(_fuelMap, other._fuelMap);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FuelConsumptionMap)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_entries != null ? _entries.GetHashCode() : 0) * 397) ^
                       (_fuelMap != null ? _fuelMap.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
