using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

[assembly: InternalsVisibleTo("VectoCoreTest")]

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public class EngineeringModeSimulationDataReader : AbstractSimulationDataReader
	{
		internal EngineeringModeSimulationDataReader() {}


		protected static void CheckForEngineeringMode(VersionInfo info, string msg)
		{
			if (info.SavedInDeclarationMode) {
				LogManager.GetLogger(typeof (EngineeringModeSimulationDataReader))
					.WarnFormat("File was saved in Declaration Mode but is used for Engineering Mode!");
			}
		}

		protected override void ProcessJob(VectoJobFile vectoJob)
		{
			var declaration = vectoJob as VectoJobFileV2Engineering;
			if (declaration == null) {
				throw new VectoException("Unhandled Job File Format");
			}
			var job = declaration;

			ReadVehicle(Path.Combine(job.BasePath, job.Body.VehicleFile));

			ReadEngine(Path.Combine(job.BasePath, job.Body.EngineFile));

			ReadGearbox(Path.Combine(job.BasePath, job.Body.GearboxFile));
		}

		protected override void ReadVehicle(string file)
		{
			var json = File.ReadAllText(file);
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


		internal VehicleData CreateVehicleData(VehicleFileV5Engineering vehicle)
		{
			var data = vehicle.Body;

			var retVal = SetCommonVehicleData(vehicle);

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

		public override IEnumerable<VectoRunData> NextRun()
		{
			var job = Job as VectoJobFileV2Engineering;
			if (job == null) {
				Log.Warn("Job-file is null or unsupported version");
				yield break;
			}
			foreach (var cycle in job.Body.Cycles) {
				var simulationRunData = new VectoRunData() {
					BasePath = job.BasePath,
					JobFileName = job.JobFile,
					EngineData = CreateEngineData((dynamic) Engine),
					Cycle = DrivingCycleData.ReadFromFile(Path.Combine(job.BasePath, cycle), DrivingCycleData.CycleType.DistanceBased),
					IsEngineOnly = false
				};
				yield return simulationRunData;
			}
		}


		public static VehicleData CreateVehicleDataFromFile(string file)
		{
			var data = DoReadVehicleFile(file);
			return new EngineeringModeSimulationDataReader().CreateVehicleData((dynamic) data);
		}

		public static CombustionEngineData CreateEngineDataFromFile(string file)
		{
			var data = DoReadEngineFile(file);
			return new EngineeringModeSimulationDataReader().CreateEngineData((dynamic) data);
		}

		public static GearboxData CreateGearboxDataFromFile(string file)
		{
			var data = DoReadGearboxFile(file);
			return new EngineeringModeSimulationDataReader().CreateGearboxData((dynamic) data);
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
					Job.JobFile = Path.GetFileName(file);
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
					var tmp = JsonConvert.DeserializeObject<EngineFileV2Engineering>(json);
					tmp.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					return tmp;
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
					var tmp = JsonConvert.DeserializeObject<GearboxFileV4Engineering>(json);
					tmp.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					return tmp;
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
				case 5:
					var tmp = JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
					tmp.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					return tmp;
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of vehicle-file. Got version " + fileInfo.Version);
			}
		}


		internal CombustionEngineData CreateEngineData(EngineFileV2Engineering engine)
		{
			var retVal = SetCommonCombustionEngineData(engine);
			retVal.Inertia = engine.Body.Inertia.SI<KilogramSquareMeter>();
			foreach (var entry in engine.Body.FullLoadCurves) {
				retVal.AddFullLoadCurve(entry.Gears, FullLoadCurve.ReadFromFile(Path.Combine(engine.BasePath, entry.Path), false));
			}

			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV4Engineering gearbox)
		{
			var retVal = SetCommonGearboxData(gearbox.Body);

			var data = gearbox.Body;

			retVal.Inertia = data.Inertia.SI<KilogramSquareMeter>();
			retVal.TractionInterruption = data.TractionInterruption.SI<Second>();
			retVal.SkipGears = data.SkipGears;
			retVal.EarlyShiftUp = data.EarlyShiftUp;
			retVal.TorqueReserve = data.TorqueReserve;
			retVal.StartTorqueReserve = data.StartTorqueReserve;
			retVal.ShiftTime = data.ShiftTime.SI<Second>();
			retVal.StartSpeed = data.StartSpeed.SI<MeterPerSecond>();
			retVal.StartAcceleration = data.StartAcceleration.SI<MeterPerSquareSecond>();

			retVal.HasTorqueConverter = data.TorqueConverter.Enabled;

			for (uint i = 0; i < gearbox.Body.Gears.Count; i++) {
				var gearSettings = gearbox.Body.Gears[(int) i];
				var lossMapPath = Path.Combine(gearbox.BasePath, gearSettings.LossMap);
				TransmissionLossMap lossMap = TransmissionLossMap.ReadFromFile(lossMapPath, gearSettings.Ratio);

				var shiftPolygon = !String.IsNullOrEmpty(gearSettings.ShiftPolygon)
					? ShiftPolygon.ReadFromFile(Path.Combine(gearbox.BasePath, gearSettings.ShiftPolygon))
					: null;

				var gear = new GearData(lossMap, shiftPolygon, gearSettings.Ratio, gearSettings.TCactive);
				if (i == 0) {
					retVal.AxleGearData = gear;
				} else {
					retVal._gearData.Add(i, gear);
				}
			}
			return retVal;
		}
	}
}