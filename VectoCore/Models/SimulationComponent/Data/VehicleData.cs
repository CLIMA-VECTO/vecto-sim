using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class VehicleData : SimulationComponentData
	{
		public enum VehicleCategory
		{
			RigidTruck,
			Tractor,
			CityBus,
			InterurbanBus,
			Coach
		}

		public enum CrossWindCorrectionMode
		{
			NoCorrection,
			SpeedDependent,
			VAirBeta
		}

		public enum AxleConfiguration
		{
			AxleConfig4x2,
			AxleConfig4x4,
			AxleConfig6x2,
			AxleConfig6x4,
			AxleConfig6x6,
			AxleConfig8x2,
			AxleConfig8x4,
			AxleConfig8x6,
			AxleConfig8x8,
		}

		[DataMember] private DataV5Engineering _data;

		[DataMember]
		protected DataV5Engineering Data
		{
			get { return _data; }
			set
			{
				_data = value;
				_data.SetProperties(this);
			}
		}

		[DataMember] private CrossWindCorrectionMode _crossWindCorrectionMode;

		[DataMember] private RetarderData _retarder;

		[DataMember] private List<Axle> _axleData;

		[DataMember] private AxleConfiguration _axleConfiguration;

		[DataMember]
		public string BasePath { get; protected set; }

		protected VehicleData(string basePath)
		{
			BasePath = basePath;
		}

		public bool SavedInDeclarationMode
		{
			get { return _data.Body.SavedInDeclarationMode; }
			set { _data.Body.SavedInDeclarationMode = value; }
		}

		[DataMember] private VehicleCategory _vehicleCategory;

		public VehicleCategory Category
		{
			get { return _vehicleCategory; }
			set
			{
				_vehicleCategory = value;
				_data.Body.VehicleCategoryStr = value.ToString();
			}
		}


		public Kilogram CurbWeight
		{
			get { return _data.Body.CurbWeight.SI<Kilogram>(); }
			set { _data.Body.CurbWeight = value.Double(); }
		}


		public Kilogram CurbWeigthExtra
		{
			get { return _data.Body.CurbWeightExtra.SI<Kilogram>(); }
			set { _data.Body.CurbWeightExtra = value.Double(); }
		}

		public Kilogram Loading
		{
			get { return _data.Body.Loading.SI<Kilogram>(); }
			set { _data.Body.Loading = value.Double(); }
		}

		public Kilogram TotalVehicleWeight()
		{
			return CurbWeight + CurbWeigthExtra + Loading;
		}

		public Kilogram GrossVehicleMassRating
		{
			get { return _data.Body.GrossVehicleMassRating.SI<Kilogram>(); }
			set { _data.Body.GrossVehicleMassRating = (double) value.ConvertTo().Kilo.Kilo.Gramm; }
		}

		public double DragCoefficient
		{
			get { return _data.Body.DragCoefficient; }
			set { _data.Body.DragCoefficient = value; }
		}

		public SquareMeter CrossSectionArea
		{
			get { return _data.Body.CrossSectionArea.SI<SquareMeter>(); }
			set { _data.Body.CrossSectionArea = value.Double(); }
		}

		public double DragCoefficientRigidTruck
		{
			get { return _data.Body.DragCoefficient; }
			set { _data.Body.DragCoefficient = value; }
		}

		public SquareMeter CrossSectionAreaRigidTruck
		{
			get { return _data.Body.CrossSectionArea.SI<SquareMeter>(); }
			set { _data.Body.CrossSectionArea = value.Double(); }
		}

		public Meter DynamicTyreRadius
		{
			get { return _data.Body.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>(); }
			set { _data.Body.DynamicTyreRadius = (double) value.ConvertTo().Milli.Meter; }
		}

		public Kilogram ReducedMassWheels { get; set; }

		public string Rim
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public CrossWindCorrectionMode CrossWindCorrection
		{
			get { return _crossWindCorrectionMode; }
			set { _crossWindCorrectionMode = value; }
		}

		public RetarderData Retarder
		{
			get { return _retarder; }
			protected set { _retarder = value; }
		}

		public AxleConfiguration Axles
		{
			get { return _axleConfiguration; }
		}

		public double TotalRollResistanceCoefficient { get; protected set; }

		public static VehicleData ReadFromFile(string fileName)
		{
			return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName));
		}

		public static VehicleData ReadFromJson(string json, string basePath = "")
		{
			var vehicleData = new VehicleData(basePath);

			var fileVersion = GetFileVersion(json);
			switch (fileVersion) {
				case 5:
					var data = JsonConvert.DeserializeObject<DataV5Engineering>(json);
					vehicleData.Data = data;
					break;
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileVersion);
			}

			return vehicleData;
		}

		public class DataV5Engineering
		{
			[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
			[JsonProperty(Required = Required.Always)] public DataBody Body;

			public void SetProperties(VehicleData vehicleData)
			{
				vehicleData._axleConfiguration = AxleConfiguration.AxleConfig4x2;

				vehicleData._retarder = new RetarderData(Body.Retarder, vehicleData.BasePath);

				vehicleData._axleData = new List<Axle>(Body.AxleConfig.Axles.Count);
				var RRC = 0.0;
				var mRed0 = 0.SI<Kilogram>();
				foreach (var axleData in Body.AxleConfig.Axles) {
					var axle = new Axle(axleData);
					if (axle.RollResistanceCoefficient < 0) {
						throw new VectoException("Axle roll resistance coefficient < 0");
					}
					if (axle.TyreTestLoad <= 0) {
						throw new VectoException("Axle tyre test load (FzISO) must be greater than 0!");
					}
					var nrWheels = axle.TwinTyres ? 4 : 2;
					RRC += axle.AxleWeightShare * axle.RollResistanceCoefficient *
							Math.Pow(
								(axle.AxleWeightShare * vehicleData.TotalVehicleWeight() * Physics.GravityAccelleration / axle.TyreTestLoad /
								nrWheels).Double(), Physics.RollResistanceExponent - 1);
					mRed0 += nrWheels * (axle.Inertia / vehicleData.DynamicTyreRadius / vehicleData.DynamicTyreRadius).Cast<Kilogram>();

					vehicleData._axleData.Add(axle);
				}
				vehicleData.TotalRollResistanceCoefficient = RRC;
				vehicleData.ReducedMassWheels = mRed0;

				switch (Body.VehicleCategoryStr) {
					case "RigidTruck":
						vehicleData.Category = VehicleCategory.RigidTruck;
						break;
					case "Tractor":
						vehicleData.Category = VehicleCategory.Tractor;
						break;
					case "CityBus":
						vehicleData.Category = VehicleCategory.CityBus;
						break;
					case "InterurbanBus":
						vehicleData.Category = VehicleCategory.InterurbanBus;
						break;
					case "Coach":
						vehicleData.Category = VehicleCategory.Coach;
						break;
				}

				switch (Body.CrossWindCorrectionModeStr) {
					case "CdOfBeta":
						vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.VAirBeta;
						break;
					case "CdOfV":
						vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.SpeedDependent;
						break;
					default:
						vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection;
						break;
				}
			}

			public class DataBody
			{
				[JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

				[JsonProperty("VehCat", Required = Required.Always)] public string VehicleCategoryStr;

				[JsonProperty(Required = Required.Always)] public double CurbWeight;

				[JsonProperty] public double CurbWeightExtra;

				[JsonProperty] public double Loading;

				[JsonProperty("MassMax", Required = Required.Always)] public double GrossVehicleMassRating;

				[JsonProperty("Cd2")] public double DragCoefficientRigidTruck; // without trailer

				[JsonProperty("CrossSecArea2")] public double CrossSectionAreaRigidTruck;

				[JsonProperty("Cd", Required = Required.Always)] public double DragCoefficient;

				[JsonProperty("CrossSecArea", Required = Required.Always)] public double CrossSectionArea;

				[JsonProperty("rdyn")] public double DynamicTyreRadius;

				[JsonProperty("Rim")] public string RimStr;

				[JsonProperty("CdCorrMode")] public string CrossWindCorrectionModeStr;

				[JsonProperty("CdCorrFile")] public string CrossWindCorrectionFile;

				[JsonProperty("Retarder", Required = Required.Always)] public RetarderData.Data Retarder;

				[JsonProperty(Required = Required.Always)] public AxleConfigData AxleConfig;


				public class AxleConfigData
				{
					[JsonProperty("Type", Required = Required.Always)] public string TypeStr;
					[JsonProperty(Required = Required.Always)] public IList<AxleData> Axles;
				}

				public class AxleData
				{
					[JsonProperty] public double Inertia;
					[JsonProperty] public string WheelsStr;
					[JsonProperty] public double AxleWeightShare;
					[JsonProperty(Required = Required.Always)] public bool TwinTyres;
					[JsonProperty("RRCISO", Required = Required.Always)] public double RollResistanceCoefficient;
					[JsonProperty("FzISO", Required = Required.Always)] public double TyreTestLoad;
				}
			}
		}


		public class DataV5Declaration
		{
			[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
			[JsonProperty(Required = Required.Always)] public DataBody Body;

			public void SetProperties(VehicleData vehicleData)
			{
				vehicleData._axleConfiguration = AxleConfiguration.AxleConfig4x2;

				vehicleData._retarder = new RetarderData(Body.Retarder, vehicleData.BasePath);

				vehicleData._axleData = new List<Axle>(Body.AxleConfig.Axles.Count);
				var RRC = 0.0;
				var mRed0 = 0.SI<Kilogram>();
				foreach (var axleData in Body.AxleConfig.Axles) {
					var axle = new Axle(axleData);
					if (axle.RollResistanceCoefficient < 0) {
						throw new VectoException("Axle roll resistance coefficient < 0");
					}
					if (axle.TyreTestLoad <= 0) {
						throw new VectoException("Axle tyre test load (FzISO) must be greater than 0!");
					}
					var nrWheels = axle.TwinTyres ? 4 : 2;
					RRC += axle.AxleWeightShare * axle.RollResistanceCoefficient *
							Math.Pow(
								(axle.AxleWeightShare * vehicleData.TotalVehicleWeight() * Physics.GravityAccelleration / axle.TyreTestLoad /
								nrWheels).Double(), Physics.RollResistanceExponent - 1);
					mRed0 += nrWheels * (axle.Inertia / vehicleData.DynamicTyreRadius / vehicleData.DynamicTyreRadius).Cast<Kilogram>();

					vehicleData._axleData.Add(axle);
				}
				vehicleData.TotalRollResistanceCoefficient = RRC;
				vehicleData.ReducedMassWheels = mRed0;

				switch (Body.VehicleCategoryStr) {
					case "RigidTruck":
						vehicleData.Category = VehicleCategory.RigidTruck;
						break;
					case "Tractor":
						vehicleData.Category = VehicleCategory.Tractor;
						break;
					case "CityBus":
						vehicleData.Category = VehicleCategory.CityBus;
						break;
					case "InterurbanBus":
						vehicleData.Category = VehicleCategory.InterurbanBus;
						break;
					case "Coach":
						vehicleData.Category = VehicleCategory.Coach;
						break;
				}

				//switch (Body.CrossWindCorrectionModeStr) {
				//	case "CdOfBeta":
				//		vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.VAirBeta;
				//		break;
				//	case "CdOfV":
				//		vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.SpeedDependent;
				//		break;
				//	default:
				//		vehicleData._crossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection;
				//		break;
				//}
			}

			public class DataBody
			{
				[JsonProperty("SavedInDeclMode", Required = Required.Always)] public bool SavedInDeclarationMode;

				[JsonProperty("VehCat", Required = Required.Always)] public string VehicleCategoryStr;

				[JsonProperty(Required = Required.Always)] public double CurbWeight;

				//[JsonProperty]
				//public double CurbWeightExtra;

				//[JsonProperty]
				//public double Loading;

				[JsonProperty("MassMax", Required = Required.Always)] public double GrossVehicleMassRating;

				[JsonProperty("Cd2")] public double DragCoefficientRigidTruck; // without trailer

				[JsonProperty("CrossSecArea2")] public double CrossSectionAreaRigidTruck;

				[JsonProperty("Cd", Required = Required.Always)] public double DragCoefficient;

				[JsonProperty("CrossSecArea", Required = Required.Always)] public double CrossSectionArea;

				//[JsonProperty("rdyn")]
				//public double DynamicTyreRadius;

				[JsonProperty("Rim", Required = Required.Always)] public string RimStr;

				//[JsonProperty("CdCorrMode")]
				//public string CrossWindCorrectionModeStr;

				//[JsonProperty("CdCorrFile")]
				//public string CrossWindCorrectionFile;

				[JsonProperty("Retarder", Required = Required.Always)] public RetarderData.Data Retarder;

				[JsonProperty(Required = Required.Always)] public AxleConfigData AxleConfig;


				public class AxleConfigData
				{
					[JsonProperty("Type", Required = Required.Always)] public string TypeStr;
					[JsonProperty(Required = Required.Always)] public IList<AxleData> Axles;
				}

				public class AxleData
				{
					//[JsonProperty]
					//public double Inertia;
					[JsonProperty(Required = Required.Always)] public string WheelsStr;
					//[JsonProperty(Required = Required.Always)]
					//public double AxleWeightShare;
					[JsonProperty(Required = Required.Always)] public bool TwinTyres;
					[JsonProperty("RRCISO", Required = Required.Always)] public double RollResistanceCoefficient;
					[JsonProperty("FzISO", Required = Required.Always)] public double TyreTestLoad;
				}
			}
		}

		public class Axle
		{
			private DataV5Engineering.DataBody.AxleData _data;

			public Axle(DataV5Engineering.DataBody.AxleData data)
			{
				_data = data;
			}

			public Axle(DataV5Declaration.DataBody.AxleData data) {}

			public SI Inertia
			{
				get { return _data.Inertia.SI().Kilo.Gramm.Square.Meter; }
				set { _data.Inertia = value.Double(); }
			}

			public double RollResistanceCoefficient
			{
				get { return _data.RollResistanceCoefficient; }
				set { _data.RollResistanceCoefficient = value; }
			}

			public Newton TyreTestLoad
			{
				get { return _data.TyreTestLoad.SI<Newton>(); }
				set { _data.TyreTestLoad = value.Double(); }
			}

			public double AxleWeightShare
			{
				get { return _data.AxleWeightShare; }
				set { _data.AxleWeightShare = value; }
			}

			public bool TwinTyres
			{
				get { return _data.TwinTyres; }
				set { _data.TwinTyres = value; }
			}
		}
	}
}