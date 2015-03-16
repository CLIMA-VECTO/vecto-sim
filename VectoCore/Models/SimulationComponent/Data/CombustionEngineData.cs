using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            }

            [JsonProperty(Required = Required.Always)]
            public DataBody Body;
        }

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

        private readonly Dictionary<string, FullLoadCurve> _fullLoadCurves = new Dictionary<string, FullLoadCurve>();

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
                combustionEngineData._fullLoadCurves[loadCurve.Gears] = fullLoadCurve;
            }

            return combustionEngineData;
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

        public void WriteToFile(string fileName)
        {
            //todo handle file exceptions
            File.WriteAllText(fileName, ToJson());
        }

        public FullLoadCurve GetFullLoadCurve(uint gear)
        {
            foreach (var gearRange in _fullLoadCurves.Keys)
            {
                var low = uint.Parse(gearRange.Split('-').First().Trim());
                if (low <= gear)
                {
                    var high = uint.Parse(gearRange.Split('-').Last().Trim());
                    if (high >= gear)
                        return _fullLoadCurves[gearRange];
                }
            }
            throw new KeyNotFoundException(string.Format("Gear '{0}' was not found in the FullLoadCurves.", gear));
        }
    }
}
