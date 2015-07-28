using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper
{
	public class EngineeringDataAdapter : AbstractSimulationDataAdapter
	{
		public override VehicleData CreateVehicleData(VectoVehicleFile vehicle, Models.Declaration.Mission segment,
			Kilogram loading)
		{
			return CreateVehicleData(vehicle);
		}

		public override VehicleData CreateVehicleData(VectoVehicleFile vehicle)
		{
			var fileV5Eng = vehicle as VehicleFileV5Engineering;
			if (fileV5Eng != null) {
				return CreateVehicleData(fileV5Eng);
			}
			throw new VectoException("Unsupported EngineData File Instance");
		}

		public override CombustionEngineData CreateEngineData(VectoEngineFile engine)
		{
			var fileV2Eng = engine as EngineFileV3Engineering;
			if (fileV2Eng != null) {
				return CreateEngineData(fileV2Eng);
			}
			throw new VectoException("Unsupported EngineData File Instance");
		}

		public override GearboxData CreateGearboxData(VectoGearboxFile gearbox, CombustionEngineData engine)
		{
			var fileV5Eng = gearbox as GearboxFileV5Engineering;
			if (fileV5Eng != null) {
				return CreateGearboxData(fileV5Eng);
			}
			throw new VectoException("Unsupported GearboxData File Instance");
		}

		public override DriverData CreateDriverData(VectoJobFile job)
		{
			var filev2Eng = job as VectoJobFileV2Engineering;
			if (filev2Eng != null) {
				return CreateDriverData(filev2Eng);
			}
			throw new VectoException("Unsupported Job File Instance");
		}

		//=================================


		internal DriverData CreateDriverData(VectoJobFileV2Engineering job)
		{
			var data = job.Body;

			var accelerationData = AccelerationCurveData.ReadFromFile(Path.Combine(job.BasePath, data.AccelerationCurve));
			var lookAheadData = new DriverData.LACData() {
				Enabled = data.LookAheadCoasting.Enabled,
				Deceleration = DoubleExtensionMethods.SI<MeterPerSquareSecond>(data.LookAheadCoasting.Dec),
				MinSpeed = data.LookAheadCoasting.MinSpeed.KMPHtoMeterPerSecond(),
			};
			var overspeedData = new DriverData.OverSpeedEcoRollData() {
				Mode = DriverData.ParseDriverMode(data.OverSpeedEcoRoll.Mode),
				MinSpeed = data.OverSpeedEcoRoll.MinSpeed.KMPHtoMeterPerSecond(),
				OverSpeed = data.OverSpeedEcoRoll.OverSpeed.KMPHtoMeterPerSecond(),
				UnderSpeed = data.OverSpeedEcoRoll.UnderSpeed.KMPHtoMeterPerSecond(),
			};
			var startstopData = new VectoRunData.StartStopData() {
				Enabled = data.StartStop.Enabled,
				Delay = data.StartStop.Delay.SI<Second>(),
				MinTime = data.StartStop.MinTime.SI<Second>(),
				MaxSpeed = data.StartStop.MaxSpeed.KMPHtoMeterPerSecond(),
			};
			var retVal = new DriverData() {
				AccelerationCurve = accelerationData,
				LookAheadCoasting = lookAheadData,
				OverSpeedEcoRoll = overspeedData,
				StartStop = startstopData,
			};
			return retVal;
		}

		/// <summary>
		/// convert datastructure representing file-contents into internal datastructure
		/// Vehicle, file-format version 5
		/// </summary>
		/// <param name="vehicle">VehicleFileV5 container</param>
		/// <returns>VehicleData instance</returns>
		internal VehicleData CreateVehicleData(VehicleFileV5Engineering vehicle)
		{
			var data = vehicle.Body;

			var retVal = SetCommonVehicleData(data, vehicle.BasePath);

			retVal.BasePath = vehicle.BasePath;
			retVal.CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>();
			retVal.Loading = data.Loading.SI<Kilogram>();
			retVal.DynamicTyreRadius = data.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>();

			retVal.AxleData = data.AxleConfig.Axles.Select(axle => new Axle {
				Inertia = DoubleExtensionMethods.SI<KilogramSquareMeter>(axle.Inertia),
				TwinTyres = axle.TwinTyres,
				RollResistanceCoefficient = axle.RollResistanceCoefficient,
				AxleWeightShare = axle.AxleWeightShare,
				TyreTestLoad = axle.TyreTestLoad.SI<Newton>(),
				//Wheels = axle.WheelsStr
			}).ToList();
			return retVal;
		}


		/// <summary>
		/// convert datastructure representing the file-contents into internal data structure
		/// Engine, file-format version 2
		/// </summary>
		/// <param name="engine">Engin-Data file (Engineering mode)</param>
		/// <returns></returns>
		internal CombustionEngineData CreateEngineData(EngineFileV3Engineering engine)
		{
			var retVal = SetCommonCombustionEngineData(engine.Body, engine.BasePath);
			retVal.Inertia = engine.Body.Inertia.SI<KilogramSquareMeter>();
			retVal.FullLoadCurve = EngineFullLoadCurve.ReadFromFile(Path.Combine(engine.BasePath, engine.Body.FullLoadCurve),
				false);
			retVal.FullLoadCurve.EngineData = retVal;
			return retVal;
		}


		/// <summary>
		/// convert datastructure representing the file-contents into internal data structure
		/// Gearbox, File-format Version 4
		/// </summary>
		/// <param name="gearbox"></param>
		/// <returns></returns>
		internal GearboxData CreateGearboxData(GearboxFileV5Engineering gearbox)
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
				var gearSettings = gearbox.Body.Gears[(int)i];
				var lossMapPath = Path.Combine(gearbox.BasePath, gearSettings.LossMap);
				TransmissionLossMap lossMap = TransmissionLossMap.ReadFromFile(lossMapPath, gearSettings.Ratio);

				var shiftPolygon = !String.IsNullOrEmpty(gearSettings.ShiftPolygon)
					? ShiftPolygon.ReadFromFile(Path.Combine(gearbox.BasePath, gearSettings.ShiftPolygon))
					: null;
				var fullLoad = !String.IsNullOrEmpty(gearSettings.FullLoadCurve) && !gearSettings.FullLoadCurve.Equals("<NOFILE>")
					? GearFullLoadCurve.ReadFromFile(Path.Combine(gearbox.BasePath, gearSettings.FullLoadCurve))
					: null;

				var gear = new GearData() {
					LossMap = lossMap,
					ShiftPolygon = shiftPolygon,
					FullLoadCurve = fullLoad,
					Ratio = gearSettings.Ratio,
					TorqueConverterActive = gearSettings.TCactive
				};
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