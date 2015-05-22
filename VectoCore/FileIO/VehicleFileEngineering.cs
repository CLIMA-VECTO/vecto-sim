using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO
{
//	class VehicleFileEngineering
//	{

	public class VehicleFileV5Engineering : VehicleFileV5Declaration
	{
//		[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
		[JsonProperty(Required = Required.Always)] public new DataBodyEng Body;

		//public void SetProperties(VehicleData vehicleData)
		//{
		//	vehicleData._axleConfiguration = VehicleData.AxleConfiguration.AxleConfig4x2;

		//	vehicleData._retarder = new RetarderData(Body.Retarder, vehicleData.BasePath);

		//	vehicleData._axleData = new List<VehicleData.Axle>(Body.AxleConfig.Axles.Count);
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

		//	switch (Body.VehicleCategoryStr) {
		//		case "RigidTruck":
		//			vehicleData.Category = VehicleData.VehicleCategory.RigidTruck;
		//			break;
		//		case "Tractor":
		//			vehicleData.Category = VehicleData.VehicleCategory.Tractor;
		//			break;
		//		case "CityBus":
		//			vehicleData.Category = VehicleData.VehicleCategory.CityBus;
		//			break;
		//		case "InterurbanBus":
		//			vehicleData.Category = VehicleData.VehicleCategory.InterurbanBus;
		//			break;
		//		case "Coach":
		//			vehicleData.Category = VehicleData.VehicleCategory.Coach;
		//			break;
		//	}

		//	switch (Body.CrossWindCorrectionModeStr) {
		//		case "CdOfBeta":
		//			vehicleData._crossWindCorrectionMode = VehicleData.CrossWindCorrectionMode.VAirBeta;
		//			break;
		//		case "CdOfV":
		//			vehicleData._crossWindCorrectionMode = VehicleData.CrossWindCorrectionMode.SpeedDependent;
		//			break;
		//		default:
		//			vehicleData._crossWindCorrectionMode = VehicleData.CrossWindCorrectionMode.NoCorrection;
		//			break;
		//	}
		//}

		public class DataBodyEng : DataBodyDecl
		{
			//[JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

			//[JsonProperty("VehCat", Required = Required.Always)] public string VehicleCategoryStr;

			//[JsonProperty(Required = Required.Always)] public double CurbWeight;

			[JsonProperty] public double CurbWeightExtra;

			[JsonProperty] public double Loading;

			//[JsonProperty("MassMax", Required = Required.Always)] public double GrossVehicleMassRating;

			//[JsonProperty("Cd2")] public double DragCoefficientRigidTruck; // without trailer

			//[JsonProperty("CrossSecArea2")] public double CrossSectionAreaRigidTruck;

			//[JsonProperty("Cd", Required = Required.Always)] public double DragCoefficient;

			//[JsonProperty("CrossSecArea", Required = Required.Always)] public double CrossSectionArea;

			[JsonProperty("rdyn")] public double DynamicTyreRadius;

			//[JsonProperty("Rim")] public string RimStr;

			[JsonProperty("CdCorrMode")] public string CrossWindCorrectionModeStr;

			[JsonProperty("CdCorrFile")] public string CrossWindCorrectionFile;

			//[JsonProperty("Retarder", Required = Required.Always)] public RetarderData.Data Retarder;

			[JsonProperty(Required = Required.Always)] public new AxleConfigData AxleConfig;


			public new class AxleConfigData
			{
				[JsonProperty("Type", Required = Required.Always)] public string TypeStr;
				[JsonProperty(Required = Required.Always)] public IList<AxleDataEng> Axles;
			}

			public class AxleDataEng : AxleDataDecl
			{
				[JsonProperty] public double Inertia;
//				[JsonProperty] public string WheelsStr;
				[JsonProperty] public double AxleWeightShare;
//				[JsonProperty(Required = Required.Always)] public bool TwinTyres;
//				[JsonProperty("RRCISO", Required = Required.Always)] public double RollResistanceCoefficient;
//				[JsonProperty("FzISO", Required = Required.Always)] public double TyreTestLoad;
			}
		}
	}
}

//}