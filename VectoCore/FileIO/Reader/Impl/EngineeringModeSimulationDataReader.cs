using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper;
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
		protected DriverData Driver;

		internal EngineeringModeSimulationDataReader() {}


		protected static void CheckForEngineeringMode(VersionInfo info, string msg)
		{
			if (info.SavedInDeclarationMode) {
				LogManager.GetLogger(typeof(EngineeringModeSimulationDataReader))
					.WarnFormat("File was saved in Declaration Mode but is used for Engineering Mode!");
			}
		}

		protected override void ProcessJob(VectoJobFile vectoJob)
		{
			var engineering = vectoJob as VectoJobFileV2Engineering;
			if (engineering == null) {
				throw new VectoException("Unhandled Job File Format");
			}
			var job = engineering;

			if (job.Body.EngineOnlyMode) {
				throw new VectoException("Job File has been saved in EngineOnlyMode!");
			}

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
					Vehicle.BasePath = Path.GetDirectoryName(file);
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Version);
			}
		}

		/// <summary>
		/// Iterate over all cycles defined in the JobFile and create a container with all data required for creating a simulation run
		/// </summary>
		/// <returns>VectoRunData instance for initializing the powertrain.</returns>
		public override IEnumerable<VectoRunData> NextRun()
		{
			var job = Job as VectoJobFileV2Engineering;
			if (job == null) {
				Log.Warn("Job-file is null or unsupported version");
				yield break;
			}
			var dao = new EngineeringDataAdapter();
			var driver = dao.CreateDriverData(job);
			foreach (var cycle in job.Body.Cycles) {
				var simulationRunData = new VectoRunData() {
					BasePath = job.BasePath,
					JobFileName = job.JobFile,
					EngineData = dao.CreateEngineData(Engine),
					GearboxData = dao.CreateGearboxData(Gearbox, null),
					VehicleData = dao.CreateVehicleData(Vehicle),
					DriverData = driver,
					//Aux = 
					// TODO: distance or time-based cycle!
					Cycle =
						DrivingCycleDataReader.ReadFromFile(Path.Combine(job.BasePath, cycle), DrivingCycleData.CycleType.DistanceBased),
					IsEngineOnly = IsEngineOnly
				};
				yield return simulationRunData;
			}
		}

		/// <summary>
		/// create VehicleData instance directly from a file
		/// </summary>
		/// <param name="file">filename</param>
		/// <returns>VehicleData instance</returns>
		public static VehicleData CreateVehicleDataFromFile(string file)
		{
			var data = DoReadVehicleFile(file);
			return new EngineeringDataAdapter().CreateVehicleData(data);
		}

		public static DriverData CreateDriverDataFromFile(string file)
		{
			var data = DoReadJobFile(file);
			return new EngineeringDataAdapter().CreateDriverData(data);
		}

		/// <summary>
		/// Create CombustionEngineData instance directly from a file
		/// </summary>
		/// <param name="file">filename</param>
		/// <returns>CombustionengineData instance</returns>
		public static CombustionEngineData CreateEngineDataFromFile(string file)
		{
			var data = DoReadEngineFile(file);
			return new EngineeringDataAdapter().CreateEngineData(data);
		}

		/// <summary>
		/// Create gearboxdata instance directly from a file
		/// </summary>
		/// <param name="file">filename</param>
		/// <returns>GearboxData instance</returns>
		public static GearboxData CreateGearboxDataFromFile(string file)
		{
			var data = DoReadGearboxFile(file);
			return new EngineeringDataAdapter().CreateGearboxData(data, null);
		}


		/// <summary>
		/// initialize vecto job member (deserialize Vecot-file
		/// </summary>
		/// <param name="file"></param>
		protected override void ReadJobFile(string file)
		{
			Job = DoReadJobFile(file);
		}

		/// <summary>
		/// initialize Engine member (deserialize Engine-file)
		/// </summary>
		/// <param name="file"></param>
		protected override void ReadEngine(string file)
		{
			Engine = DoReadEngineFile(file);
		}

		/// <summary>
		/// initialize Gearbox member (deserialize Gearbox-file)
		/// </summary>
		/// <param name="file"></param>
		protected override void ReadGearbox(string file)
		{
			Gearbox = DoReadGearboxFile(file);
		}

		//==============================================================

		/// <summary>
		/// initialize Job member (deserialize Job-file)
		/// </summary>
		/// <param name="file">file</param>
		protected static VectoJobFile DoReadJobFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Job");

			switch (fileInfo.Version) {
				case 2:
					var tmp = JsonConvert.DeserializeObject<VectoJobFileV2Engineering>(json);
					tmp.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					tmp.JobFile = Path.GetFileName(file);
					return tmp;
				default:
					throw new UnsupportedFileVersionException("Unsupported version of job-file. Got version " + fileInfo.Version);
			}
		}

		/// <summary>
		/// De-serialize engine-file (JSON)
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		protected static VectoEngineFile DoReadEngineFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Engine");

			switch (fileInfo.Version) {
				case 3:
					var tmp = JsonConvert.DeserializeObject<EngineFileV3Engineering>(json);
					tmp.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					return tmp;
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of engine-file. Got version " + fileInfo.Version);
			}
		}

		/// <summary>
		/// De-serialize gearbox-file (JSON)
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		protected static VectoGearboxFile DoReadGearboxFile(string file)
		{
			var json = File.ReadAllText(file);
			var fileInfo = GetFileVersion(json);
			CheckForEngineeringMode(fileInfo, "Gearbox");

			switch (fileInfo.Version) {
				case 5:
					var tmp = JsonConvert.DeserializeObject<GearboxFileV5Engineering>(json);
					tmp.BasePath = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
					return tmp;
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of gearbox-file. Got version " + fileInfo.Version);
			}
		}

		/// <summary>
		/// De-serialize vehicle-file (JSON)
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
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

		public override bool IsEngineOnly
		{
			get { return false; }
		}
	}
}