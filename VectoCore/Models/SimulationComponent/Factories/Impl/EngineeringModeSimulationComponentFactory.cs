using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

[assembly: InternalsVisibleTo("VectoCoreTest")]

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl
{
	public class EngineeringModeSimulationComponentFactory : InputFileReader, IDataFileReader
	{
		internal EngineeringModeSimulationComponentFactory() {}

		public VectoJobData ReadVectoJobFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public VectoJobData ReadVectoJobJson(string json, string basePath)
		{
			var fileInfo = GetFileVersion(json);
			if (fileInfo.Item2) {
				Log.WarnFormat("File was saved in Declaration Mode but is used for Engineering Mode!");
			}

			switch (fileInfo.Item1) {
				case 2:
					var data = JsonConvert.DeserializeObject<VectoJobFileV2Engineering>(json);
					return CreateVectoJobData(data, basePath);
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vecto file. Got Version" + fileInfo.Item1);
			}
		}

		public VehicleData ReadVehicleDataFile(string fileName)
		{
			var json = File.ReadAllText(fileName);

			return ReadVehicleDataJson(json, Path.GetDirectoryName(fileName));
		}

		public VehicleData ReadVehicleDataJson(string json, string basePath)
		{
			var fileInfo = GetFileVersion(json);

			if (fileInfo.Item2) {
				Log.WarnFormat("File was saved in Declaration Mode but is used for Engineering Mode!");
			}

			switch (fileInfo.Item1) {
				case 5:
					var data = JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
					return CreateVehicleData(data.Body, basePath);
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Item1);
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


		internal VectoJobData CreateVectoJobData(VectoJobFileV2Engineering data, string basePath)
		{
			return new VectoJobData();
		}


		internal VehicleData CreateVehicleData(VehicleFileV5Engineering.DataBodyEng data, string basePath)
		{
			return new VehicleData {
				BasePath = basePath,
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				AxleConfiguration =
					(AxleConfiguration) Enum.Parse(typeof (AxleConfiguration), "AxleConfig_" + data.AxleConfig.TypeStr),
				// TODO: @@@quam better use of enum-prefix
				CurbWeight = data.CurbWeight.SI<Kilogram>(),
				CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = data.GrossVehicleMassRating.SI().Kilo.Kilo.Gramm.Cast<Kilogram>(),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>(),
				DynamicTyreRadius = data.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>(),
				Rim = data.RimStr,
				Retarder = new RetarderData(data.Retarder, basePath),
				AxleData = data.AxleConfig.Axles.Select(axle => new Axle {
					Inertia = DoubleExtensionMethods.SI<KilogramSquareMeter>(axle.Inertia),
					TwinTyres = axle.TwinTyres,
					RollResistanceCoefficient = axle.RollResistanceCoefficient,
					AxleWeightShare = axle.AxleWeightShare,
					TyreTestLoad = DoubleExtensionMethods.SI<Newton>(axle.TyreTestLoad),
					Wheels = axle.WheelsStr
				}).ToList()
			};
		}
	}
}