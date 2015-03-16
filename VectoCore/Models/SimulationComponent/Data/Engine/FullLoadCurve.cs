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
	public class FullLoadCurve : SimulationComponentData
    {
        private static class Fields
        {
            public const string EngineSpeed = "n";
            public const string TorqueFullLoad = "Mfull";
            public const string TorqueDrag = "Mdrag";
            public const string PT1 = "PT1";
        }

        protected bool Equals(FullLoadCurve other)
        {
            return Equals(entries, other.entries);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FullLoadCurve) obj);
        }

        public override int GetHashCode()
        {
            return (entries != null ? entries.GetHashCode() : 0);
        }

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
            var data = VectoCSVFile.Read(fileName);
            fullLoadCurve.entries = new List<FullLoadCurveEntry>();

            //todo: catch exceptions if value format is wrong.
            foreach (DataRow row in data.Rows)
            {
                var entry = new FullLoadCurveEntry();
                entry.EngineSpeed = row.GetDouble(Fields.EngineSpeed);
                entry.TorqueFullLoad = row.GetDouble(Fields.TorqueFullLoad);
                entry.TorqueDrag = row.GetDouble(Fields.TorqueDrag);
                entry.PT1 = row.GetDouble(Fields.PT1);
                fullLoadCurve.entries.Add(entry);
            }
            return fullLoadCurve;
        }

	    public double FullLoadStationaryTorque(double rpm)
	    {
		    var idx = FindIndexForRpm(rpm);
		    return VectoMath.Interpolate(entries[idx - 1].EngineSpeed, entries[idx].EngineSpeed,
			    entries[idx - 1].TorqueFullLoad, entries[idx].TorqueFullLoad, rpm);
	    }

	    public double FullLoadStationaryPower(double rpm)
	    {
		    return VectoMath.ConvertRpmToPower(rpm, FullLoadStationaryTorque(rpm));
	    }

	    public double DragLoadStationaryTorque(double rpm)
	    {
		    var idx = FindIndexForRpm(rpm);
			return VectoMath.Interpolate(entries[idx - 1].EngineSpeed, entries[idx].EngineSpeed,
				entries[idx - 1].TorqueDrag, entries[idx].TorqueDrag, rpm);		    
	    }

	    public double DragLoadStationaryPower(double rpm)
	    {
		    return VectoMath.ConvertRpmToPower(rpm, DragLoadStationaryTorque(rpm));
	    }

	    public double PT1(double rpm)
	    {
		    var idx = FindIndexForRpm(rpm);
			return VectoMath.Interpolate(entries[idx - 1].EngineSpeed, entries[idx].EngineSpeed,
				entries[idx - 1].PT1, entries[idx].PT1, rpm);
	    }

	    protected int FindIndexForRpm(double rpm)
	    {
			int idx;
			if (rpm < entries[0].EngineSpeed) {
				Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}", rpm,
					entries[0].EngineSpeed);
				idx = 1;
			} else {
				idx = entries.FindIndex(x => x.EngineSpeed > rpm);
			}
			if (idx <= 0) {
				idx = rpm > entries[0].EngineSpeed ? entries.Count - 1 : 1;
			}
		    return idx;
	    }
    }
}
