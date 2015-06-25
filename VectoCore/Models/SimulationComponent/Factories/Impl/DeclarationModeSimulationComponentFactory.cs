using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl
{
	public class DeclarationModeSimulationComponentFactory : AbstractSimulationRunCreator
	{
		public const int PoweredAxle = 1;

		internal DeclarationModeSimulationComponentFactory() {}

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

		public override IEnumerable<IVectoRun> NextRun()
		{
			var segment = GetVehicleClassification((dynamic) Vehicle);
			foreach (var mission in segment.Missions) {
				foreach (var loading in mission.Loadings) {
					var jobData = new VectoJobData() {
						VehicleData = CreateVehicleData((dynamic) Vehicle, mission, loading),
						EngineData = CreateEngineData((dynamic) Engine),
						GearboxData = CreateGearboxData((dynamic) Gearbox),
					};
					//var builder = new SimulatorFactory.SimulatorBuilder();
				}
			}
			throw new NotImplementedException();
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
			var json = File.ReadAllText(Job.BasePath + file);
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
			var json = File.ReadAllText(Job.BasePath + file);
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
			var json = File.ReadAllText(Job.BasePath + file);
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
			var retVal = SetCommonVehicleData(data);

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
					TwinTyres = DeclarationData.Constants.Trailer.TwinTyres,
					RollResistanceCoefficient = DeclarationData.Constants.Trailer.RollResistanceCoefficient,
					TyreTestLoad = DeclarationData.Constants.Trailer.TyreTestLoad.SI<Newton>(),
					Inertia = DeclarationData.Wheels.Lookup(DeclarationData.Constants.Trailer.WheelsType).Inertia
				});
			}

			return retVal;
		}

		internal CombustionEngineData CreateEngineData(EngineFileV2Declaration engine)
		{
			var retVal = new CombustionEngineData();

			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV4Declaration gearbox)
		{
			var retVal = SetcommonGearboxData(gearbox.Body);

			if (retVal.Type == GearboxData.GearboxType.AutomaticTransmission) {
				throw new VectoSimulationException("Automatic Transmission currently not supported in DeclarationMode!");
			}
			if (retVal.Type == GearboxData.GearboxType.Custom) {
				throw new VectoSimulationException("Custom Transmission not supported in DeclarationMode!");
			}
			retVal.Inertia = DeclarationData.Constants.Gearbox.Inertia.SI<KilogramSquareMeter>();
			retVal.TractionInterruption = DeclarationData.Constants.Gearbox.TractionInterruption(retVal.Type);
			retVal.SkipGears = DeclarationData.Constants.Gearbox.SkipGears(retVal.Type);
			retVal.EarlyShiftUp = DeclarationData.Constants.Gearbox.EarlyShiftGears((retVal.Type));

			retVal.TorqueReserve = DeclarationData.Constants.Gearbox.TorqueReserve;
			retVal.StartTorqueReserve = DeclarationData.Constants.Gearbox.TorqueReserveStart;
			retVal.ShiftTime = DeclarationData.Constants.Gearbox.MinTimeBetweenGearshifts.SI<Second>();
			retVal.StartSpeed = DeclarationData.Constants.Gearbox.StartSpeed.SI<MeterPerSecond>();
			retVal.StartAcceleration = DeclarationData.Constants.Gearbox.StartAcceleration.SI<MeterPerSquareSecond>();

			retVal.HasTorqueConverter = false;

			//retVal.g

			return retVal;
		}
	}
}