using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
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
        private readonly Dictionary<string, FullLoadCurve> _fullLoadCurves = new Dictionary<string, FullLoadCurve>();

        public static CombustionEngineData ReadFromFile(string fileName)
        {
            //todo: file exception handling: file not readable, wrong file format
            return ReadFromJson(File.ReadAllText(fileName));
        }

        public static CombustionEngineData ReadFromJson(string json)
        {
            CombustionEngineData engine = new CombustionEngineData();
            var results = JsonConvert.DeserializeObject<dynamic>(json);

            //todo: handle error when fields not exist
	        if (results["Header"] == null) {
		        throw new InvalidFileFormatException("could not find 'Header' Section");
	        }
            var header = results["Header"];

            if (header["FileVersion"] > 2)
                throw new UnsupportedFileVersionException("Unsupported Version of .veng file. Got Version: " + header["FileVersion"]);

            var body = results["Body"];

            if (header["FileVersion"] > 1)
                engine.SavedInDeclarationMode = body["SavedInDeclMode"];

            engine.ModelName = body["ModelName"];
            engine.Displacement = body["Displacement"];
            engine.IdleSpeed = body["IdlingSpeed"];
            engine.Inertia = body["Inertia"];

			// engine.GetType().GetProperty("Inertia").SetValue(engine, body["Inerita"]);

            foreach (dynamic loadCurve in body["FullLoadCurves"])
                engine._fullLoadCurves[loadCurve["Gears"].Value] = FullLoadCurve.ReadFromFile(loadCurve["Path"].Value);

            engine.ConsumptionMap = FuelConsumptionMap.ReadFromFile(body["FuelMap"].Value);

            if (body["WHTC-Urban"] != null)
                engine.WHTCUrban = body["WHTC-Urban"].Value;

            if (body["WHTC-Rural"] != null)
				engine.WHTCRural = body["WHTC-Rural"].Value;

            if (body["WHTC-Motorway"] != null)
				engine.WHTCMotorway = body["WHTC-Motorway"].Value;

            return engine;
        }

        public double WHTCMotorway { get; set; }

        public double WHTCRural { get; set; }

        public double WHTCUrban { get; set; }

        public bool SavedInDeclarationMode { get; set; }

        /// <summary>
        /// Engine description (e.g., mode, type, etc.
        /// </summary>
        public String ModelName { get; set; }

        /// <summary>
        /// Engine displacement [ccm]
        /// </summary>
        public double Displacement { get; set; }

        public double IdleSpeed { get; set; }

        public double RatedSpeed { get; set; }

        public double Inertia { get; set; }

        public double MaxPower { get; set; }

        public FuelConsumptionMap ConsumptionMap { get; set; }

        public FullLoadCurve GetFullLoadCurve(uint gear)
        {
            foreach (var gear_range in _fullLoadCurves.Keys)
            {
                var low = uint.Parse(gear_range.Split('-').First().Trim());
                if (low <= gear)
                {
                    var high = uint.Parse(gear_range.Split('-').Last().Trim());
                    if (high >= gear)
                        return _fullLoadCurves[gear_range];
                }
            }
            throw new KeyNotFoundException(string.Format("Gear '{0}' was not found in the FullLoadCurves.", gear));
        }
    }
}
