using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    /// <summary>
    /// Represents the CombustionEngineData. Fileformat: .veng
    /// </summary>
    /// <code>
    /// {
    ///  "Header": {
    ///    "CreatedBy": " ()",
    ///    "Date": "3/4/2015 12:26:24 PM",
    ///    "AppVersion": "2.0.4-beta3",
    ///    "FileVersion": 2
    ///  },
    ///  "Body": {
    ///    "SavedInDeclMode": false,
    ///    "ModelName": "Generic 24t Coach",
    ///    "Displacement": 12730.0,
    ///    "IdlingSpeed": 560.0,
    ///    "Inertia": 3.8,
    ///    "FullLoadCurves": [
    ///      {
    ///        "Path": "24t Coach.vfld",
    ///        "Gears": "0 - 99"
    ///      }
    ///    ],
    ///    "FuelMap": "24t Coach.vmap",
    ///    "WHTC-Urban": 0.0,
    ///    "WHTC-Rural": 0.0,
    ///    "WHTC-Motorway": 0.0
    ///  }
    /// }
    /// </code>
    public class CombustionEngineData : SimulationComponentData
    {
        /// <summary>
        /// A class which represents the json data format for serializing and deserializing the EngineData files.
        /// </summary>
        public class Data
        {
            public class DataHeader
            {
                [JsonProperty(Required = Required.Always)]
                public string CreatedBy;

                [JsonProperty(Required = Required.Always)]
                public DateTime Date;

                [JsonProperty(Required = Required.Always)]
                public string AppVersion;

                [JsonProperty(Required = Required.Always)]
                public double FileVersion;

                #region Equality members

                protected bool Equals(DataHeader other)
                {
                    return string.Equals(CreatedBy, other.CreatedBy) && Date.Equals(other.Date) &&
                           string.Equals(AppVersion, other.AppVersion) && FileVersion.Equals(other.FileVersion);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != GetType()) return false;
                    return Equals((DataHeader)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = (CreatedBy != null ? CreatedBy.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ Date.GetHashCode();
                        hashCode = (hashCode * 397) ^ (AppVersion != null ? AppVersion.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ FileVersion.GetHashCode();
                        return hashCode;
                    }
                }

                #endregion
            }

            [JsonProperty(Required = Required.Always)]
            public DataHeader Header;

            public class DataBody
            {
                [JsonProperty("SavedInDeclMode")]
                public bool SavedInDeclarationMode;

                [JsonProperty(Required = Required.Always)]
                public string ModelName;

                [JsonProperty(Required = Required.Always)]
                public double Displacement;

                [JsonProperty("IdlingSpeed", Required = Required.Always)]
                public double IdleSpeed;

                [JsonProperty(Required = Required.Always)]
                public double Inertia;

                public class DataFullLoadCurve
                {
                    [JsonProperty(Required = Required.Always)]
                    public string Path;

                    [JsonProperty(Required = Required.Always)]
                    public string Gears;

                    #region Equality Members

                    protected bool Equals(DataFullLoadCurve other)
                    {
                        return string.Equals(Path, other.Path) && string.Equals(Gears, other.Gears);
                    }

                    public override bool Equals(object obj)
                    {
                        if (ReferenceEquals(null, obj)) return false;
                        if (ReferenceEquals(this, obj)) return true;
                        if (obj.GetType() != GetType()) return false;
                        return Equals((DataFullLoadCurve)obj);
                    }

                    public override int GetHashCode()
                    {
                        unchecked
                        {
                            return ((Path != null ? Path.GetHashCode() : 0) * 397) ^
                                   (Gears != null ? Gears.GetHashCode() : 0);
                        }
                    }

                    #endregion
                }

                [JsonProperty(Required = Required.Always)]
                public IList<DataFullLoadCurve> FullLoadCurves;

                [JsonProperty(Required = Required.Always)]
                public string FuelMap;

                [JsonProperty("WHTC-Urban")]
                public double WHTCUrban;

                [JsonProperty("WHTC-Rural")]
                public double WHTCRural;

                [JsonProperty("WHTC-Motorway")]
                public double WHTCMotorway;

                #region Equality members

                protected bool Equals(DataBody other)
                {
                    return SavedInDeclarationMode.Equals(other.SavedInDeclarationMode)
                           && string.Equals(ModelName, other.ModelName)
                           && Displacement.Equals(other.Displacement)
                           && IdleSpeed.Equals(other.IdleSpeed)
                           && Inertia.Equals(other.Inertia)
                           && FullLoadCurves.SequenceEqual(other.FullLoadCurves)
                           && string.Equals(FuelMap, other.FuelMap)
                           && WHTCUrban.Equals(other.WHTCUrban)
                           && WHTCRural.Equals(other.WHTCRural)
                           && WHTCMotorway.Equals(other.WHTCMotorway);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != GetType()) return false;
                    return Equals((DataBody)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        var hashCode = SavedInDeclarationMode.GetHashCode();
                        hashCode = (hashCode * 397) ^ (ModelName != null ? ModelName.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ Displacement.GetHashCode();
                        hashCode = (hashCode * 397) ^ IdleSpeed.GetHashCode();
                        hashCode = (hashCode * 397) ^ Inertia.GetHashCode();
                        hashCode = (hashCode * 397) ^ (FullLoadCurves != null ? FullLoadCurves.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (FuelMap != null ? FuelMap.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ WHTCUrban.GetHashCode();
                        hashCode = (hashCode * 397) ^ WHTCRural.GetHashCode();
                        hashCode = (hashCode * 397) ^ WHTCMotorway.GetHashCode();
                        return hashCode;
                    }
                }

                #endregion
            }

            [JsonProperty(Required = Required.Always)]
            public DataBody Body;

            #region Equality members

            protected bool Equals(Data other)
            {
                return Equals(Header, other.Header) && Equals(Body, other.Body);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Data)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Header != null ? Header.GetHashCode() : 0) * 397) ^ (Body != null ? Body.GetHashCode() : 0);
                }
            }

            #endregion
        }

        [JsonProperty]
        private Data _data;

        public bool SavedInDeclarationMode
        {
            get { return _data.Body.SavedInDeclarationMode; }
            protected set { _data.Body.SavedInDeclarationMode = value; }
        }

        public string ModelName
        {
            get { return _data.Body.ModelName; }
            protected set { _data.Body.ModelName = value; }
        }

        public double Displacement
        {
            get { return _data.Body.Displacement; }
            protected set { _data.Body.Displacement = value; }
        }

        public double IdleSpeed
        {
            get { return _data.Body.IdleSpeed; }
            protected set { _data.Body.IdleSpeed = value; }
        }

        public double Inertia
        {
            get { return _data.Body.Inertia; }
            protected set { _data.Body.Inertia = value; }
        }

        public double WHTCUrban
        {
            get { return _data.Body.WHTCUrban; }
            protected set { _data.Body.WHTCUrban = value; }
        }

        public double WHTCRural
        {
            get { return _data.Body.WHTCRural; }
            protected set { _data.Body.WHTCRural = value; }
        }

        public double WHTCMotorway
        {
            get { return _data.Body.WHTCMotorway; }
            protected set { _data.Body.WHTCMotorway = value; }
        }

        public FuelConsumptionMap ConsumptionMap { get; set; }



        public class RangeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value.GetType() == typeof(string) ? new Range((string)value) : base.ConvertFrom(context, culture, value);
            }
        }

        [TypeConverter(typeof(RangeConverter))]
        private class Range
        {
            private uint Start { get; set; }

            private uint End { get; set; }

            public Range(string range)
            {
                Start = uint.Parse(range.Split('-').First().Trim());
                End = uint.Parse(range.Split('-').Last().Trim());
            }

            public override string ToString()
            {
                return string.Format("{0} - {1}", Start, End);
            }

            public bool Contains(uint value)
            {
                return Start <= value && value <= End;
            }

            #region Equality members

            protected bool Equals(Range other)
            {
                return Start == other.Start && End == other.End;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Range)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (int)((Start * 397) ^ End);
                }
            }

            #endregion
        }

        [JsonProperty]
        private readonly Dictionary<Range, FullLoadCurve> _fullLoadCurves = new Dictionary<Range, FullLoadCurve>();

        public static CombustionEngineData ReadFromFile(string fileName)
        {
            //todo: file exception handling: file not readable
            return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName));
        }

        public static CombustionEngineData ReadFromJson(string json, string basePath = "")
        {
            var combustionEngineData = new CombustionEngineData();
            //todo handle conversion errors
            var d = JsonConvert.DeserializeObject<Data>(json);
            combustionEngineData._data = d;

            if (d.Header.FileVersion > 2)
                throw new UnsupportedFileVersionException("Unsupported Version of .veng file. Got Version: " + d.Header.FileVersion);

            combustionEngineData.ConsumptionMap = FuelConsumptionMap.ReadFromFile(Path.Combine(basePath, d.Body.FuelMap));

            foreach (var loadCurve in d.Body.FullLoadCurves)
            {
                var fullLoadCurve = FullLoadCurve.ReadFromFile(Path.Combine(basePath, loadCurve.Path));
                var gearRange = new Range(loadCurve.Gears);
                combustionEngineData._fullLoadCurves[gearRange] = fullLoadCurve;
            }

            return combustionEngineData;
        }

        public void WriteToFile(string fileName)
        {
            //todo handle file exceptions
            File.WriteAllText(fileName, ToJson());
        }

        public string ToJson()
        {
            _data.Header.Date = DateTime.Now;
            _data.Header.FileVersion = 2;
            _data.Header.AppVersion = "3.0.0"; // todo: get current app version!
            _data.Header.CreatedBy = ""; // todo: get current user
            _data.Body.SavedInDeclarationMode = false; //todo: get declaration mode setting
            return JsonConvert.SerializeObject(_data, Formatting.Indented);
        }

        public FullLoadCurve GetFullLoadCurve(uint gear)
        {
            var curve = _fullLoadCurves.FirstOrDefault(kv => kv.Key.Contains(gear));
            if (curve.Key.Equals(null))
                throw new KeyNotFoundException(string.Format("Gear '{0}' was not found in the FullLoadCurves.", gear));

            return curve.Value;
        }

        #region Equality members

        protected bool Equals(CombustionEngineData other)
        {
            return Equals(_data, other._data)
                   && _fullLoadCurves.Keys.SequenceEqual(other._fullLoadCurves.Keys)
                   && _fullLoadCurves.Values.SequenceEqual(other._fullLoadCurves.Values)
                   && Equals(ConsumptionMap, other.ConsumptionMap);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CombustionEngineData)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _data.GetHashCode();
                hashCode = (hashCode * 397) ^ (_fullLoadCurves != null ? _fullLoadCurves.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ConsumptionMap != null ? ConsumptionMap.GetHashCode() : 0);
                return hashCode;
            }
        }
        #endregion
    }
}
