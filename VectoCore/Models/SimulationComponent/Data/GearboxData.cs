using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	[DataContract]
	public class GearboxData : SimulationComponentData
	{
		public enum GearboxType
		{
			ManualTransmision,
			AutomatedManualTransmission,
			AutomaticTransmission,
			Custom
		}

		public GearData AxleGearData { get; internal set; }

		private readonly Dictionary<uint, GearData> _gearData = new Dictionary<uint, GearData>();

//		public static GearboxData ReadFromJson(string json, string basePath = "")
//		{
////			var lossMaps = new Dictionary<string, TransmissionLossMap>();

//			var gearboxData = new GearboxData();

//			var d = JsonConvert.DeserializeObject<Data>(json);
//			if (d.Header.FileVersion > 4) {
//				throw new UnsupportedFileVersionException("Unsupported Version of .vgbx file. Got Version: " + d.Header.FileVersion);
//			}

//			gearboxData._data = d;

//			for (uint i = 0; i < d.Body.Gears.Count; i++) {
//				var gearSettings = d.Body.Gears[(int)i];
//				var lossMapPath = Path.Combine(basePath, gearSettings.LossMap);
//				TransmissionLossMap lossMap = TransmissionLossMap.ReadFromFile(lossMapPath, gearSettings.Ratio);

//				var shiftPolygon = !String.IsNullOrEmpty(gearSettings.ShiftPolygon)
//					? ShiftPolygon.ReadFromFile(Path.Combine(basePath, gearSettings.ShiftPolygon))
//					: null;

//				var gear = new GearData(lossMap, shiftPolygon, gearSettings.Ratio, gearSettings.TCactive);
//				if (i == 0) {
//					gearboxData.AxleGearData = gear;
//				} else {
//					gearboxData._gearData.Add(i, gear);
//				}
//			}

//			switch (d.Body.GearboxTypeStr) {
//				case "AMT":
//					gearboxData._type = GearboxType.AutomatedManualTransmission;
//					break;
//				case "MT":
//					gearboxData._type = GearboxType.ManualTransmision;
//					break;
//				case "AT":
//					gearboxData._type = GearboxType.AutomaticTransmission;
//					break;
//				default:
//					gearboxData._type = GearboxType.Custom;
//					break;
//			}
//			return gearboxData;
//		}

		// @@@quam: according to Raphael no longer required
		//public void CalculateAverageEfficiency(CombustionEngineData engineData)
		//{
		//	var angularVelocityStep = (2.0 / 3.0) * (engineData.GetFullLoadCurve(0).RatedSpeed() - engineData.IdleSpeed) / 10.0;

		//	var axleGearEfficiencySum = 0.0;
		//	var axleGearSumCount = 0;

		//	foreach (var gearEntry in _gearData) {
		//		var gearEfficiencySum = 0.0;
		//		var gearSumCount = 0;
		//		for (var angularVelocity = engineData.IdleSpeed + angularVelocityStep;
		//			angularVelocity < engineData.GetFullLoadCurve(0).RatedSpeed();
		//			angularVelocity += angularVelocityStep) {
		//			var fullLoadStationaryTorque = engineData.GetFullLoadCurve(gearEntry.Key).FullLoadStationaryTorque(angularVelocity);
		//			var torqueStep = (2.0 / 3.0) * fullLoadStationaryTorque / 10.0;
		//			for (var engineOutTorque = (1.0 / 3.0) * fullLoadStationaryTorque;
		//				engineOutTorque < fullLoadStationaryTorque;
		//				engineOutTorque += torqueStep) {
		//				var engineOutPower = Formulas.TorqueToPower(engineOutTorque, angularVelocity);
		//				var gearboxOutPower =
		//					Formulas.TorqueToPower(
		//						gearEntry.Value.LossMap.GearboxOutTorque(angularVelocity, engineOutTorque), angularVelocity);
		//				if (gearboxOutPower > engineOutPower) {
		//					gearboxOutPower = engineOutPower;
		//				}

		//				gearEfficiencySum += ((engineOutPower - gearboxOutPower) / engineOutPower).Double();
		//				gearSumCount += 1;


		//				// axle gear
		//				var angularVelocityAxleGear = angularVelocity / gearEntry.Value.Ratio;
		//				var axlegearOutPower =
		//					Formulas.TorqueToPower(
		//						AxleGearData.LossMap.GearboxOutTorque(angularVelocityAxleGear,
		//							Formulas.PowerToTorque(engineOutPower, angularVelocityAxleGear)),
		//						angularVelocityAxleGear);
		//				if (axlegearOutPower > engineOutPower) {
		//					axlegearOutPower = engineOutPower;
		//				}
		//				axleGearEfficiencySum += (axlegearOutPower / engineOutPower).Double();
		//				axleGearSumCount += 1;
		//			}
		//		}
		//		gearEntry.Value.AverageEfficiency = gearEfficiencySum / gearSumCount;
		//	}
		//	AxleGearData.AverageEfficiency = axleGearEfficiencySum / axleGearSumCount;
		//}

		public int GearsCount()
		{
			return _gearData.Count;
		}

		public GearData this[uint i]
		{
			get { return _gearData[i]; }
		}


		public GearboxType Type { get; internal set; }

		/// <summary>
		///		kgm^2
		/// </summary>
		public KilogramSquareMeter Inertia { get; internal set; }

		/// <summary>
		///		[s]
		/// </summary>
		public Second TractionInterruption { get; internal set; }

		/// <summary>
		///		[%] (0-1)
		/// </summary>
		public double TorqueReserve { get; internal set; }

		/// <summary>
		///		used by gear-shift model
		/// </summary>
		public bool SkipGears { get; internal set; }

		/// <summary>
		///		[s]
		/// </summary>
		public Second ShiftTime { get; internal set; }

		public bool EarlyShiftUp { get; internal set; }

		/// <summary>
		/// [%] (0-1)
		/// </summary>
		public double StartTorqueReserve { get; internal set; }

		/// <summary>
		/// [m/s]
		/// </summary>
		public MeterPerSecond StartSpeed { get; internal set; }

		/// <summary>
		/// [m/s^2]
		/// </summary>
		public SI StartAcceleration { get; internal set; }

		public bool HasTorqueConverter { get; internal set; }
	}
}