using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl
{
	public class DeclarationModeSimulationComponentFactory : InputFileReader, IDataFileReader
	{
		internal DeclarationModeSimulationComponentFactory() {}

		//public void SetJobFile(string fileName)
		//{
		//	var json = File.ReadAllText(fileName);
		//	SetJobJson(json, Path.GetDirectoryName(fileName));
		//}

		//public void SetJobJson(string json, string basePath)
		//{
		//	var fileInfo = GetFileVersion(json);

		//	if (!fileInfo.Item2) {
		//		throw new VectoException("File not saved in Declaration Mode!");
		//	}
		//	switch (fileInfo.Item1) {
		//		case 2:
		//			break;
		//		default:
		//			throw new UnsupportedFileVersionException("Unsupported version of .vecto file/data. Got Version " + fileInfo.Item1);
		//	}
		//}

		public VectoJobData ReadVectoJobFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public VehicleData ReadVehicleDataFile(string fileName)
		{
			var json = File.ReadAllText(fileName);

			return ReadVehicleDataJson(json, Path.GetDirectoryName(fileName));
		}

		public VehicleData ReadVehicleDataJson(string json, string basePath)
		{
			var fileInfo = GetFileVersion(json);

			if (!fileInfo.SavedInDeclarationMode) {
				throw new VectoException("File not saved in Declaration Mode!");
			}

			switch (fileInfo.Version) {
				case 5:
					var data = JsonConvert.DeserializeObject<VehicleFileV5Declaration>(json);
					return CreateVehicleData(data.Body, basePath);
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Version);
			}
		}

		public void ReadEngineFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public void ReadEngineJson(string jsonData, string basePath)
		{
			throw new NotImplementedException();
		}

		internal VehicleData CreateVehicleData(VehicleFileV5Declaration.DataBodyDecl data, string basePath)
		{
			var retVal = new VehicleData {
				BasePath = basePath,
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				AxleConfiguration =
					(AxleConfiguration) Enum.Parse(typeof (AxleConfiguration), "AxleConfig_" + data.AxleConfig.TypeStr),
				// TODO: @@@quam better use of enum-prefix
				CurbWeight = data.CurbWeight.SI<Kilogram>(),
				//CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				//Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = data.GrossVehicleMassRating.SI().Kilo.Kilo.Gramm.Cast<Kilogram>(),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>(),
				//DynamicTyreRadius = data.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>(),
				Rim = data.RimStr,
				Retarder = new RetarderData(data.Retarder, basePath),
				AxleData = data.AxleConfig.Axles.Select(axle => new Axle {
					//Inertia = DoubleExtensionMethods.SI<KilogramSquareMeter>(axle.Inertia),
					TwinTyres = axle.TwinTyres,
					RollResistanceCoefficient = axle.RollResistanceCoefficient,
					//AxleWeightShare = axle.AxleWeightShare,
					TyreTestLoad = DoubleExtensionMethods.SI<Newton>(axle.TyreTestLoad),
					//Wheels = axle.WheelsStr
				}).ToList()
			};

			return retVal;
		}
	}
}