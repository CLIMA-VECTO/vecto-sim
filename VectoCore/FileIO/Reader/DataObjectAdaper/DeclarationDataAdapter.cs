using System.Collections.Generic;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
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
		public override VehicleData CreateVehicleData(VectoVehicleFile vehicle, Mission mission, Kilogram loading)
		{
			var fileV5Decl = vehicle as VehicleFileV7Declaration;
			if (fileV5Decl != null) {
				return CreateVehicleData(fileV5Decl, mission, loading);
			}
			throw new VectoException("Unsupported VehicleData File Instance");
		}

		public override VehicleData CreateVehicleData(VectoVehicleFile vehicle)
		{
			var fileV7Decl = vehicle as VehicleFileV7Declaration;
			if (fileV7Decl != null) {
				return SetCommonVehicleData(fileV7Decl.Body, fileV7Decl.BasePath);
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

			var lookAheadData = new DriverData.LACData {
				Enabled = DeclarationData.Driver.LookAhead.Enabled,
				Deceleration = DeclarationData.Driver.LookAhead.Deceleration,
				MinSpeed = DeclarationData.Driver.LookAhead.MinimumSpeed
			};
			var overspeedData = new DriverData.OverSpeedEcoRollData {
				Mode = DriverData.ParseDriverMode(data.OverSpeedEcoRoll.Mode),
				MinSpeed = DeclarationData.Driver.OverSpeedEcoRoll.MinSpeed,
				OverSpeed = DeclarationData.Driver.OverSpeedEcoRoll.OverSpeed,
				UnderSpeed = DeclarationData.Driver.OverSpeedEcoRoll.UnderSpeed
			};
			if (!DeclarationData.Driver.OverSpeedEcoRoll.AllowedModes.Contains(overspeedData.Mode)) {
				throw new VectoSimulationException("Specified Overspeed/EcoRoll Mode not allowed in declaration mode! {0}",
					overspeedData.Mode);
			}
			var startstopData = new VectoRunData.StartStopData {
				Enabled = data.StartStop.Enabled,
				Delay = DeclarationData.Driver.StartStop.Delay,
				MinTime = DeclarationData.Driver.StartStop.MinTime,
				MaxSpeed = DeclarationData.Driver.StartStop.MaxSpeed,
			};
			var retVal = new DriverData {
				LookAheadCoasting = lookAheadData,
				OverSpeedEcoRoll = overspeedData,
				StartStop = startstopData,
			};
			return retVal;
		}


		internal VehicleData CreateVehicleData(VehicleFileV7Declaration vehicle, Mission mission, Kilogram loading)
		{
			var data = vehicle.Body;
			var retVal = SetCommonVehicleData(data, vehicle.BasePath);

			retVal.BasePath = vehicle.BasePath;

			retVal.GrossVehicleMassRating = vehicle.Body.GrossVehicleMassRating.SI<Ton>().Cast<Kilogram>();

			retVal.CurbWeigthExtra = mission.MassExtra;
			retVal.Loading = loading;
			retVal.DynamicTyreRadius =
				DeclarationData.DynamicTyreRadius(data.AxleConfig.Axles[DeclarationData.PoweredAxle()].WheelsStr, data.RimStr);

			retVal.CrossWindCorrectionMode = CrossWindCorrectionMode.DeclarationModeCorrection;
			retVal.AerodynamicDragAera = mission.UseCdA2
				? data.DragCoefficientRigidTruck.SI<SquareMeter>()
				: data.DragCoefficient.SI<SquareMeter>();

			if (data.AxleConfig.Axles.Count < mission.AxleWeightDistribution.Length) {
				throw new VectoException(
					string.Format("Vehicle does not contain sufficient axles. {0} axles defined, {1} axles required",
						data.AxleConfig.Axles.Count, mission.AxleWeightDistribution.Count()));
			}
			var axleData = new List<Axle>();
			for (var i = 0; i < mission.AxleWeightDistribution.Length; i++) {
				var axleInput = data.AxleConfig.Axles[i];
				var axle = new Axle {
					AxleWeightShare = mission.AxleWeightDistribution[i],
					TwinTyres = axleInput.TwinTyres,
					RollResistanceCoefficient = axleInput.RollResistanceCoefficient,
					TyreTestLoad = axleInput.TyreTestLoad.SI<Newton>(),
					Inertia = DeclarationData.Wheels.Lookup(axleInput.WheelsStr.Replace(" ", "")).Inertia,
				};
				axleData.Add(axle);
			}

			axleData.AddRange(mission.TrailerAxleWeightDistribution.Select(tmp => new Axle {
				AxleWeightShare = tmp,
				TwinTyres = DeclarationData.Trailer.TwinTyres,
				RollResistanceCoefficient = DeclarationData.Trailer.RollResistanceCoefficient,
				TyreTestLoad = DeclarationData.Trailer.TyreTestLoad.SI<Newton>(),
				Inertia = DeclarationData.Wheels.Lookup(DeclarationData.Trailer.WheelsType).Inertia
			}));
			retVal.AxleData = axleData;
			return retVal;
		}

		internal CombustionEngineData CreateEngineData(EngineFileV3Declaration engine)
		{
			var retVal = SetCommonCombustionEngineData(engine.Body, engine.BasePath);
			retVal.Inertia = DeclarationData.Engine.EngineInertia(retVal.Displacement);
			retVal.FullLoadCurve = EngineFullLoadCurve.ReadFromFile(Path.Combine(engine.BasePath, engine.Body.FullLoadCurve),
				true);
			retVal.FullLoadCurve.EngineData = retVal;
			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV5Declaration gearbox, CombustionEngineData engine)
		{
			var retVal = SetCommonGearboxData(gearbox.Body);
			switch (retVal.Type) {
				case GearboxType.AT:
					throw new VectoSimulationException("Automatic Transmission currently not supported in DeclarationMode!");
				case GearboxType.Custom:
					throw new VectoSimulationException("Custom Transmission not supported in DeclarationMode!");
			}
			if (gearbox.Body.Gears.Count < 2) {
				throw new VectoSimulationException(
					"At least two Gear-Entries must be defined in Gearbox: 1 Axle-Gear and at least 1 Gearbox-Gear!");
			}

			retVal.Inertia = DeclarationData.Gearbox.Inertia.SI<KilogramSquareMeter>();
			retVal.TractionInterruption = retVal.Type.TractionInterruption();
			retVal.SkipGears = retVal.Type.SkipGears();
			retVal.EarlyShiftUp = retVal.Type.EarlyShiftGears();

			retVal.TorqueReserve = DeclarationData.Gearbox.TorqueReserve;
			retVal.StartTorqueReserve = DeclarationData.Gearbox.TorqueReserveStart;
			retVal.ShiftTime = DeclarationData.Gearbox.MinTimeBetweenGearshifts.SI<Second>();
			retVal.StartSpeed = DeclarationData.Gearbox.StartSpeed.SI<MeterPerSecond>();
			retVal.StartAcceleration = DeclarationData.Gearbox.StartAcceleration.SI<MeterPerSquareSecond>();

			retVal.HasTorqueConverter = false;

			var axleGear = gearbox.Body.Gears.First();
			var axleLossMap = TransmissionLossMap.ReadFromFile(Path.Combine(gearbox.BasePath, axleGear.LossMap), axleGear.Ratio);
			retVal.AxleGearData = new GearData { LossMap = axleLossMap, Ratio = axleGear.Ratio, TorqueConverterActive = false };

			retVal.Gears = gearbox.Body.Gears.Skip(1).Select((gear, i) => {
				var gearLossMap = TransmissionLossMap.ReadFromFile(Path.Combine(gearbox.BasePath, gear.LossMap), gear.Ratio);
				var gearFullLoad = (string.IsNullOrWhiteSpace(gear.FullLoadCurve) || gear.FullLoadCurve == "<NOFILE>")
					? engine.FullLoadCurve
					: FullLoadCurve.ReadFromFile(Path.Combine(gearbox.BasePath, gear.FullLoadCurve));

				var fullLoadCurve = IntersectFullLoadCurves(engine.FullLoadCurve, gearFullLoad);
				var shiftPolygon = DeclarationData.Gearbox.ComputeShiftPolygon(fullLoadCurve, engine.IdleSpeed);
				return new KeyValuePair<uint, GearData>((uint)i + 1,
					new GearData {
						LossMap = gearLossMap,
						ShiftPolygon = shiftPolygon,
						FullLoadCurve = gearFullLoad ?? engine.FullLoadCurve,
						Ratio = gear.Ratio,
						TorqueConverterActive = false
					});
			}).ToDictionary(kv => kv.Key, kv => kv.Value);
			return retVal;
		}

		/// <summary>
		/// Intersects full load curves.
		/// </summary>
		/// <param name="curves">full load curves</param>
		/// <returns>A combined EngineFullLoadCurve with the minimum full load torque over all inputs curves.</returns>
		private static EngineFullLoadCurve IntersectFullLoadCurves(EngineFullLoadCurve engineCurve, FullLoadCurve gearCurve)
		{
			var entries = gearCurve.FullLoadEntries.Concat(engineCurve.FullLoadEntries)
				.Select(entry => entry.EngineSpeed)
				.OrderBy(engineSpeed => engineSpeed)
				.Distinct()
				.Select(engineSpeed => new FullLoadCurve.FullLoadCurveEntry {
					EngineSpeed = engineSpeed,
					TorqueFullLoad =
						VectoMath.Min(engineCurve.FullLoadStationaryTorque(engineSpeed), gearCurve.FullLoadStationaryTorque(engineSpeed))
				});

			var flc = new EngineFullLoadCurve {
				FullLoadEntries = entries.ToList(),
				EngineData = engineCurve.EngineData,
				PT1Data = engineCurve.PT1Data
			};
			return flc;
		}

		public IEnumerable<VectoRunData.AuxData> CreateAuxiliaryData(IEnumerable<VectoRunData.AuxData> auxList,
			MissionType mission, VehicleClass hvdClass)
		{
			foreach (var auxData in auxList) {
				var aux = new VectoRunData.AuxData { DemandType = AuxiliaryDemandType.Constant };

				switch (auxData.Type) {
					case AuxiliaryType.Fan:
						aux.PowerDemand = DeclarationData.Fan.Lookup(mission, auxData.Technology);
						aux.ID = Constants.Auxiliaries.IDs.Fan;
						break;
					case AuxiliaryType.SteeringPump:
						aux.PowerDemand = DeclarationData.SteeringPump.Lookup(mission, hvdClass, auxData.Technology);
						aux.ID = Constants.Auxiliaries.IDs.SteeringPump;
						break;
					case AuxiliaryType.HeatingVentilationAirCondition:
						aux.PowerDemand = DeclarationData.HeatingVentilationAirConditioning.Lookup(mission, hvdClass);
						aux.ID = Constants.Auxiliaries.IDs.HeatingVentilationAirCondition;
						break;
					case AuxiliaryType.PneumaticSystem:
						aux.PowerDemand = DeclarationData.PneumaticSystem.Lookup(mission, hvdClass);
						aux.ID = Constants.Auxiliaries.IDs.PneumaticSystem;
						break;
					case AuxiliaryType.ElectricSystem:
						aux.PowerDemand = DeclarationData.ElectricSystem.Lookup(mission, auxData.TechList);
						aux.ID = Constants.Auxiliaries.IDs.ElectricSystem;
						break;
				}
				yield return aux;
			}
		}
	}
}