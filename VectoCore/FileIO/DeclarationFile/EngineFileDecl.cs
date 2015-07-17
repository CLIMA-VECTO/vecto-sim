using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.FileIO.DeclarationFile
{
	/// <summary>
	///     Represents the CombustionEngineData. Fileformat: .veng
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
	internal class EngineFileV2Declaration : VectoEngineFile
	{
		[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
		[JsonProperty(Required = Required.Always)] public DataBodyDecl Body;

		public class DataBodyDecl
		{
			[JsonProperty("SavedInDeclMode", Required = Required.Always)] public bool SavedInDeclarationMode;

			/// <summary>
			///     Model. Free text defining the engine model, type, etc.
			/// </summary>
			[JsonProperty(Required = Required.Always)] public string ModelName;

			/// <summary>
			///     [ccm] Displacement in cubic centimeter.
			///     Used in Declaration Mode to calculate inertia.
			/// </summary>
			[JsonProperty(Required = Required.Always)] public double Displacement;

			/// <summary>
			///     [rpm] Idling Engine Speed
			///     Low idle, applied in simulation for vehicle standstill in neutral gear position.
			/// </summary>
			[JsonProperty("IdlingSpeed", Required = Required.Always)] public double IdleSpeed;

			[JsonProperty(Required = Required.Always)] public IList<DataFullLoadCurve> FullLoadCurves;

			/// <summary>
			///     The Fuel Consumption Map is used to calculate the base Fuel Consumption (FC) value.
			/// </summary>
			[JsonProperty(Required = Required.Always)] public string FuelMap;

			/// <summary>
			///     [g/kWh] The WHTC test results are required in Declaration Mode for the motorway WHTC FC Correction.
			/// </summary>
			[JsonProperty("WHTC-Motorway")] public double WHTCMotorway;

			/// <summary>
			///     [g/kWh] The WHTC test results are required in Declaration Mode for the rural WHTC FC Correction.
			/// </summary>
			[JsonProperty("WHTC-Rural")] public double WHTCRural;

			/// <summary>
			///     [g/kWh] The WHTC test results are required in Declaration Mode for the urban WHTC FC Correction.
			/// </summary>
			[JsonProperty("WHTC-Urban")] public double WHTCUrban;

			/// <summary>
			///     Multiple Full Load and Drag Curves (.vfld) can be defined and assigned to different gears.
			///     GearData "0" must be assigned for idling and Engine Only Mode.
			/// </summary>
			public class DataFullLoadCurve
			{
				[JsonProperty(Required = Required.Always)] public string Gears;
				[JsonProperty(Required = Required.Always)] public string Path;
			}
		}
	}
}