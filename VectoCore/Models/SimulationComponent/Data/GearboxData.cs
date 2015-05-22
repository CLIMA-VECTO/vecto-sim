using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	/// <summary>
	///		Represents the Data containing all parameters of the gearbox
	/// </summary>
	/// {
	///  "Header": {
	///    "CreatedBy": "Raphael Luz IVT TU-Graz (85407225-fc3f-48a8-acda-c84a05df6837)",
	///    "Date": "29.07.2014 16:59:17",
	///    "AppVersion": "2.0.4-beta",
	///    "FileVersion": 4
	///  },
	///  "Body": {
	///    "SavedInDeclMode": false,
	///    "ModelName": "Generic 24t Coach",
	///    "Inertia": 0.0,
	///    "TracInt": 1.0,
	///     "TqReserve": 20.0,
	///		"SkipGears": true,
	///		"ShiftTime": 2,
	///		"EaryShiftUp": true,
	///		"StartTqReserve": 20.0,
	///		"StartSpeed": 2.0,
	///		"StartAcc": 0.6,
	///		"GearboxType": "AMT",
	///		"TorqueConverter": {
	///			"Enabled": false,
	///			"File": "<NOFILE>",
	///			"RefRPM": 0.0,
	///			"Inertia": 0.0
	///		}
	///    "Gears": [
	///      {
	///        "Ratio": 3.240355,
	///        "LossMap": "Axle.vtlm"
	///      },
	///      {
	///        "Ratio": 6.38,
	///        "LossMap": "Indirect GearData.vtlm",
	///        "TCactive": false,
	///        "ShiftPolygon": "ShiftPolygon.vgbs"
	///      },
	///		...
	///		]
	/// }
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

		[DataMember]
		public GearData AxleGearData { get; protected set; }

		//private double _axleGearEfficiency;

		[DataMember] private Dictionary<uint, GearData> _gearData = new Dictionary<uint, GearData>();

		//private Dictionary<uint, double> _gearEfficiency = new Dictionary<uint, double>();

		[DataMember] private Data _data;

		[DataMember] private GearboxType _type;

		public static GearboxData ReadFromFile(string fileName)
		{
			return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName));
		}

		public static GearboxData ReadFromJson(string json, string basePath = "")
		{
//			var lossMaps = new Dictionary<string, TransmissionLossMap>();

			var gearboxData = new GearboxData();

			var d = JsonConvert.DeserializeObject<Data>(json);
			if (d.Header.FileVersion > 4) {
				throw new UnsupportedFileVersionException("Unsupported Version of .vgbx file. Got Version: " + d.Header.FileVersion);
			}

			gearboxData._data = d;

			for (uint i = 0; i < d.Body.Gears.Count; i++) {
				var gearSettings = d.Body.Gears[(int)i];
				var lossMapPath = Path.Combine(basePath, gearSettings.LossMap);
				TransmissionLossMap lossMap = TransmissionLossMap.ReadFromFile(lossMapPath, gearSettings.Ratio);

				var shiftPolygon = !String.IsNullOrEmpty(gearSettings.ShiftPolygon)
					? ShiftPolygon.ReadFromFile(Path.Combine(basePath, gearSettings.ShiftPolygon))
					: null;

				var gear = new GearData(lossMap, shiftPolygon, gearSettings.Ratio, gearSettings.TCactive);
				if (i == 0) {
					gearboxData.AxleGearData = gear;
				} else {
					gearboxData._gearData.Add(i, gear);
				}
			}

			switch (d.Body.GearboxTypeStr) {
				case "AMT":
					gearboxData._type = GearboxType.AutomatedManualTransmission;
					break;
				case "MT":
					gearboxData._type = GearboxType.ManualTransmision;
					break;
				case "AT":
					gearboxData._type = GearboxType.AutomaticTransmission;
					break;
				default:
					gearboxData._type = GearboxType.Custom;
					break;
			}
			return gearboxData;
		}

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
			return _data.Body.Gears.Count;
		}

		public GearData this[uint i]
		{
			get { return _gearData[i]; }
		}

		public bool SavedInDeclarationMode
		{
			get { return _data.Body.SavedInDeclarationMode; }
			protected set { _data.Body.SavedInDeclarationMode = value; }
		}

		public GearboxType Type()
		{
			return _type;
		}

		/// <summary>
		///		kgm^2
		/// </summary>
		public KilogramSquareMeter Inertia
		{
			get { return _data.Body.Inertia.SI<KilogramSquareMeter>(); }
			protected set { _data.Body.Inertia = (double)value.ConvertTo().Kilo.Gramm.Square.Meter; }
		}

		/// <summary>
		///		[s]
		/// </summary>
		public Second TractionInterruption
		{
			get { return _data.Body.TractionInterruption.SI<Second>(); }
			protected set { _data.Body.TractionInterruption = value.Double(); }
		}

		public double TorqueReserve
		{
			get { return _data.Body.TorqueReserve / 100; }
			protected set { _data.Body.TorqueReserve = value * 100; }
		}

		/// <summary>
		///		used by gear-shift model
		/// </summary>
		public bool SkipGears
		{
			get { return _data.Body.SkipGears; }
			protected set { _data.Body.SkipGears = value; }
		}

		public Second ShiftTime
		{
			get { return _data.Body.ShiftTime.SI<Second>(); }
			protected set { _data.Body.ShiftTime = value.Double(); }
		}

		public bool EarlyShiftUp
		{
			get { return _data.Body.EarlyShiftUp; }
			protected set { _data.Body.EarlyShiftUp = value; }
		}

		public double StartTorqueReserve
		{
			get { return _data.Body.StartTorqueReserve / 100; }
			protected set { _data.Body.StartTorqueReserve = value * 100; }
		}

		public MeterPerSecond StartSpeed
		{
			get { return _data.Body.StartSpeed.SI<MeterPerSecond>(); }
			protected set { _data.Body.StartSpeed = value.Double(); }
		}

		public SI StartAcceleration
		{
			get { return _data.Body.StartAcceleration; }
			protected set { _data.Body.StartAcceleration = value; }
		}

		public bool HasTorqueConverter
		{
			get { return _data.Body.TorqueConverter != null; }
		}


		/// <summary>
		///		A class which represents the json data format for serializing and deseralizing the GearboxData files
		/// </summary>
		public class Data
		{
			[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
			[JsonProperty(Required = Required.Always)] public DataBody Body;

			public class DataBody
			{
				[JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

				/// <summary>
				///		Model. Free text defining the gearbox model, type, etc.
				/// </summary>
				[JsonProperty(Required = Required.Always)] public string ModelName;

				/// <summary>
				///		[kgm^2] Rotation inertia of the gearbox (constant for all gears)
				/// </summary>
				[JsonProperty(Required = Required.Always)] public double Inertia;

				/// <summary>
				///		[s] Interruption during gear shift event
				/// </summary>
				[JsonProperty("TracInt", Required = Required.Always)] public double TractionInterruption;

				/// <summary>
				///		[%] This parameter is required for the "Allow shift-up inside polygons" and "Skip Gears" option
				/// </summary>
				[JsonProperty("TqReserve")] public double TorqueReserve;

				[JsonProperty] public bool SkipGears;

				/// <summary>
				///   min. time interval between two gearshifts
				/// </summary>
				[JsonProperty] public double ShiftTime;

				/// <summary>
				///   ???
				/// </summary>
				[JsonProperty] public bool EarlyShiftUp;

				/// <summary>
				///  ???
				/// </summary>
				[JsonProperty("StartTqReserve")] public double StartTorqueReserve;

				/// <summary>
				///		[m/s] vehicle speed at start
				/// </summary>
				[JsonProperty] public double StartSpeed;

				/// <summary>
				///   [m/s^2] accelleration of the vehicle at start
				/// </summary>
				[JsonProperty("StartAcc")] public SI StartAcceleration;

				[JsonProperty("GearboxType", Required = Required.Always)] public string GearboxTypeStr;

				/// <summary>
				///		Contains all parameters of the torque converter if used
				/// </summary>
				[JsonProperty] public TorqueConverterData TorqueConverter;

				[JsonProperty(Required = Required.Always)] public IList<GearData> Gears;

				public class GearData
				{
					[JsonProperty(Required = Required.Always)] public double Ratio;
					[JsonProperty(Required = Required.Always)] public string LossMap;
					[JsonProperty] public string ShiftPolygon;
					[JsonProperty] public bool TCactive;
				}
			}
		}
	}
}