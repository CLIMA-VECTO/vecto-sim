using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper
{
	public class DeclarationDataAdapter : AbstractSimulationDataAdapter
	{
		public override VehicleData CreateVehicleData(VectoVehicleFile vehicle, Mission segment, Kilogram loading)
		{
			var fileV5Decl = vehicle as VehicleFileV5Declaration;
			if (fileV5Decl != null) {
				return CreateVehicleData(fileV5Decl, segment, loading);
			}
			throw new VectoException("Unsupported VehicleData File Instance");
		}

		public override VehicleData CreateVehicleData(VectoVehicleFile vehicle)
		{
			var fileV5Decl = vehicle as VehicleFileV5Declaration;
			if (fileV5Decl != null) {
				return SetCommonVehicleData(fileV5Decl.Body, fileV5Decl.BasePath);
			}
			throw new VectoException("Mission Data and loading required in DeclarationMode");
		}

		public override CombustionEngineData CreateEngineData(VectoEngineFile engine)
		{
			var fileV2Decl = engine as EngineFileV3Declaration;
			if (fileV2Decl != null) {
				return CreateEngineData(fileV2Decl);
			}
			throw new VectoException("Unsupported EngineData File Instance");
		}

		public override GearboxData CreateGearboxData(VectoGearboxFile gearbox, CombustionEngineData engine)
		{
			var fileV5Decl = gearbox as GearboxFileV5Declaration;
			if (fileV5Decl != null) {
				return CreateGearboxData(fileV5Decl, engine);
			}
			throw new VectoException("Unsupported GearboxData File Instance");
		}

		public override DriverData CreateDriverData(VectoJobFile job)
		{
			var fileV2Decl = job as VectoJobFileV2Declaration;
			if (fileV2Decl != null) {
				return CreateDriverData(fileV2Decl);
			}
			throw new VectoException("Unsupported Job File Instance");
		}

		//==========================


		public DriverData CreateDriverData(VectoJobFileV2Declaration job)
		{
			var data = job.Body;

			var lookAheadData = new DriverData.LACData() {
				Enabled = DeclarationData.Driver.LookAhead.Enabled,
				Deceleration = DeclarationData.Driver.LookAhead.Deceleration,
				MinSpeed = DeclarationData.Driver.LookAhead.MinimumSpeed
			};
			var overspeedData = new DriverData.OverSpeedEcoRollData() {
				Mode = DriverData.ParseDriverMode(data.OverSpeedEcoRoll.Mode),
				MinSpeed = DeclarationData.Driver.OverSpeedEcoRoll.MinSpeed,
				OverSpeed = DeclarationData.Driver.OverSpeedEcoRoll.OverSpeed,
				UnderSpeed = DeclarationData.Driver.OverSpeedEcoRoll.UnderSpeed
			};
			if (!DeclarationData.Driver.OverSpeedEcoRoll.AllowedModes.Contains(overspeedData.Mode)) {
				throw new VectoSimulationException(
					String.Format("Specified Overspeed/EcoRoll Mode not allowed in declaration mode! {0}", overspeedData.Mode));
			}
			var startstopData = new VectoRunData.StartStopData() {
				Enabled = data.StartStop.Enabled,
				Delay = DeclarationData.Driver.StartStop.Delay,
				MinTime = DeclarationData.Driver.StartStop.MinTime,
				MaxSpeed = DeclarationData.Driver.StartStop.MaxSpeed,
			};
			var retVal = new DriverData() {
				LookAheadCoasting = lookAheadData,
				OverSpeedEcoRoll = overspeedData,
				StartStop = startstopData,
			};
			return retVal;
		}


		internal VehicleData CreateVehicleData(VehicleFileV5Declaration vehicle, Mission mission, Kilogram loading)
		{
			var data = vehicle.Body;
			var retVal = SetCommonVehicleData(data, vehicle.BasePath);

			retVal.BasePath = vehicle.BasePath;

			retVal.GrossVehicleMassRating = vehicle.Body.GrossVehicleMassRating.SI<Ton>().Cast<Kilogram>();

			retVal.CurbWeigthExtra = mission.MassExtra;
			retVal.Loading = loading;
			retVal.DynamicTyreRadius =
				DeclarationData.DynamicTyreRadius(data.AxleConfig.Axles[DeclarationData.PoweredAxle()].WheelsStr, data.RimStr);

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
					TyreTestLoad = axleInput.TyreTestLoad.SI<Newton>(),
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

		internal CombustionEngineData CreateEngineData(EngineFileV3Declaration engine)
		{
			var retVal = SetCommonCombustionEngineData(engine.Body, engine.BasePath);
			retVal.Inertia = DeclarationData.Engine.EngineInertia(retVal.Displacement);
			retVal.FullLoadCurve = EngineFullLoadCurve.ReadFromFile(Path.Combine(engine.BasePath, engine.Body.FullLoadCurve),
				true);

			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV5Declaration gearbox, CombustionEngineData engine)
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
				var gearSettings = gearbox.Body.Gears[(int)i];
				var lossMapPath = Path.Combine(gearbox.BasePath, gearSettings.LossMap);
				var lossMap = TransmissionLossMap.ReadFromFile(lossMapPath, gearSettings.Ratio);


				if (i == 0) {
					retVal.AxleGearData = new GearData() {
						LossMap = lossMap,
						Ratio = gearSettings.Ratio,
						TorqueConverterActive = false
					};
				} else {
					var fullLoad = !String.IsNullOrEmpty(gearSettings.FullLoadCurve) && gearSettings.FullLoadCurve.Equals("<NOFILE>")
						? GearFullLoadCurve.ReadFromFile(Path.Combine(gearbox.BasePath, gearSettings.FullLoadCurve))
						: null;
					var shiftPolygon = DeclarationData.Gearbox.ComputeShiftPolygon(fullLoad, engine);

					retVal._gearData.Add(i, new GearData() {
						LossMap = lossMap,
						ShiftPolygon = shiftPolygon,
						Ratio = gearSettings.Ratio,
						TorqueConverterActive = false
					});
				}
			}
			return retVal;
		}
	}
}