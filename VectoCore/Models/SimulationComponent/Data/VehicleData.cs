using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
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
		AxleConfig_4x2,
		AxleConfig_4x4,
		AxleConfig_6x2,
		AxleConfig_6x4,
		AxleConfig_6x6,
		AxleConfig_8x2,
		AxleConfig_8x4,
		AxleConfig_8x6,
		AxleConfig_8x8,
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

		//internal void SetGenericData(VehicleFileV5Declaration.DataBodyDecl data)
		//{
		//	SavedInDeclarationMode = data.SavedInDeclarationMode;
		//	VehicleCategory = data.VehicleCategory();
		//	GrossVehicleMassRating = data.GrossVehicleMassRating.SI<Kilogram>();

		//	DragCoefficient = data.DragCoefficient;
		//	CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>();

		//	DragCoefficientRigidTruck = data.DragCoefficientRigidTruck;
		//	CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>();
		//}

		public string BasePath { get; internal set; }

		public bool SavedInDeclarationMode { get; internal set; }

		public VehicleCategory VehicleCategory { get; internal set; }

		public CrossWindCorrectionMode CrossWindCorrectionMode { get; internal set; }

		public RetarderData Retarder { get; internal set; }

		private List<Axle> _axleData;

		/// <summary>
		/// Set the properties for all axles of the vehicle
		/// </summary>
		public List<Axle> AxleData
		{
			get { return _axleData; }
			internal set
			{
				_axleData = value;
				ComputeRollResistanceAndReducedMassWheels();
			}
		}

		public AxleConfiguration AxleConfiguration { get; internal set; }

		public Kilogram CurbWeight { get; internal set; }

		public Kilogram CurbWeigthExtra { get; internal set; }

		public Kilogram Loading { get; internal set; }

		public Kilogram TotalVehicleWeight()
		{
			var retVal = 0.SI<Kilogram>();
			retVal += CurbWeight ?? 0.SI<Kilogram>();
			retVal += CurbWeigthExtra ?? 0.SI<Kilogram>();
			retVal += Loading ?? 0.SI<Kilogram>();
			return retVal;
		}

		public Kilogram GrossVehicleMassRating { get; internal set; }

		public double DragCoefficient { get; internal set; }

		public SquareMeter CrossSectionArea { get; internal set; }

		public double DragCoefficientRigidTruck { get; internal set; }

		public SquareMeter CrossSectionAreaRigidTruck { get; internal set; }

		public CrossWindCorrectionMode CrossWindCorrection { get; internal set; }

		public Meter DynamicTyreRadius { get; internal set; }

		public Kilogram ReducedMassWheels { get; private set; }

		public string Rim { get; internal set; }

		public double TotalRollResistanceCoefficient { get; private set; }

		protected void ComputeRollResistanceAndReducedMassWheels()
		{
			if (TotalVehicleWeight() == 0.SI<Kilogram>()) {
				throw new VectoException("Total vehicle weight must be greater than 0! Set CurbWeight and Loading before!");
			}
			if (DynamicTyreRadius == null) {
				throw new VectoException("Dynamic tyre radius must be set before axles!");
			}

			var RRC = 0.0;
			var mRed0 = 0.SI<Kilogram>();
			foreach (var axle in _axleData) {
				var nrWheels = axle.TwinTyres ? 4 : 2;
				RRC += axle.AxleWeightShare * axle.RollResistanceCoefficient *
						Math.Pow(
							(axle.AxleWeightShare * TotalVehicleWeight() * Physics.GravityAccelleration / axle.TyreTestLoad /
							nrWheels).Double(), Physics.RollResistanceExponent - 1);
				mRed0 += nrWheels * (axle.Inertia / DynamicTyreRadius / DynamicTyreRadius).Cast<Kilogram>();
			}
			TotalRollResistanceCoefficient = RRC;
			ReducedMassWheels = mRed0;
		}
	}

	public class Axle
	{
		public KilogramSquareMeter Inertia { get; internal set; }

		public double RollResistanceCoefficient { get; internal set; }

		public Newton TyreTestLoad { get; internal set; }

		public double AxleWeightShare { get; internal set; }

		public bool TwinTyres { get; internal set; }

		public string Wheels { get; internal set; }
	}
}