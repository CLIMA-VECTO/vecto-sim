using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.FileIO.DeclarationFile
{
	/// <summary>
	///     A class which represents the json data format for serializing and deserializing the Job Data files.
	/// </summary>
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
	///    "VehicleFile": "24t Coach.vveh",
	///    "EngineFile": "24t Coach.veng",
	///    "GearboxFile": "24t Coach.vgbx",
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
	public class VectoJobFileV2Declaration
	{
		[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
		[JsonProperty(Required = Required.Always)] public DataBodyDecl Body;


		public class DataBodyDecl
		{
			[JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

			[JsonProperty(Required = Required.Always)] public string VehicleFile;
			[JsonProperty(Required = Required.Always)] public string EngineFile;
			[JsonProperty(Required = Required.Always)] public string GearboxFile;
			//[JsonProperty(Required = Required.Always)] public IList<string> Cycles;
			[JsonProperty] public IList<AuxDataDecl> Aux = new List<AuxDataDecl>();
			//[JsonProperty(Required = Required.Always)] public string VACC;
			//[JsonProperty(Required = Required.Always)] public bool EngineOnlyMode;
			[JsonProperty(Required = Required.Always)] public StartStopDataDecl StartStop;
			//[JsonProperty(Required = Required.Always)] public LACData LAC;
			[JsonProperty(Required = Required.Always)] public OverSpeedEcoRollDataDecl OverSpeedEcoRoll;

			public class AuxDataDecl
			{
				//[JsonProperty(Required = Required.Always)] public string ID;
				//[JsonProperty(Required = Required.Always)] public string Type;
				//[JsonProperty(Required = Required.Always)] public string Path;
				[JsonProperty(Required = Required.Always)] public string Technology;
			}

			public class StartStopDataDecl
			{
				[JsonProperty(Required = Required.Always)] public bool Enabled;
				//[JsonProperty(Required = Required.Always)] public double MaxSpeed;
				//[JsonProperty(Required = Required.Always)] public double MinTime;
				//[JsonProperty(Required = Required.Always)] public double Delay;
			}

			//public class LACData
			//{
			//	[JsonProperty(Required = Required.Always)] public bool Enabled;
			//	[JsonProperty(Required = Required.Always)] public double Dec;
			//	[JsonProperty(Required = Required.Always)] public double MinSpeed;
			//}

			public class OverSpeedEcoRollDataDecl
			{
				[JsonProperty(Required = Required.Always)] public string Mode;
				//[JsonProperty(Required = Required.Always)] public double MinSpeed;
				//[JsonProperty(Required = Required.Always)] public double OverSpeed;
				//[JsonProperty(Required = Required.Always)] public double UnderSpeed;
			}
		}
	}
}