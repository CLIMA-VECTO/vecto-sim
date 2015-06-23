using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.FileIO.DeclarationFile
{
	internal class VehicleFileV5Declaration : VectoVehicleFile
	{
		[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
		[JsonProperty(Required = Required.Always)] public DataBodyDecl Body;

		public class DataBodyDecl
		{
			[JsonProperty("SavedInDeclMode", Required = Required.Always)] public bool SavedInDeclarationMode;

			[JsonProperty("VehCat", Required = Required.Always)] public string VehicleCategoryStr;

			public VehicleCategory VehicleCategory()
			{
				return (VehicleCategory) Enum.Parse(typeof (VehicleCategory), VehicleCategoryStr, true);
			}

			[JsonProperty(Required = Required.Always)] public double CurbWeight;

			[JsonProperty("MassMax", Required = Required.Always)] public double GrossVehicleMassRating;

			[JsonProperty("Cd", Required = Required.Always)] public double DragCoefficient;

			[JsonProperty("CrossSecArea", Required = Required.Always)] public double CrossSectionArea;

			[JsonProperty("Cd2")] public double DragCoefficientRigidTruck; // without trailer

			[JsonProperty("CrossSecArea2")] public double CrossSectionAreaRigidTruck;

			[JsonProperty("Rim", Required = Required.Always)] public string RimStr;

			[JsonProperty("Retarder", Required = Required.Always)] public RetarderData.Data Retarder;

			[JsonProperty(Required = Required.Always)] public AxleConfigData AxleConfig;


			public class AxleConfigData
			{
				[JsonProperty("Type", Required = Required.Always)] public string TypeStr;
				[JsonProperty(Required = Required.Always)] public IList<AxleDataDecl> Axles;
			}

			public class AxleDataDecl
			{
				//[JsonProperty]
				//public double Inertia;
				[JsonProperty("Wheels", Required = Required.Always)] public string WheelsStr;
				//[JsonProperty(Required = Required.Always)]
				//public double AxleWeightShare;
				[JsonProperty(Required = Required.Always)] public bool TwinTyres;
				[JsonProperty("RRCISO", Required = Required.Always)] public double RollResistanceCoefficient;
				[JsonProperty("FzISO", Required = Required.Always)] public double TyreTestLoad;
			}
		}
	}

//	}
}