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

		[DataMember] private Data _data;

		[DataMember] private VehicleCategory _vehicleCategory;

		[DataMember] private CrossWindCorrectionMode _crossWindCorrectionMode;

		[DataMember] private RetarderData _retarder;

		[DataMember] private List<Axle> _axleData;

		[DataMember] private AxleConfiguration _axleConfiguration;


		public bool SavedInDeclarationMode
		{
			get { return _data.Body.SavedInDeclarationMode; }
			set { _data.Body.SavedInDeclarationMode = value; }
		}

		public VehicleCategory Category
		{
			get { return _vehicleCategory; }
			protected set { _vehicleCategory = value; }
		}

		public double CurbWeight
		{
			get { return _data.Body.CurbWeight; }
			set { _data.Body.CurbWeight = value; }
		}

		public double CurbWeigthExtra
		{
			get { return _data.Body.CurbWeightExtra; }
			set { _data.Body.CurbWeightExtra = value; }
		}

		public double Loading
		{
			get { return _data.Body.Loading; }
			set { _data.Body.Loading = value; }
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

		public string Rim
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public CrossWindCorrectionMode CrossWindCorrection
		{
			get { return _crossWindCorrectionMode; }
			protected set { _crossWindCorrectionMode = value; }
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

		public static VehicleData ReadFromFile(string fileName)
		{
			return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName));
		}

		public static VehicleData ReadFromJson(string json, string basePath = "")
		{
			var vehicleData = new VehicleData();

			var d = JsonConvert.DeserializeObject<Data>(json);
			if (d.Header.FileVersion > 5) {
				throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + d.Header.FileVersion);
			}

			vehicleData._data = d;

			vehicleData._retarder = new RetarderData(d.Body.Retarder, basePath);

			vehicleData._axleData = new List<Axle>(d.Body.AxleConfig.Axles.Count);
			foreach (var axle in d.Body.AxleConfig.Axles) {
				vehicleData._axleData.Add(new Axle(axle));
			}

			vehicleData._axleConfiguration = AxleConfiguration.AxleConfig4x2;

			switch (d.Body.VehicleCategoryStr) {
				case "RigidTruck":
					vehicleData._vehicleCategory = VehicleCategory.RigidTruck;
					break;
				case "Tractor":
					vehicleData._vehicleCategory = VehicleCategory.Tractor;
					break;
				case "CityBus":
					vehicleData._vehicleCategory = VehicleCategory.CityBus;
					break;
				case "InterurbanBus":
					vehicleData._vehicleCategory = VehicleCategory.InterurbanBus;
					break;
				case "Coach":
					vehicleData._vehicleCategory = VehicleCategory.Coach;
					break;
			}

			switch (d.Body.CrossWindCorrectionModeStr) {
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

			return vehicleData;
		}

		public class Data
		{
			[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
			[JsonProperty(Required = Required.Always)] public DataBody Body;

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

		public class Axle
		{
			private Data.DataBody.AxleData _data;

			public Axle(Data.DataBody.AxleData data)
			{
				_data = data;
			}

			public double Inertia
			{
				get { return _data.Inertia; }
				set { _data.Inertia = value; }
			}

			public double RollResistanceCoefficient
			{
				get { return _data.RollResistanceCoefficient; }
				set { _data.RollResistanceCoefficient = value; }
			}
		}
	}
}