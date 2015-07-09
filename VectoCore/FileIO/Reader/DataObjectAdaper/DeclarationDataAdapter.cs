using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.Declaration;
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
			var fileV2Decl = engine as EngineFileV2Declaration;
			if (fileV2Decl != null) {
				return CreateEngineData(fileV2Decl);
			}
			throw new VectoException("Unsupported EngineData File Instance");
		}

		public override GearboxData CreateGearboxData(VectoGearboxFile gearbox, CombustionEngineData engine)
		{
			var fileV5Decl = gearbox as GearboxFileV4Declaration;
			if (fileV5Decl != null) {
				return CreateGearboxData(fileV5Decl, engine);
			}
			throw new VectoException("Unsupported GearboxData File Instance");
		}

		//==========================

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

		internal CombustionEngineData CreateEngineData(EngineFileV2Declaration engine)
		{
			var retVal = SetCommonCombustionEngineData(engine.Body, engine.BasePath);
			retVal.Inertia = DeclarationData.Engine.EngineInertia(retVal.Displacement);
			foreach (var entry in engine.Body.FullLoadCurves) {
				retVal.AddFullLoadCurve(entry.Gears, FullLoadCurve.ReadFromFile(Path.Combine(engine.BasePath, entry.Path), true));
			}

			return retVal;
		}

		internal GearboxData CreateGearboxData(GearboxFileV4Declaration gearbox, CombustionEngineData engine)
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


				if (i == 0) {
					retVal.AxleGearData = new GearData(lossMap, null, gearSettings.Ratio, false);
				} else {
					var shiftPolygon = DeclarationData.Gearbox.ComputeShiftPolygon(engine, i);
					retVal._gearData.Add(i, new GearData(lossMap, shiftPolygon, gearSettings.Ratio, false));
				}
			}

			return retVal;
		}
	}
}