using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl
{
	public abstract class AbstractSimulationRunCreator : InputFileReader
	{
		//protected string JobBasePath = "";

		protected VectoJobFile Job;

		protected VectoVehicleFile Vehicle;

		protected VectoGearboxFile Gearbox;

		protected VectoEngineFile Engine;

		public void SetJobFile(string filename)
		{
			//JobBasePath = Path.GetDirectoryName(filenname) + Path.DirectorySeparatorChar;
			//SetJobJson(File.ReadAllText(filenname));
			ReadJobFile(filename);
			ProcessJob((dynamic) Job);
		}

		//public void SetJobJson(string file)
		//{
		//	ReadJobFile(file);
		//	ProcessJob((dynamic) Job);
		//}

		public abstract IEnumerable<IVectoRun> NextRun();


		protected void ProcessJob(VectoJobFile job)
		{
			throw new VectoException("Invalid JobFile Format");
		}

		protected void ProcessJob(VectoJobFileV2Declaration job)
		{
			ReadVehicle(Path.Combine(job.BasePath, job.Body.VehicleFile));

			ReadEngine(Path.Combine(job.BasePath, job.Body.EngineFile));

			ReadGearbox(Path.Combine(job.BasePath, job.Body.GearboxFile));
		}

		// has to read the file string and create file-container
		protected abstract void ReadJobFile(string file);

		// has to read the file string and create file-container
		protected abstract void ReadVehicle(string file);

		protected abstract void ReadEngine(string file);

		protected abstract void ReadGearbox(string file);

		protected internal Segment GetVehicleClassification(VectoVehicleFile vehicle)
		{
			throw new NotImplementedException("Vehicleclassification for base-class not possible!");
		}

		internal VehicleData CreateVehicleData(VectoVehicleFile vehicle, Mission segment, Kilogram loading)
		{
			throw new NotImplementedException("CreateVehicleData for base-class not possible!");
		}

		internal CombustionEngineData CreateEngineData(VectoEngineFile engine)
		{
			throw new NotImplementedException("CreateEngineData for base-class not possible!");
		}

		internal GearboxData CreateGearboxData(VectoGearboxFile gearbox)
		{
			throw new NotImplementedException("CreateGearboxDataFromFile for base-class not possible!");
		}

		internal VehicleData SetCommonVehicleData(VehicleFileV5Declaration.DataBodyDecl data)
		{
			return new VehicleData {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				AxleConfiguration =
					(AxleConfiguration) Enum.Parse(typeof (AxleConfiguration), "AxleConfig_" + data.AxleConfig.TypeStr),
				// TODO: @@@quam better use of enum-prefix
				CurbWeight = data.CurbWeight.SI<Kilogram>(),
				//CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				//Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = data.GrossVehicleMassRating.SI().Kilo.Kilo.Gramm.Cast<Kilogram>(),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>(),
				//TyreRadius = data.TyreRadius.SI().Milli.Meter.Cast<Meter>(),
				Rim = data.RimStr,
				Retarder = new RetarderData() {
					LossMap = RetarderLossMap.ReadFromFile(data.Retarder.File),
					Type =
						(RetarderData.RetarderType) Enum.Parse(typeof (RetarderData.RetarderType), data.Retarder.TypeStr.ToString(), true),
					Ratio = data.Retarder.Ratio
				}
			};
		}

		internal GearboxData SetcommonGearboxData(GearboxFileV4Declaration.DataBodyDecl data)
		{
			return new GearboxData() {
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				ModelName = data.ModelName,
				Type = (GearboxData.GearboxType) Enum.Parse(typeof (GearboxData.GearboxType), data.GearboxType, true),
			};
		}
	}
}