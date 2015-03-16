using System.Collections.Generic;
using System.Data;
using System.Linq;
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
	public class FullLoadCurve : SimulationComponentData
    {
        private static class Fields
        {
            public const string EngineSpeed = "n";
            public const string TorqueFullLoad = "Mfull";
            public const string TorqueDrag = "Mdrag";
            public const string PT1 = "PT1";
        }

        private class FullLoadCurveEntry
        {
            public double EngineSpeed { get; set; }
            public double TorqueFullLoad { get; set; }
            public double TorqueDrag { get; set; }
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
                if (obj.GetType() != this.GetType()) return false;
                return Equals((FullLoadCurveEntry) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = EngineSpeed.GetHashCode();
                    hashCode = (hashCode*397) ^ TorqueFullLoad.GetHashCode();
                    hashCode = (hashCode*397) ^ TorqueDrag.GetHashCode();
                    hashCode = (hashCode*397) ^ PT1.GetHashCode();
                    return hashCode;
                }
            }

            #endregion
        }

        private List<FullLoadCurveEntry> _entries;

        public static FullLoadCurve ReadFromFile(string fileName)
        {
            var fullLoadCurve = new FullLoadCurve();
            var data = VectoCSVFile.Read(fileName);
            fullLoadCurve._entries = new List<FullLoadCurveEntry>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var entry = new FullLoadCurveEntry
                {
                    EngineSpeed = row.GetDouble(Fields.EngineSpeed),
                    TorqueFullLoad = row.GetDouble(Fields.TorqueFullLoad),
                    TorqueDrag = row.GetDouble(Fields.TorqueDrag),
                    PT1 = row.GetDouble(Fields.PT1)
                };
                fullLoadCurve._entries.Add(entry);
            }
            return fullLoadCurve;
        }

	    public double FullLoadStationaryTorque(double rpm)
	    {
		    var idx = FindIndexForRpm(rpm);
		    return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed, _entries[idx].EngineSpeed,
			    _entries[idx - 1].TorqueFullLoad, _entries[idx].TorqueFullLoad, rpm);
	    }

	    public double FullLoadStationaryPower(double rpm)
	    {
		    return VectoMath.ConvertRpmToPower(rpm, FullLoadStationaryTorque(rpm));
	    }

	    public double DragLoadStationaryTorque(double rpm)
	    {
		    var idx = FindIndexForRpm(rpm);
			return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed, _entries[idx].EngineSpeed,
				_entries[idx - 1].TorqueDrag, _entries[idx].TorqueDrag, rpm);		    
	    }

	    public double DragLoadStationaryPower(double rpm)
	    {
		    return VectoMath.ConvertRpmToPower(rpm, DragLoadStationaryTorque(rpm));
	    }

	    public double PT1(double rpm)
	    {
		    var idx = FindIndexForRpm(rpm);
			return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed, _entries[idx].EngineSpeed,
				_entries[idx - 1].PT1, _entries[idx].PT1, rpm);
	    }

	    protected int FindIndexForRpm(double rpm)
	    {
			int idx;
			if (rpm < _entries[0].EngineSpeed) {
				Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}", rpm,
					_entries[0].EngineSpeed);
				idx = 1;
			} else {
				idx = _entries.FindIndex(x => x.EngineSpeed > rpm);
			}
			if (idx <= 0) {
				idx = rpm > _entries[0].EngineSpeed ? _entries.Count - 1 : 1;
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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FullLoadCurve)obj);
        }

        public override int GetHashCode()
        {
            return (_entries != null ? _entries.GetHashCode() : 0);
        }

        #endregion
    }
}
