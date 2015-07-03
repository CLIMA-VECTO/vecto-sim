using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public class DeclarationModeSimulationDataReader : AbstractSimulationDataReader
	{
		public const int PoweredAxle = 1;

		internal DeclarationModeSimulationDataReader() {}

		//public void SetJobFile(string fileName)
		//{
		//	var file = File.ReadAllText(fileName);
		//	SetJobJson(file, Path.GetDirectoryName(fileName));
		//}

		//public void SetJobJson(string file, string basePath)
		//{
		//	var fileInfo = GetFileVersion(file);

		//	if (!fileInfo.Item2) {
		//		throw new VectoException("File not saved in Declaration Mode!");
		//	}
		//	switch (fileInfo.Item1) {
		//		case 2:
		//			break;
		//		default:
		//			throw new UnsupportedFileVersionException("Unsupported version of .vecto file/data. Got Version " + fileInfo.Item1);
		//	}
		//}

		//protected RetarderData RetarderData;

		protected void CheckForDeclarationMode(InputFileReader.VersionInfo info, string msg)
		{
			if (!info.SavedInDeclarationMode) {
				throw new VectoException("File not saved in Declaration Mode! - " + msg);
			}
		}

		public override IEnumerable<VectoRunData> NextRun()
		{
			var segment = GetVehicleClassification((dynamic) Vehicle);
			foreach (var mission in segment.Missions) {
				foreach (var loading in mission.Loadings) {
					var jobData = new VectoRunData() {
						VehicleData = CreateVehicleData((dynamic) Vehicle, mission, loading),
						EngineData = CreateEngineData((dynamic) Engine),
						GearboxData = CreateGearboxData((dynamic) Gearbox),
						// @@@ TODO: cycle
						// @@@ TODO: auxiliaries
						// @@@ TODO: ...
						IsEngineOnly = false,
						JobFileName = Job.JobFile,
					};
					yield return jobData;
					//var builder = new SimulatorFactory.PowertrainBuilder();
				}
			}
			//throw new NotImplementedException();
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
					break;
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of engine-file. Got version " + fileInfo.Version);
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
					break;
				default:
					throw new UnsupportedFileVersionException("Unsopported Version of gearbox-file. Got version " + fileInfo.Version);
			}
		}


		internal Segment GetVehicleClassification(VehicleFileV5Declaration.DataBodyDecl vehicle)
		{
			return new DeclarationSegments().Lookup(vehicle.VehicleCategory(), vehicle.AxleConfigurationType(),
				vehicle.GrossVehicleMassRating.SI<Kilogram>(), vehicle.CurbWeight.SI<Kilogram>());
		}


		internal VehicleData CreateVehicleData(VehicleFileV5Declaration vehicle, Mission mission, Kilogram loading)
		{
			var data = vehicle.Body;
			var retVal = SetCommonVehicleData(vehicle);

			retVal.BasePath = vehicle.BasePath;

			retVal.CurbWeigthExtra = mission.MassExtra;
			retVal.Loading = loading;
			retVal.DynamicTyreRadius =
				DeclarationData.DynamicTyreRadius(data.AxleConfig.Axles[PoweredAxle].WheelsStr, data.RimStr);

			if (data.AxleConfig.Axles.Count < mission.AxleWeightDistribution.Length) {
				throw new VectoException(
					String.Format("Vehicle does not contain sufficient axles. {0} axles defined, {1} axles required",
						data.AxleConfig.Axles.Count, mission.AxleWeightDistribution.Count()));
			}
			retVal.AxleData = new List<Axle>();
			for (var i = 0; i < mission.AxleWeightDistribution.Length; i++) {
				var axleInput = data.AxleConfig.Axles[i];
				var axle = new Axle {
					AxleWeightShare = mission.AxleWeightDistribution[i],
					TwinTyres = axleInput.TwinTyres,
					RollResistanceCoefficient = axleInput.RollResistanceCoefficient,
					TyreTestLoad = DoubleExtensionMethods.SI<Newton>(axleInput.TyreTestLoad),
					Inertia = DeclarationData.Wheels.Lookup(axleInput.WheelsStr).Inertia,
				};
				retVal.AxleData.Add(axle);
			}

			foreach (var tmp in mission.TrailerAxleWeightDistribution) {
				retVal.AxleData.Add(new Axle() {
					AxleWeightShare = tmp,
					TwinTyres = DeclarationData.Trailer.TwinTyres,
					RollResistanceCoefficient = DeclarationData.Trailer.RollResistanceCoefficient,
					TyreTestLoad = DeclarationData.Trailer.TyreTestLoad.SI<Newton>(),
					Inertia = DeclarationData.Wheels.Lookup(DeclarationData.Trailer.WheelsType).Inertia
				});
			}

			return retVal;
		}

		internal CombustionEngineData CreateEngineData(EngineFileV2Declaration engine)
		{
			var retVal = SetCommonCombustionEngineData(engine);
			retVal.Inertia = DeclarationData.Engine.EngineInertia(retVal.Displacement);
			foreach (var entry in engine.Body.FullLoadCurves) {
				retVal.AddFullLoadCurve(entry.Gears, FullLoadCurve.ReadFromFile(Path.Combine(engine.BasePath, entry.Path), true));
			}

			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV4Declaration gearbox)
		{
			var retVal = SetCommonGearboxData(gearbox.Body);

			if (retVal.Type == GearboxData.GearboxType.AT) {
				throw new VectoSimulationException("Automatic Transmission currently not supported in DeclarationMode!");
			}
			if (retVal.Type == GearboxData.GearboxType.Custom) {
				throw new VectoSimulationException("Custom Transmission not supported in DeclarationMode!");
			}
			retVal.Inertia = DeclarationData.Gearbox.Inertia.SI<KilogramSquareMeter>();
			retVal.TractionInterruption = DeclarationData.Gearbox.TractionInterruption(retVal.Type);
			retVal.SkipGears = DeclarationData.Gearbox.SkipGears(retVal.Type);
			retVal.EarlyShiftUp = DeclarationData.Gearbox.EarlyShiftGears((retVal.Type));

			retVal.TorqueReserve = DeclarationData.Gearbox.TorqueReserve;
			retVal.StartTorqueReserve = DeclarationData.Gearbox.TorqueReserveStart;
			retVal.ShiftTime = DeclarationData.Gearbox.MinTimeBetweenGearshifts.SI<Second>();
			retVal.StartSpeed = DeclarationData.Gearbox.StartSpeed.SI<MeterPerSecond>();
			retVal.StartAcceleration = DeclarationData.Gearbox.StartAcceleration.SI<MeterPerSquareSecond>();

			retVal.HasTorqueConverter = false;


			for (uint i = 0; i < gearbox.Body.Gears.Count; i++) {
				var gearSettings = gearbox.Body.Gears[(int) i];
				var lossMapPath = Path.Combine(gearbox.BasePath, gearSettings.LossMap);
				TransmissionLossMap lossMap = TransmissionLossMap.ReadFromFile(lossMapPath, gearSettings.Ratio);

				var shiftPolygon = ComputeShiftPolygon();

				var gear = new GearData(lossMap, shiftPolygon, gearSettings.Ratio, false);
				if (i == 0) {
					retVal.AxleGearData = gear;
				} else {
					retVal._gearData.Add(i, gear);
				}
			}

			return retVal;
		}

		internal ShiftPolygon ComputeShiftPolygon()
		{
			throw new NotImplementedException();
		}
	}
}