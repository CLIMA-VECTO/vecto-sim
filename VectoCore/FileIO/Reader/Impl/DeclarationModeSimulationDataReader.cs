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

		protected void CheckForDeclarationMode(InputFileReader.VersionInfo info, string msg)
		{
			if (!info.SavedInDeclarationMode) {
				// throw new VectoException("File not saved in Declaration Mode! - " + msg);
				Log.WarnFormat("File not saved in Declaration Mode! - {0}", msg);
			}
		}

		public override IEnumerable<VectoRunData> NextRun()
		{
			var dao = new DeclarationDataAdapter();
			var tmpVehicle = dao.CreateVehicleData(Vehicle);
			var segment = GetVehicleClassification(tmpVehicle.VehicleCategory, tmpVehicle.AxleConfiguration,
				tmpVehicle.GrossVehicleMassRating, tmpVehicle.CurbWeight);

			var cycleParser = new DrivingCycleData.DistanceBasedDataParser();

			foreach (var mission in segment.Missions) {
				var cycleData = VectoCSVFile.ReadStream(mission.CycleFile);
				foreach (var loading in mission.Loadings) {
					var engineData = dao.CreateEngineData(Engine);
					var cycle = new DrivingCycleData {
						Name = mission.ToString(),
						SavedInDeclarationMode = true,
						Entries = cycleParser.Parse(cycleData).ToList()
					};

					var simulationRunData = new VectoRunData {
						VehicleData = dao.CreateVehicleData(Vehicle, mission, loading),
						EngineData = engineData,
						GearboxData = dao.CreateGearboxData(Gearbox, engineData),
						Aux = dao.CreateAuxiliaryData(Aux, mission.MissionType, segment.VehicleClass),
						Cycle = cycle,
						IsEngineOnly = IsEngineOnly,
						JobFileName = Job.JobFile
					};
					yield return simulationRunData;
				}
			}
		}

		protected override void ProcessJob(VectoJobFile vectoJob)
		{
			var declaration = vectoJob as VectoJobFileV2Declaration;
			if (declaration == null) {
				return;
			}
			var job = declaration;

			ReadVehicle(Path.Combine(job.BasePath, job.Body.VehicleFile));

			ReadEngine(Path.Combine(job.BasePath, job.Body.EngineFile));

			ReadGearbox(Path.Combine(job.BasePath, job.Body.GearboxFile));

			ReadAuxiliary(job.Body.Aux);
		}

		protected override void ReadJobFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Job");

			switch (fileInfo.Version) {
				case 2:
					Job = JsonConvert.DeserializeObject<VectoJobFileV2Declaration>(json);
					Job.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					Job.JobFile = Path.GetFileName(file);
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported version of job-file. Got version " + fileInfo.Version);
			}
		}

		protected override void ReadVehicle(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Vehicle");

			switch (fileInfo.Version) {
				case 5:
					Vehicle = JsonConvert.DeserializeObject<VehicleFileV5Declaration>(json);
					Vehicle.BasePath = Path.GetDirectoryName(file);
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of vehicle-file. Got version " + fileInfo.Version);
			}
		}

		protected override void ReadEngine(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Engine");

			switch (fileInfo.Version) {
				case 2:
					Engine = JsonConvert.DeserializeObject<EngineFileV2Declaration>(json);
					Engine.BasePath = Path.GetDirectoryName(file);
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of engine-file. Got version " + fileInfo.Version);
			}
		}

		protected override void ReadGearbox(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForDeclarationMode(fileInfo, "Gearbox");

			switch (fileInfo.Version) {
				case 4:
					Gearbox = JsonConvert.DeserializeObject<GearboxFileV4Declaration>(json);
					Gearbox.BasePath = Path.GetDirectoryName(file);
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of gearbox-file. Got version " + fileInfo.Version);
			}
		}

		private void ReadAuxiliary(IEnumerable<VectoJobFileV2Declaration.DataBodyDecl.AuxDataDecl> auxiliaries)
		{
			// get all constant auxiliaries defined in Declaration mode
			// todo: catch exception when auxiliaries are not found
			// todo: catch exception when to much auxiliaries are present
			var aux = DeclarationData.AuxiliaryIDs().Select(id => {
				var a = auxiliaries.First(decl => decl.ID == id);
				return new VectoRunData.AuxData {
					ID = a.ID,
					Type = AuxiliaryTypeHelper.Parse(a.Type),
					Technology = a.Technology,
					TechList = a.TechList.ToArray(),
					DemandType = AuxiliaryDemandType.Constant
				};
			});

			// add a direct auxiliary
			aux = aux.Concat(new VectoRunData.AuxData { DemandType = AuxiliaryDemandType.Direct }.ToEnumerable());

			Aux = aux.ToArray();
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
	}
}