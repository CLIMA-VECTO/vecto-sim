using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

[assembly: InternalsVisibleTo("VectoCoreTest")]

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl
{
	public class EngineeringModeSimulationComponentFactory : AbstractSimulationRunCreator
	{
		internal EngineeringModeSimulationComponentFactory() {}


		protected void CheckForEngineeringMode(VersionInfo info, string msg)
		{
			if (info.SavedInDeclarationMode) {
				Log.WarnFormat("File was saved in Declaration Mode but is used for Engineering Mode!");
			}
		}

		protected override void ReadVehicle(string file)
		{
			var json = File.ReadAllText(Job.BasePath + file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Vehicle");

			switch (fileInfo.Version) {
				case 5:
					Vehicle = JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Version);
			}
		}

		public void ReadEngineFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public void ReadEngineJson(string jsonData, string basePath)
		{
			throw new NotImplementedException();
		}


		internal VectoJobData CreateVectoJobData(VectoJobFileV2Engineering data, string basePath)
		{
			return new VectoJobData();
		}


		internal VehicleData CreateVehicleData(VehicleFileV5Engineering.DataBodyEng data, string basePath)
		{
			return new VehicleData {
				BasePath = basePath,
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				AxleConfiguration =
					(AxleConfiguration) Enum.Parse(typeof (AxleConfiguration), "AxleConfig_" + data.AxleConfig.TypeStr),
				// TODO: @@@quam better use of enum-prefix
				CurbWeight = data.CurbWeight.SI<Kilogram>(),
				CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = data.GrossVehicleMassRating.SI().Kilo.Kilo.Gramm.Cast<Kilogram>(),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>(),
				DynamicTyreRadius = data.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>(),
				Rim = data.RimStr,
				Retarder = new RetarderData() {
					LossMap = RetarderLossMap.ReadFromFile(basePath + data.Retarder.File),
					Type = (RetarderData.RetarderType) Enum.Parse(typeof (RetarderData), data.Retarder.TypeStr, true),
					Ratio = data.Retarder.Ratio
				},
				AxleData = data.AxleConfig.Axles.Select(axle => new Axle {
					Inertia = DoubleExtensionMethods.SI<KilogramSquareMeter>(axle.Inertia),
					TwinTyres = axle.TwinTyres,
					RollResistanceCoefficient = axle.RollResistanceCoefficient,
					AxleWeightShare = axle.AxleWeightShare,
					TyreTestLoad = DoubleExtensionMethods.SI<Newton>(axle.TyreTestLoad),
					//Wheels = axle.WheelsStr
				}).ToList()
			};
		}

		public override IEnumerable<IVectoRun> NextRun()
		{
			throw new NotImplementedException();
		}

		public CombustionEngineData CreateEngineDataFromFile(string file)
		{
			var data = DoReadEngineFile(file);
			return CreateEngineData((dynamic) data);
		}


		protected override void ReadJobFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Job");

			switch (fileInfo.Version) {
				case 2:
					Job = JsonConvert.DeserializeObject<VectoJobFileV2Engineering>(json);
					Job.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported version of job-file. Got version " + fileInfo.Version);
			}
		}

		protected override void ReadEngine(string file)
		{
			Engine = DoReadEngineFile(file);
		}

		protected VectoEngineFile DoReadEngineFile(string file)
		{
			var json = File.ReadAllText(Job.BasePath + file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Engine");

			switch (fileInfo.Version) {
				case 2:
					return JsonConvert.DeserializeObject<EngineFileV2Engineering>(json);
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of engine-file. Got version " + fileInfo.Version);
			}
		}

		protected override void ReadGearbox(string json)
		{
			throw new NotImplementedException();
		}

		internal CombustionEngineData CreateEngineData(EngineFileV2Engineering engine)
		{
			var retVal = new CombustionEngineData();

			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV4Engineering gearbox)
		{
			var retVal = new GearboxData();

			return retVal;
		}
	}
}