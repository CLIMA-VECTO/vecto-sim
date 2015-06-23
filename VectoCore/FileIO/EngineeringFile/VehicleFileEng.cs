using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.FileIO.DeclarationFile;

namespace TUGraz.VectoCore.FileIO.EngineeringFile
{
	internal class VehicleFileV5Engineering : VehicleFileV5Declaration
	{
		[JsonProperty(Required = Required.Always)] public new DataBodyEng Body;


		internal class DataBodyEng : DataBodyDecl
		{
			[JsonProperty] public double CurbWeightExtra;

			[JsonProperty] public double Loading;


			[JsonProperty("rdyn")] public double DynamicTyreRadius;


			[JsonProperty("CdCorrMode")] public string CrossWindCorrectionModeStr;

			[JsonProperty("CdCorrFile")] public string CrossWindCorrectionFile;


			[JsonProperty(Required = Required.Always)] public new AxleConfigData AxleConfig;


			public new class AxleConfigData
			{
				[JsonProperty("Type", Required = Required.Always)] public string TypeStr;
				[JsonProperty(Required = Required.Always)] public IList<AxleDataEng> Axles;
			}

			public class AxleDataEng : AxleDataDecl
			{
				[JsonProperty] public double Inertia;
				[JsonProperty] public double AxleWeightShare;
			}
		}
	}
}

//}