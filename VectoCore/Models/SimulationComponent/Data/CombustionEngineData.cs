using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
        private readonly Dictionary<uint, FullLoadCurve> _fullLoadCurves = new Dictionary<uint, FullLoadCurve>(); 

	    public CombustionEngineData(string fileName)
	    {
            //todo: file exception handling: file not readable, wrong file format
            using (StreamReader r = new StreamReader(fileName))
            {
                var json = r.ReadToEnd();
                var results = JsonConvert.DeserializeObject<dynamic>(json);
                var body = results.Body;

                ModelName = body.ModelName;
                Displacement = body.Displacement;
                IdleSpeed = body.IdlingSpeed;
                Inertia = body.Inertia;

                foreach (dynamic loadCurve in body.FullLoadCurves)
                {
                    string[] gears = loadCurve.Gears.ToString().Split('-');
                    var firstGear = uint.Parse(gears.First().Trim());
                    var lastGear = uint.Parse(gears.Last().Trim());

                    for (var i = firstGear; i <= lastGear; i++)
                    {
                        _fullLoadCurves[i] = new FullLoadCurve(loadCurve.Path.ToString());
                    }
                }

                ConsumptionMap = new FuelConsumptionMap(body.FuelMap.ToString());
            }
	    }

		/// <summary>
		/// Engine description (e.g., mode, type, etc.
		/// </summary>
		public String ModelName { get; protected set; }
		
		/// <summary>
		/// Engine displacement [ccm]
		/// </summary>
		public double Displacement { get; protected set; }

		public double IdleSpeed { get; protected set; }

		public double RatedSpeed { get; protected set; }

		public double Inertia { get; protected set; }

		public double MaxPower { get; set; }

		public FuelConsumptionMap ConsumptionMap { get; protected set; }

		public FullLoadCurve FullLoadCurve(uint gear)
		{
		    return _fullLoadCurves[gear];
		}
	}
}
