using System;
using System.Collections.Generic;
using System.IO;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public abstract class AbstractSimulationDataReader : InputFileReader, ISimulationDataReader
	{
		//protected string JobBasePath = "";

		protected VectoJobFile Job;

		protected VectoVehicleFile Vehicle;

		protected VectoGearboxFile Gearbox;

		protected VectoEngineFile Engine;


		public void SetJobFile(string filename)
		{
			ReadJobFile(filename);
			ProcessJob(Job);
		}

		public abstract IEnumerable<VectoRunData> NextRun();


		protected virtual void ProcessJob(VectoJobFile job)
		{
			throw new VectoException("Invalid JobFile Container");
		}


		// has to read the file string and create file-container
		protected abstract void ReadJobFile(string file);

		// has to read the file string and create file-container
		protected abstract void ReadVehicle(string file);

		protected abstract void ReadEngine(string file);

		protected abstract void ReadGearbox(string file);


		protected internal virtual Segment GetVehicleClassification(VectoVehicleFile vehicle)
		{
			throw new NotImplementedException("Vehicleclassification for base-class not possible!");
		}

		internal virtual VehicleData CreateVehicleData(VectoVehicleFile vehicle, Mission segment, Kilogram loading)
		{
			throw new NotImplementedException("CreateVehicleData for base-class not possible!");
		}

		internal virtual CombustionEngineData CreateEngineData(VectoEngineFile engine)
		{
			throw new NotImplementedException("CreateEngineData for base-class not possible!");
		}

		internal virtual GearboxData CreateGearboxData(VectoGearboxFile gearbox)
		{
			throw new NotImplementedException("CreateGearboxDataFromFile for base-class not possible!");
		}

		internal VehicleData SetCommonVehicleData(VehicleFileV5Declaration vehicle)
		{
			var data = ((dynamic) vehicle).Body;
			return new VehicleData {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				AxleConfiguration =
					(AxleConfiguration) Enum.Parse(typeof (AxleConfiguration), "AxleConfig_" + data.AxleConfig.TypeStr),
				// TODO: @@@quam better use of enum-prefix
				CurbWeight = SIConvert<Kilogram>(data.CurbWeight),
				//CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				//Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = SIConvert<Kilogram>(data.GrossVehicleMassRating * 1000),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = SIConvert<SquareMeter>(data.CrossSectionArea),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = SIConvert<SquareMeter>(data.CrossSectionAreaRigidTruck),
				//TyreRadius = data.TyreRadius.SI().Milli.Meter.Cast<Meter>(),
				Rim = data.RimStr,
				Retarder = new RetarderData() {
					LossMap = RetarderLossMap.ReadFromFile(Path.Combine(vehicle.BasePath, data.Retarder.File)),
					Type =
						(RetarderData.RetarderType) Enum.Parse(typeof (RetarderData.RetarderType), data.Retarder.TypeStr.ToString(), true),
					Ratio = data.Retarder.Ratio
				}
			};
		}

		internal CombustionEngineData SetCommonCombustionEngineData(EngineFileV2Declaration engine)
		{
			var data = ((dynamic) engine).Body;
			var retVal = new CombustionEngineData() {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				ModelName = data.ModelName,
				Displacement = SIConvert<CubicMeter>(data.Displacement * 0.000001), // convert vom ccm to m^3
				IdleSpeed = DoubleExtensionMethods.RPMtoRad(data.IdleSpeed),
				ConsumptionMap = FuelConsumptionMap.ReadFromFile(Path.Combine(engine.BasePath, data.FuelMap)),
				WHTCUrban = SIConvert<KilogramPerWattSecond>(data.WHTCUrban),
				WHTCMotorway = SIConvert<KilogramPerWattSecond>(data.WHTCMotorway),
				WHTCRural = SIConvert<KilogramPerWattSecond>(data.WHTCRural),
			};
			return retVal;
		}

		internal GearboxData SetCommonGearboxData(GearboxFileV4Declaration.DataBodyDecl data)
		{
			return new GearboxData() {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				ModelName = data.ModelName,
				Type = (GearboxData.GearboxType) Enum.Parse(typeof (GearboxData.GearboxType), data.GearboxType, true),
			};
		}

		protected T1 SIConvert<T1>(double val) where T1 : SIBase<T1>
		{
			return val.SI<T1>();
		}
	}
}