using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
    public class FullLoadCurve : SimulationComponentData
    {
        private static class Fields
        {
            /// <summary>
            /// [rpm] engine speed 
            /// </summary>
            public const string EngineSpeed = "engine speed";

            /// <summary>
            /// [Nm] full load torque 
            /// </summary>
            public const string TorqueFullLoad = "full load torque";

            /// <summary>
            /// [Nm] motoring torque 
            /// </summary>
            public const string TorqueDrag = "motoring torque";

            /// <summary>
            /// [s] time constant 
            /// </summary>
            public const string PT1 = "PT1";
        }

        private class FullLoadCurveEntry
        {
            /// <summary>
            /// [rad/s] engine speed 
            /// </summary>
            public double EngineSpeed { get; set; }

            /// <summary>
            /// [Nm] full load torque 
            /// </summary>
            public double TorqueFullLoad { get; set; }

            /// <summary>
            /// [Nm] motoring torque 
            /// </summary>
            public double TorqueDrag { get; set; }

            /// <summary>
            /// [s] PT1 time constant 
            /// </summary>
            public double PT1 { get; set; }

            #region Equality members
            protected bool Equals(FullLoadCurveEntry other)
            {
                return EngineSpeed.Equals(other.EngineSpeed)
                       && TorqueFullLoad.Equals(other.TorqueFullLoad)
                       && TorqueDrag.Equals(other.TorqueDrag)
                       && PT1.Equals(other.PT1);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((FullLoadCurveEntry)obj);
            }

            public override int GetHashCode()
            {
                var hashCode = EngineSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ TorqueFullLoad.GetHashCode();
                hashCode = (hashCode * 397) ^ TorqueDrag.GetHashCode();
                hashCode = (hashCode * 397) ^ PT1.GetHashCode();
                return hashCode;
            }

            #endregion
        }

        [JsonProperty]
        private List<FullLoadCurveEntry> _entries;

        public static FullLoadCurve ReadFromFile(string fileName)
        {
            var data = VectoCSVFile.Read(fileName);

            //todo Contract.Requires<VectoException>(data.Columns.Count != 4, "FullLoadCurve Data File must consist of 4 columns.");
            if (data.Columns.Count != 4)
                throw new VectoException("FullLoadCurve Data File must consist of 4 columns.");

            //todo Contract.Requires<VectoException>(data.Rows.Count < 2, "FullLoadCurve must consist of at least two lines with numeric values (below file header)");
            if (data.Rows.Count < 2)
                throw new VectoException("FullLoadCurve must consist of at least two lines with numeric values (below file header)");

            List<FullLoadCurveEntry> entries;
            if (HeaderIsValid(data.Columns))
            {
                entries = CreateFromColumnNames(data);
            }
            else
            {
                var log = LogManager.GetLogger<FullLoadCurve>();
                log.WarnFormat("FullLoadCurve: Header Line is not valid. Expected: '{0}, {1}, {2}, {3}', Got: '{4}'. Falling back to column index.",
                               Fields.EngineSpeed, Fields.TorqueFullLoad, Fields.TorqueDrag, Fields.PT1,
                               string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));

                entries = CreateFromColumnIndizes(data);
            }

            return new FullLoadCurve { _entries = entries };
        }

        private static bool HeaderIsValid(DataColumnCollection columns)
        {
            return columns.Contains(Fields.EngineSpeed)
                   && columns.Contains(Fields.TorqueDrag)
                   && columns.Contains(Fields.TorqueFullLoad)
                   && columns.Contains(Fields.PT1);
        }

        private static List<FullLoadCurveEntry> CreateFromColumnNames(DataTable data)
        {
            return (from DataRow row in data.Rows
                    select new FullLoadCurveEntry
                    {
                        EngineSpeed = row.ParseDouble(Fields.EngineSpeed).SI().Rounds.Per.Minute,
                        TorqueFullLoad = row.ParseDouble(Fields.TorqueFullLoad),
                        TorqueDrag = row.ParseDouble(Fields.TorqueDrag),
                        PT1 = row.ParseDouble(Fields.PT1)
                    }).ToList();
        }

        private static List<FullLoadCurveEntry> CreateFromColumnIndizes(DataTable data)
        {
            return (from DataRow row in data.Rows
                    select new FullLoadCurveEntry
                    {
                        EngineSpeed = row.ParseDouble(0).SI().Rounds.Per.Minute,
                        TorqueFullLoad = row.ParseDouble(1),
                        TorqueDrag = row.ParseDouble(2),
                        PT1 = row.ParseDouble(3)
                    }).ToList();
        }

        /// <summary>
        /// [rad/s] => [Nm]
        /// </summary>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>[Nm]</returns>
        public SI FullLoadStationaryTorque(SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Newton.Meter));

            var idx = FindIndex(angularFrequency);
            return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed, _entries[idx].EngineSpeed,
                                         _entries[idx - 1].TorqueFullLoad, _entries[idx].TorqueFullLoad,
                                         angularFrequency).SI().Newton.Meter;
        }

        /// <summary>
        /// [rad/s] => [W]
        /// </summary>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>[W]</returns>
        public SI FullLoadStationaryPower(SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

            return Formulas.TorqueToPower(FullLoadStationaryTorque(angularFrequency), angularFrequency);
        }

        /// <summary>
        /// [rad/s] => [Nm]
        /// </summary>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>[Nm]</returns>
        public SI DragLoadStationaryTorque(SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Newton.Meter));

            var idx = FindIndex(angularFrequency);
            return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed, _entries[idx].EngineSpeed,
                                         _entries[idx - 1].TorqueDrag, _entries[idx].TorqueDrag,
                                         angularFrequency).SI().Newton.Meter;
        }

        /// <summary>
        /// [rad/s] => [W].
        /// </summary>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>[W]</returns>
        public SI DragLoadStationaryPower(SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

            return Formulas.TorqueToPower(DragLoadStationaryTorque(angularFrequency), angularFrequency);
        }

        /// <summary>
        /// [rad/s] => [-]
        /// </summary>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>[-]</returns>
        public SI PT1(SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI()));

            var idx = FindIndex(angularFrequency);
            return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed, _entries[idx].EngineSpeed,
                                         _entries[idx - 1].PT1, _entries[idx].PT1,
                                         angularFrequency).SI();
        }

        /// <summary>
        /// [rad/s] => index. Get item index for engineSpeed.
        /// </summary>
        /// <param name="engineSpeed">[rad/s]</param>
        /// <returns>index</returns>
        protected int FindIndex(SI engineSpeed)
        {
            Contract.Requires(engineSpeed.HasEqualUnit(new SI().Radian.Per.Second));

            int idx;
            if (engineSpeed < _entries[0].EngineSpeed)
            {
                Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}",
                                engineSpeed.To().Rounds.Per.Minute, _entries[0].EngineSpeed.SI().To().Rounds.Per.Minute);
                idx = 1;
            }
            else
            {
                idx = _entries.FindIndex(x => x.EngineSpeed > engineSpeed);
            }
            if (idx <= 0)
            {
                idx = engineSpeed > _entries[0].EngineSpeed ? _entries.Count - 1 : 1;
            }
            return idx;
        }

        #region Equality members

        protected bool Equals(FullLoadCurve other)
        {
            return _entries.SequenceEqual(other._entries);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FullLoadCurve)obj);
        }

        public override int GetHashCode()
        {
            return (_entries != null ? _entries.GetHashCode() : 0);
        }

        #endregion
    }
}
