using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
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

	public class VehicleData : SimulationComponentData
	{
		//public static VehicleData ReadFromFile(string fileName)
		//{
		//	return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName));
		//}


		//public static VehicleData ReadFromJson(string json, string basePath = "")
		//{
		//	//var vehicleData = new VehicleData(basePath);

		//	var fileInfo = GetFileVersion(json);
		//	switch (fileInfo.Item1) {
		//		case 5:
		//			if (fileInfo.Item2) {
		//				var data = JsonConvert.DeserializeObject<VehicleFileV5Declaration>(json);
		//				return new VehicleData(basePath, data);
		//			} else {
		//				var data = JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
		//				return new VehicleData(basePath, data);
		//			}
		//			//vehicleData.Data = data;
		//		default:
		//			throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Item1);
		//	}

		//	//return null;
		//}


		//protected VehicleData(string basePath, VehicleFileV5Declaration data)
		//{
		//	BasePath = basePath;
		//	SetGenericData(data.Body);
		//}

		//protected VehicleData(string basePath, VehicleFileV5Engineering data)
		//{
		//	BasePath = basePath;
		//	SetGenericData(data.Body);
		//}

		protected void SetGenericData(VehicleFileV5Declaration.DataBodyDecl data)
		{
			SavedInDeclarationMode = data.SavedInDeclarationMode;
			VehicleCategory = data.VehicleCategory();
			GrossVehicleMassRating = data.GrossVehicleMassRating.SI<Kilogram>();

			DragCoefficient = data.DragCoefficient;
			CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>();

			DragCoefficientRigidTruck = data.DragCoefficientRigidTruck;
			CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>();
		}

		public string BasePath { get; protected set; }

		public bool SavedInDeclarationMode { get; internal set; }

		protected readonly VehicleCategory _vehicleCategory;

		public VehicleCategory VehicleCategory
		{
			get { return _vehicleCategory; }
		}

		public CrossWindCorrectionMode CrossWindCorrectionMode { get; protected set; }

		public RetarderData Retarder { get; protected set; }

		//[DataMember] private List<Axle> _axleData;

		public AxleConfiguration AxleConfiguration { get; protected set; }

		public Kilogram CurbWeight { get; protected set; }

		public Kilogram CurbWeigthExtra { get; protected set; }

		public Kilogram Loading { get; protected set; }

		public Kilogram TotalVehicleWeight()
		{
			return CurbWeight + CurbWeigthExtra + Loading;
		}

		public Kilogram GrossVehicleMassRating { get; protected internal set; }

		public double DragCoefficient { get; protected internal set; }

		public SquareMeter CrossSectionArea { get; protected internal set; }

		public double DragCoefficientRigidTruck { get; protected internal set; }

		public SquareMeter CrossSectionAreaRigidTruck { get; protected internal set; }

		public CrossWindCorrectionMode CrossWindCorrection { get; protected set; }

		public Meter DynamicTyreRadius { get; protected set; }

		public Kilogram ReducedMassWheels { get; set; }

		public string Rim { get; protected set; }

		public double TotalRollResistanceCoefficient { get; protected set; }
	}

	public class Axle
	{
		//private DataV5Engineering.DataBody.AxleData _data;

		public Axle()
		{
			//_data = data;
		}

		//public Axle(DataV5Declaration.DataBody.AxleData data) {}

		public KilogramSquareMeter Inertia { get; protected set; }

		public double RollResistanceCoefficient { get; protected set; }

		public Newton TyreTestLoad { get; protected set; }

		public double AxleWeightShare { get; protected set; }

		public bool TwinTyres { get; protected set; }
	}
}