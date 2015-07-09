using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.Simulation.Data
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
	[DataContract]
	public class VectoJobData : SimulationComponentData
	{
		/// <summary>
		///     A class which represents the json data format for serializing and deserializing the Job Data files.
		/// </summary>
		public class Data
		{
			[JsonProperty(Required = Required.Always)] public DataHeader Header;
			[JsonProperty(Required = Required.Always)] public DataBody Body;

			public class DataHeader
			{
				[JsonProperty(Required = Required.Always)] public string CreatedBy;
				[JsonProperty(Required = Required.Always)] public DateTime Date;
				[JsonProperty(Required = Required.Always)] public string AppVersion;
				[JsonProperty(Required = Required.Always)] public double FileVersion;
			}

			public class DataBody
			{
				[JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

				[JsonProperty(Required = Required.Always)] public string VehicleFile;
				[JsonProperty(Required = Required.Always)] public string EngineFile;
				[JsonProperty(Required = Required.Always)] public string GearboxFile;
				[JsonProperty(Required = Required.Always)] public IList<string> Cycles;
				[JsonProperty] public IList<AuxData> Aux = new List<AuxData>();
				[JsonProperty(Required = Required.Always)] public string VACC;
				[JsonProperty(Required = Required.Always)] public bool EngineOnlyMode;
				[JsonProperty(Required = Required.Always)] public StartStopData StartStop;
				[JsonProperty(Required = Required.Always)] public LACData LAC;
				[JsonProperty(Required = Required.Always)] public OverSpeedEcoRollData OverSpeedEcoRoll;

				public class AuxData
				{
					[JsonProperty(Required = Required.Always)] public string ID;
					[JsonProperty(Required = Required.Always)] public string Type;
					[JsonProperty(Required = Required.Always)] public string Path;
					[JsonProperty(Required = Required.Always)] public string Technology;
				}

				public class StartStopData
				{
					[JsonProperty(Required = Required.Always)] public bool Enabled;
					[JsonProperty(Required = Required.Always)] public double MaxSpeed;
					[JsonProperty(Required = Required.Always)] public double MinTime;
					[JsonProperty(Required = Required.Always)] public double Delay;
				}

				public class LACData
				{
					[JsonProperty(Required = Required.Always)] public bool Enabled;
					[JsonProperty(Required = Required.Always)] public double Dec;
					[JsonProperty(Required = Required.Always)] public double MinSpeed;
				}

				public class OverSpeedEcoRollData
				{
					[JsonProperty(Required = Required.Always)] public string Mode;
					[JsonProperty(Required = Required.Always)] public double MinSpeed;
					[JsonProperty(Required = Required.Always)] public double OverSpeed;
					[JsonProperty(Required = Required.Always)] public double UnderSpeed;
				}
			}
		}

		[DataMember] private Data _data;


		public string VehicleFile
		{
			get { return _data.Body.VehicleFile; }
		}

		public string EngineFile
		{
			get { return _data.Body.EngineFile; }
		}

		public string GearboxFile
		{
			get { return _data.Body.GearboxFile; }
		}

		public IList<string> Cycles
		{
			get { return _data.Body.Cycles; }
		}

		public IList<Data.DataBody.AuxData> Aux
		{
			get { return _data.Body.Aux; }
		}

		public string AccelerationLimitingFile
		{
			get { return _data.Body.VACC; }
		}

		public bool IsEngineOnly
		{
			get { return _data.Body.EngineOnlyMode; }
		}

		public Data.DataBody.StartStopData StartStop
		{
			get { return _data.Body.StartStop; }
		}

		public Data.DataBody.LACData LookAheadCoasting
		{
			get { return _data.Body.LAC; }
		}

		public Data.DataBody.OverSpeedEcoRollData OverSpeedEcoRoll
		{
			get { return _data.Body.OverSpeedEcoRoll; }
		}

		public string JobFileName { get; set; }

		public static VectoJobData ReadFromFile(string fileName)
		{
			return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName), fileName);
		}

		public static VectoJobData ReadFromJson(string json, string basePath = "", string fileName = "")
		{
			var data = new VectoJobData();
			data.JobFileName = fileName;
			//todo handle conversion errors
			var d = JsonConvert.DeserializeObject<Data>(json);

			data._data = d;

			if (d.Header.FileVersion > 2) {
				throw new UnsupportedFileVersionException("Unsupported Version of .vecto file. Got Version: " + d.Header.FileVersion);
			}
			return data;
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
	}
}