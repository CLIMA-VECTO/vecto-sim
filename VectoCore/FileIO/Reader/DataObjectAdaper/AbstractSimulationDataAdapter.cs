using System;
using System.IO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper
{
	public abstract class AbstractSimulationDataAdapter
	{
		public abstract VehicleData CreateVehicleData(VectoVehicleFile vehicle, Mission segment, Kilogram loading);

		public abstract VehicleData CreateVehicleData(VectoVehicleFile vehicle);

		public abstract CombustionEngineData CreateEngineData(VectoEngineFile engine);

		public abstract GearboxData CreateGearboxData(VectoGearboxFile gearbox, CombustionEngineData engine);

		// =========================

		internal VehicleData SetCommonVehicleData(VehicleFileV5Declaration.DataBodyDecl data, string basePath)
		{
			var retVal = new VehicleData {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				AxleConfiguration = AxleConfigurationHelper.Parse(data.AxleConfig.TypeStr),
				CurbWeight = data.CurbWeight.SI<Kilogram>(),
				//CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				//Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = data.GrossVehicleMassRating.SI<Ton>().Cast<Kilogram>(),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>(),
				//TyreRadius = data.TyreRadius.SI().Milli.Meter.Cast<Meter>(),
				Rim = data.RimStr,
			};

			var retarder = new RetarderData() {
				Type =
					(RetarderData.RetarderType)Enum.Parse(typeof(RetarderData.RetarderType), data.Retarder.TypeStr.ToString(), true),
			};
			if (retarder.Type != RetarderData.RetarderType.None) {
				retarder.LossMap = RetarderLossMap.ReadFromFile(Path.Combine(basePath, data.Retarder.File));
				retarder.Ratio = data.Retarder.Ratio;
			}
			retVal.Retarder = retarder;

			return retVal;
		}

		internal CombustionEngineData SetCommonCombustionEngineData(EngineFileV3Declaration.DataBodyDecl data, string basePath)
		{
			var retVal = new CombustionEngineData() {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				ModelName = data.ModelName,
				Displacement = data.Displacement.SI().Cubic.Centi.Meter.Cast<CubicMeter>(), // convert vom ccm to m^3
				IdleSpeed = data.IdleSpeed.RPMtoRad(),
				ConsumptionMap = FuelConsumptionMap.ReadFromFile(Path.Combine(basePath, data.FuelMap)),
				WHTCUrban = data.WHTCUrban.SI<KilogramPerWattSecond>(),
				WHTCMotorway = data.WHTCMotorway.SI<KilogramPerWattSecond>(),
				WHTCRural = data.WHTCRural.SI<KilogramPerWattSecond>(),
			};
			return retVal;
		}

		internal GearboxData SetCommonGearboxData(GearboxFileV5Declaration.DataBodyDecl data)
		{
			return new GearboxData() {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				ModelName = data.ModelName,
				Type = data.GearboxType.Parse<GearboxType>()
			};
		}

		public abstract DriverData CreateDriverData(VectoJobFile job);
	}
}