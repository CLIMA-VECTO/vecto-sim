using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.DeclarationFile
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
	///		"GearboxType": "AMT",
	///    "Gears": [
	///      {
	///        "Ratio": 3.240355,
	///        "LossMap": "Axle.vtlm"
	///      },
	///      {
	///        "Ratio": 6.38,
	///        "LossMap": "Indirect GearData.vtlm",
	///      },
	///		...
	///		]
	/// }
	public class GearboxFileV5Declaration : VectoGearboxFile
	{
		[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
		[JsonProperty(Required = Required.Always)] public DataBodyDecl Body;

		public class DataBodyDecl
		{
			[JsonProperty("SavedInDeclMode", Required = Required.Always)] public bool SavedInDeclarationMode;

			/// <summary>
			///		Model. Free text defining the gearbox model, type, etc.
			/// </summary>
			[JsonProperty(Required = Required.Always)] public string ModelName;

			[JsonProperty(Required = Required.Always)] public string GearboxType;

			[JsonProperty(Required = Required.Always)] public IList<GearDataDecl> Gears;
		}

		public class GearDataDecl
		{
			[JsonProperty(Required = Required.Always)] public double Ratio;
			[JsonProperty(Required = Required.Always)] public string LossMap;
			[JsonProperty] public string FullLoadCurve;
		}
	}
}