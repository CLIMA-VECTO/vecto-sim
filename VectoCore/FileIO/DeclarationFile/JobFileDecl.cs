using System.Collections.Generic;
using Newtonsoft.Json;

namespace TUGraz.VectoCore.FileIO.DeclarationFile
{
	/// <summary>
	/// A class which represents the json data format for serializing and deserializing the Job Data files.
	/// Fileformat: .vecto
	/// </summary>
	/// <code>
	/// {
	///   "Header": {
	///     "CreatedBy": " ()",
	///     "Date": "3/4/2015 12:31:06 PM",
	///     "AppVersion": "2.0.4-beta3",
	///     "FileVersion": 2
	///   },
	///   "Body": {
	///     "SavedInDeclMode": true,
	///     "VehicleFile": "../Components/12t Delivery Truck.vveh",
	///     "EngineFile": "../Components/12t Delivery Truck.veng",
	///     "GearboxFile": "../Components/12t Delivery Truck.vgbx",
	///     "Cycles": [
	///       "Long Haul",
	///       "Regional Delivery",
	///       "Urban Delivery"
	///     ],
	///     "Aux": [
	///       {
	///         "ID": "FAN",
	///         "Type": "Fan",
	///         "Path": "<NOFILE>",
	///         "Technology": ""
	///       },
	///       {
	///         "ID": "STP",
	///         "Type": "Steering pump",
	///         "Path": "<NOFILE>",
	///         "Technology": ""
	///       },
	///       {
	///         "ID": "AC",
	///         "Type": "HVAC",
	///         "Path": "<NOFILE>",
	///         "Technology": ""
	///       },
	///       {
	///         "ID": "ES",
	///         "Type": "Electric System",
	///         "Path": "<NOFILE>",
	///         "Technology": "",
	///         "TechList": []
	///       },
	///       {
	///         "ID": "PS",
	///         "Type": "Pneumatic System",
	///         "Path": "<NOFILE>",
	///         "Technology": ""
	///       }
	///     ],
	///     "VACC": "<NOFILE>",
	///     "EngineOnlyMode": true,
	///     "StartStop": {
	///       "Enabled": false,
	///       "MaxSpeed": 5.0,
	///       "MinTime": 5.0,
	///       "Delay": 5
	///     },
	///     "LAC": {
	///       "Enabled": true,
	///       "Dec": -0.5,
	///       "MinSpeed": 50.0
	///     },
	///     "OverSpeedEcoRoll": {
	///       "Mode": "OverSpeed",
	///       "MinSpeed": 50.0,
	///       "OverSpeed": 5.0,
	///       "UnderSpeed": 5.0
	///     }
	///   }
	/// }
	/// </code>
	public class VectoJobFileV2Declaration : VectoJobFile
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

			public class AuxDataDecl : VectoAuxiliaryFile
			{
				[JsonProperty(Required = Required.Always)] public string ID;
				[JsonProperty(Required = Required.Always)] public string Type;
				//[JsonProperty(Required = Required.Always)] public string Path;
				[JsonProperty(Required = Required.Always)] public string Technology;
				[JsonProperty] public IList<string> TechList;
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