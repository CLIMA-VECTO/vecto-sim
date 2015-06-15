using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.FileIO
{
//	public class VehicleFileDeclaration
//	{

	public class VehicleFileV5Declaration
	{
		[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
		[JsonProperty(Required = Required.Always)] public DataBodyDecl Body;

		//public void SetProperties(VehicleData vehicleData)
		//{
		//	vehicleData.AxleConfiguration = AxleConfiguration.AxleConfig4x2;

		//	vehicleData.Retarder = new RetarderData(Body.Retarder, vehicleData.BasePath);

		//	vehicleData.AxleData = new List<VehicleData.Axle>(Body.AxleConfig.Axles.Count);
		//	var RRC = 0.0;
		//	var mRed0 = 0.SI<Kilogram>();
		//	foreach (var axleData in Body.AxleConfig.Axles) {
		//		var axle = new VehicleData.Axle(axleData);
		//		if (axle.RollResistanceCoefficient < 0) {
		//			throw new VectoException("Axle roll resistance coefficient < 0");
		//		}
		//		if (axle.TyreTestLoad <= 0) {
		//			throw new VectoException("Axle tyre test load (FzISO) must be greater than 0!");
		//		}
		//		var nrWheels = axle.TwinTyres ? 4 : 2;
		//		RRC += axle.AxleWeightShare * axle.RollResistanceCoefficient *
		//				Math.Pow(
		//					(axle.AxleWeightShare * vehicleData.TotalVehicleWeight() * Physics.GravityAccelleration / axle.TyreTestLoad /
		//					nrWheels).Double(), Physics.RollResistanceExponent - 1);
		//		mRed0 += nrWheels * (axle.Inertia / vehicleData.DynamicTyreRadius / vehicleData.DynamicTyreRadius).Cast<Kilogram>();

		//		vehicleData._axleData.Add(axle);
		//	}
		//	vehicleData.TotalRollResistanceCoefficient = RRC;
		//	vehicleData.ReducedMassWheels = mRed0;

		//	vehicleData.VehicleCategory = Enum.Parse(typeof (VehicleCategory), Body.VehicleCategoryStr, true);


		//	//switch (Body.CrossWindCorrectionModeStr) {
		//	//	case "CdOfBeta":
		//	//		vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.VAirBeta;
		//	//		break;
		//	//	case "CdOfV":
		//	//		vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.SpeedDependent;
		//	//		break;
		//	//	default:
		//	//		vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection;
		//	//		break;
		//	//}
		//}

		public class DataBodyDecl
		{
			[JsonProperty("SavedInDeclMode", Required = Required.Always)] public bool SavedInDeclarationMode;

			[JsonProperty("VehCat", Required = Required.Always)] public string VehicleCategoryStr;

			public VehicleCategory VehicleCategory()
			{
				return (VehicleCategory)Enum.Parse(typeof(VehicleCategory), VehicleCategoryStr, true);
			}

			[JsonProperty(Required = Required.Always)] public double CurbWeight;

			//[JsonProperty]
			//public double CurbWeightExtra;

			//[JsonProperty]
			//public double Loading;

			[JsonProperty("MassMax", Required = Required.Always)] public double GrossVehicleMassRating;

			[JsonProperty("Cd", Required = Required.Always)] public double DragCoefficient;

			[JsonProperty("CrossSecArea", Required = Required.Always)] public double CrossSectionArea;

			[JsonProperty("Cd2")] public double DragCoefficientRigidTruck; // without trailer

			[JsonProperty("CrossSecArea2")] public double CrossSectionAreaRigidTruck;


			//[JsonProperty("rdyn")]
			//public double DynamicTyreRadius;

			[JsonProperty("Rim", Required = Required.Always)] public string RimStr;

			//[JsonProperty("CdCorrMode")]
			//public string CrossWindCorrectionModeStr;

			//[JsonProperty("CdCorrFile")]
			//public string CrossWindCorrectionFile;

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