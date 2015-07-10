using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.FileIO.EngineeringFile
{
	/// <summary>
	///     Represents the Vecto Job File. Fileformat: .vecto
	/// </summary>
	/// <code>
	///{
	///  "Header": {
	///    "CreatedBy": " ()",
	///    "Date": "3/4/2015 2:09:13 PM",
	///    "AppVersion": "2.0.4-beta3",
	///    "FileVersion": 2
	///  },
	///  "Body": {
	///    "SavedInDeclMode": false,
	///    "VehicleData": "24t Coach.vveh",
	///    "EngineData": "24t Coach.veng",
	///    "GearboxData": "24t Coach.vgbx",
	///    "Cycles": [
	///      "W:\\VECTO\\CITnet\\VECTO\\bin\\Debug\\Declaration\\MissionCycles\\LOT2_rural Engine Only.vdri"
	///    ],
	///    "Aux": [
	///      {
	///        "ID": "ALT1",
	///        "Type": "Alternator",
	///        "Path": "24t_Coach_ALT.vaux",
	///        "Technology": ""
	///      },
	///      {
	///        "ID": "ALT2",
	///        "Type": "Alternator",
	///        "Path": "24t_Coach_ALT.vaux",
	///        "Technology": ""
	///      },
	///      {
	///        "ID": "ALT3",
	///        "Type": "Alternator",
	///        "Path": "24t_Coach_ALT.vaux",
	///        "Technology": ""
	///      }
	///    ],
	///    "AccelerationLimitingFile": "Coach.vacc",
	///    "IsEngineOnly": true,
	///    "StartStop": {
	///      "Enabled": false,
	///      "MaxSpeed": 5.0,
	///      "MinTime": 0.0,
	///      "Delay": 0
	///    },
	///    "LookAheadCoasting": {
	///      "Enabled": true,
	///      "Dec": -0.5,
	///      "MinSpeed": 50.0
	///    },
	///    "OverSpeedEcoRoll": {
	///      "Mode": "OverSpeed",
	///      "MinSpeed": 70.0,
	///      "OverSpeed": 5.0,
	///      "UnderSpeed": 5.0
	///    }
	///  }
	///}
	/// </code>
	public class VectoJobFileV2Engineering : VectoJobFileV2Declaration
	{
		[JsonProperty(Required = Required.Always)] public new DataBodyEng Body;


		public class DataBodyEng : VectoJobFileV2Declaration.DataBodyDecl
		{
			[JsonProperty(Required = Required.Always)] public IList<string> Cycles;
			[JsonProperty] public new IList<AuxDataEng> Aux = new List<AuxDataEng>();
			[JsonProperty(Required = Required.Always)] public new StartStopDataDeclEng StartStop;
			[JsonProperty(Required = Required.Always)] public LACDataEng LAC;
			[JsonProperty(Required = Required.Always)] public new OverSpeedEcoRollDataEng OverSpeedEcoRoll;

			public class AuxDataEng : DataBodyDecl.AuxDataDecl
			{
				[JsonProperty(Required = Required.Always)] public string ID;
				[JsonProperty(Required = Required.Always)] public string Type;
				[JsonProperty(Required = Required.Always)] public string Path;
			}

			public class StartStopDataDeclEng : StartStopDataDecl
			{
				[JsonProperty(Required = Required.Always)] public double MaxSpeed;
				[JsonProperty(Required = Required.Always)] public double MinTime;
				[JsonProperty(Required = Required.Always)] public double Delay;
			}

			public class LACDataEng
			{
				[JsonProperty(Required = Required.Always)] public bool Enabled;
				[JsonProperty(Required = Required.Always)] public double Dec;
				[JsonProperty(Required = Required.Always)] public double MinSpeed;
			}

			public class OverSpeedEcoRollDataEng : OverSpeedEcoRollDataDecl
			{
				[JsonProperty(Required = Required.Always)] public double MinSpeed;
				[JsonProperty(Required = Required.Always)] public double OverSpeed;
				[JsonProperty(Required = Required.Always)] public double UnderSpeed;
			}
		}
	}
}