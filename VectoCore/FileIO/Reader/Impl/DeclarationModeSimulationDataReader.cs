using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public class DeclarationModeSimulationDataReader : AbstractSimulationDataReader
	{
		internal DeclarationModeSimulationDataReader() {}

		protected void CheckForDeclarationMode(VersionInfo info, string msg)
		{
			if (!info.SavedInDeclarationMode) {
				// throw new VectoException("File not saved in Declaration Mode! - " + msg);
				Log.Warn("File not saved in Declaration Mode! - {0}", msg);
			}
		}

		public override IEnumerable<VectoRunData> NextRun()
		{
			var dao = new DeclarationDataAdapter();
			var tmpVehicle = dao.CreateVehicleData(Vehicle);
			var segment = GetVehicleClassification(tmpVehicle.VehicleCategory, tmpVehicle.AxleConfiguration,
				tmpVehicle.GrossVehicleMassRating, tmpVehicle.CurbWeight);
			var driverdata = dao.CreateDriverData(Job);
			driverdata.AccelerationCurve = AccelerationCurveData.ReadFromStream(segment.AccelerationFile);

			var resultCount = segment.Missions.Sum(m => m.Loadings.Count);

			var engineData = dao.CreateEngineData(Engine);
			var gearboxData = dao.CreateGearboxData(Gearbox, engineData);
			var report = new DeclarationReport(engineData.FullLoadCurve, segment, "CREATOR", engineData.ModelName, "engineStr",
				gearboxData.ModelName, "gearboxStr", Job.BasePath, Job.JobFile, resultCount);

			foreach (var mission in segment.Missions) {
				var cycle = DrivingCycleDataReader.ReadFromStream(mission.CycleFile, CycleType.DistanceBased);
				foreach (var loading in mission.Loadings) {
					var simulationRunData = new VectoRunData {
						Loading = loading.Key,
						VehicleData = dao.CreateVehicleData(Vehicle, mission, loading.Value),
						EngineData = engineData,
						GearboxData = dao.CreateGearboxData(Gearbox, engineData),
						Aux = dao.CreateAuxiliaryData(Aux, mission.MissionType, segment.VehicleClass),
						Cycle = cycle,
						DriverData = driverdata,
						IsEngineOnly = IsEngineOnly,
						JobFileName = Job.JobFile,
						BasePath = "",
						ModFileSuffix = loading.Key.ToString(),
						Report = report,
						Mission = mission,
					};
					simulationRunData.Cycle.Name = mission.MissionType.ToString();
					simulationRunData.VehicleData.VehicleClass = segment.VehicleClass;
					yield return simulationRunData;
				}
			}
		}

		protected override void ProcessJob(VectoJobFile vectoJob)
		{
			try {
				var job = vectoJob as VectoJobFileV2Declaration;
				if (job == null) {
					throw new VectoException("Unhandled Job File Format. Expected: Job File, Version 2, Declaration Mode. Got: {0}",
						vectoJob.GetType());
				}
				Vehicle = ReadVehicle(Path.Combine(job.BasePath, job.Body.VehicleFile));
				Engine = ReadEngine(Path.Combine(job.BasePath, job.Body.EngineFile));
				Gearbox = ReadGearbox(Path.Combine(job.BasePath, job.Body.GearboxFile));
				Aux = ReadAuxiliary(job.BasePath, job.Body.Aux);
			} catch (VectoException e) {
				var message = string.Format("Exception during processing of job file \"{0}\": {1}", vectoJob.JobFile, e.Message);
				Log.Error(message);
				throw new VectoException(message, e);
			}
		}

		protected override VectoJobFile ReadJobFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Job");

			switch (fileInfo.Version) {
				case 2:
					var job = JsonConvert.DeserializeObject<VectoJobFileV2Declaration>(json);
					job.BasePath = file;
					job.JobFile = file;
					return job;
				default:
					throw new UnsupportedFileVersionException("Unsupported version of job-file. Got version " + fileInfo.Version);
			}
		}

		protected override VectoVehicleFile ReadVehicle(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Vehicle");

			switch (fileInfo.Version) {
				case 7:
					var vehicle = JsonConvert.DeserializeObject<VehicleFileV7Declaration>(json);
					vehicle.BasePath = file;
					return vehicle;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of vehicle-file. Got version " + fileInfo.Version);
			}
		}

		protected override VectoEngineFile ReadEngine(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Engine");

			switch (fileInfo.Version) {
				case 3:
					var engine = JsonConvert.DeserializeObject<EngineFileV3Declaration>(json);
					engine.BasePath = file;
					return engine;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of engine-file. Got version " + fileInfo.Version);
			}
		}

		protected override VectoGearboxFile ReadGearbox(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Gearbox");

			switch (fileInfo.Version) {
				case 5:
					var gearbox = JsonConvert.DeserializeObject<GearboxFileV5Declaration>(json);
					gearbox.BasePath = file;
					return gearbox;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of gearbox-file. Got version " + fileInfo.Version);
			}
		}

		protected override IList<VectoRunData.AuxData> ReadAuxiliary(string basePath,
			IEnumerable<VectoAuxiliaryFile> auxiliaries)
		{
			var inputAuxiliaries = auxiliaries.Cast<VectoJobFileV2Declaration.DataBodyDecl.AuxDataDecl>().ToArray();
			return DeclarationData.AuxiliaryIDs().Select(id => {
				try {
					var a = inputAuxiliaries.First(decl => decl.ID == id);
					return new VectoRunData.AuxData {
						ID = a.ID,
						Type = AuxiliaryTypeHelper.Parse(a.Type),
						Technology = a.Technology,
						TechList = a.TechList.DefaultIfNull(Enumerable.Empty<string>()).ToArray(),
						DemandType = AuxiliaryDemandType.Constant
					};
				} catch (InvalidOperationException) {
					var message = string.Format("Auxiliary was not found: Expected: [{0}], Got: [{1}]",
						string.Join(", ", DeclarationData.AuxiliaryIDs()), string.Join(", ", inputAuxiliaries.Select(a => a.ID)));
					Log.Error(message);
					throw new VectoException(message);
				}
			}).Concat(new VectoRunData.AuxData { ID = "", DemandType = AuxiliaryDemandType.Direct }.ToEnumerable()).ToList();
		}

		internal Segment GetVehicleClassification(VehicleCategory category, AxleConfiguration axles, Kilogram grossMassRating,
			Kilogram curbWeight)
		{
			//return new DeclarationSegments().Lookup(vehicle.VehicleCategory(),
			//	EnumHelper.ParseAxleConfigurationType(vehicle.AxleConfig.TypeStr),
			//	vehicle.GrossVehicleMassRating.SI<Ton>().Cast<Kilogram>(), vehicle.CurbWeight.SI<Kilogram>());

			return DeclarationData.Segments.Lookup(category, axles, grossMassRating, curbWeight);
		}

		public override bool IsEngineOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Create gearboxdata instance directly from a file
		/// </summary>
		/// <param name="gearBoxFile"></param>
		/// <param name="engineFile"></param>
		/// <returns>GearboxData instance</returns>
		public static GearboxData CreateGearboxDataFromFile(string gearBoxFile, string engineFile)
		{
			var reader = new DeclarationModeSimulationDataReader();
			var engine = reader.ReadEngine(engineFile);
			var gearbox = reader.ReadGearbox(gearBoxFile);
			var dao = new DeclarationDataAdapter();
			var engineData = dao.CreateEngineData(engine);
			return dao.CreateGearboxData(gearbox, engineData);
		}
	}
}