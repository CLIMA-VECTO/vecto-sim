using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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


		protected static void CheckForEngineeringMode(VersionInfo info, string msg)
		{
			if (info.SavedInDeclarationMode) {
				LogManager.GetLogger(typeof (EngineeringModeSimulationComponentFactory))
					.WarnFormat("File was saved in Declaration Mode but is used for Engineering Mode!");
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


		internal VectoJobData CreateVectoJobData(VectoJobFileV2Engineering data, string basePath)
		{
			return new VectoJobData();
		}


		internal VehicleData CreateVehicleData(VehicleFileV5Engineering vehicle)
		{
			var data = vehicle.Body;

			var retVal = SetCommonVehicleData(data);

			retVal.BasePath = vehicle.BasePath;
			retVal.CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>();
			retVal.Loading = data.Loading.SI<Kilogram>();
			retVal.DynamicTyreRadius = data.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>();

			retVal.AxleData = data.AxleConfig.Axles.Select(axle => new Axle {
				Inertia = DoubleExtensionMethods.SI<KilogramSquareMeter>(axle.Inertia),
				TwinTyres = axle.TwinTyres,
				RollResistanceCoefficient = axle.RollResistanceCoefficient,
				AxleWeightShare = axle.AxleWeightShare,
				TyreTestLoad = DoubleExtensionMethods.SI<Newton>(axle.TyreTestLoad),
				//Wheels = axle.WheelsStr
			}).ToList();
			return retVal;
		}

		public override IEnumerable<IVectoRun> NextRun()
		{
			throw new NotImplementedException();
		}


		public static VehicleData CreateVehicleDataFromFile(string file)
		{
			var data = DoReadVehicleFile(file);
			return new EngineeringModeSimulationComponentFactory().CreateVehicleData((dynamic) data);
		}

		public static CombustionEngineData CreateEngineDataFromFile(string file)
		{
			var data = DoReadEngineFile(file);
			return new EngineeringModeSimulationComponentFactory().CreateEngineData((dynamic) data);
		}

		public static GearboxData CreateGearboxDataFromFile(string file)
		{
			var data = DoReadGearboxFile(file);
			return new EngineeringModeSimulationComponentFactory().CreateGearboxData((dynamic) data);
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

		protected override void ReadGearbox(string file)
		{
			Gearbox = DoReadGearboxFile(file);
		}

		protected static VectoEngineFile DoReadEngineFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Engine");

			switch (fileInfo.Version) {
				case 2:
					return JsonConvert.DeserializeObject<EngineFileV2Engineering>(json);
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of engine-file. Got version " + fileInfo.Version);
			}
		}

		protected static VectoGearboxFile DoReadGearboxFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Gearbox");

			switch (fileInfo.Version) {
				case 4:
					return JsonConvert.DeserializeObject<GearboxFileV4Engineering>(json);
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of gearbox-file. Got version " + fileInfo.Version);
			}
		}

		protected static VectoVehicleFile DoReadVehicleFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Vehicle");

			switch (fileInfo.Version) {
				case 4:
					return JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of vehicle-file. Got version " + fileInfo.Version);
			}
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