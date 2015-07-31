using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.EngineeringFile
{
	/// <summary>
	///		Represents the Data containing all parameters of the gearbox
	/// </summary>
	/// {
	///  "Header": {
	///    "CreatedBy": "Raphael Luz IVT TU-Graz (85407225-fc3f-48a8-acda-c84a05df6837)",
	///    "Date": "29.07.2014 16:59:17",
	///    "AppVersion": "2.0.4-beta",
	///    "FileVersion": 4
	///  },
	///  "Body": {
	///    "SavedInDeclMode": false,
	///    "ModelName": "Generic 24t Coach",
	///    "Inertia": 0.0,
	///    "TracInt": 1.0,
	///     "TqReserve": 20.0,
	///		"SkipGears": true,
	///		"ShiftTime": 2,
	///		"EaryShiftUp": true,
	///		"StartTqReserve": 20.0,
	///		"StartSpeed": 2.0,
	///		"StartAcc": 0.6,
	///		"GearboxType": "AMT",
	///		"TorqueConverter": {
	///			"Enabled": false,
	///			"File": "<NOFILE>",
	///			"RefRPM": 0.0,
	///			"Inertia": 0.0
	///		}
	///    "Gears": [
	///      {
	///        "Ratio": 3.240355,
	///        "LossMap": "Axle.vtlm"
	///      },
	///      {
	///        "Ratio": 6.38,
	///        "LossMap": "Indirect GearData.vtlm",
	///        "TCactive": false,
	///        "ShiftPolygon": "ShiftPolygon.vgbs"
	///      },
	///		...
	///		]
	/// }
	public class GearboxFileV5Engineering : GearboxFileV5Declaration
	{
		[JsonProperty(Required = Required.Always)] public new DataBodyEng Body;

		public class DataBodyEng : DataBodyDecl
		{
			[JsonProperty(Required = Required.Always)] public new IList<GearDataEng> Gears;

			/// <summary>
			///		[kgm^2] Rotation inertia of the gearbox (constant for all gears)
			/// </summary>
			[JsonProperty(Required = Required.Always)] public double Inertia;

			/// <summary>
			///		[s] Interruption during gear shift event
			/// </summary>
			[JsonProperty("TracInt", Required = Required.Always)] public double TractionInterruption;

			/// <summary>
			///		[%] This parameter is required for the "Allow shift-up inside polygons" and "Skip Gears" option
			/// </summary>
			[JsonProperty("TqReserve")] public double TorqueReserve;

			[JsonProperty] public bool SkipGears;

			/// <summary>
			///   min. time interval between two gearshifts
			/// </summary>
			[JsonProperty] public double ShiftTime;

			/// <summary>
			///   ???
			/// </summary>
			[JsonProperty] public bool EarlyShiftUp;

			/// <summary>
			///  ???
			/// </summary>
			[JsonProperty("StartTqReserve")] public double StartTorqueReserve;

			/// <summary>
			///		[m/s] vehicle speed at start
			/// </summary>
			[JsonProperty] public double StartSpeed;

			/// <summary>
			///   [m/s^2] accelleration of the vehicle at start
			/// </summary>
			[JsonProperty("StartAcc")] public double StartAcceleration;


			/// <summary>
			///		Contains all parameters of the torque converter if used
			/// </summary>
			[JsonProperty] public TorqueConverterDataEng TorqueConverter;
		}

		public class GearDataEng : GearDataDecl
		{
			[JsonProperty] public string ShiftPolygon;
			[JsonProperty] public bool TCactive;
		}

		public class TorqueConverterDataEng
		{
			[JsonProperty(Required = Required.Always)] public bool Enabled;
			[JsonProperty(Required = Required.Always)] public string File;
			[JsonProperty("RefRPM", Required = Required.Always)] public double ReferenceRPM;
			[JsonProperty(Required = Required.Always)] public double Inertia;
		}
	}
}