using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories
{
	public class EngineeringModeFactory : InputFileReader
	{
		protected static EngineeringModeFactory _instance;

		public static EngineeringModeFactory Instance()
		{
			return _instance ?? (_instance = new EngineeringModeFactory());
		}

		private void CheckEngineeringMode(string fileName, VersionInfo fileInfo)
		{
			if (fileInfo.SavedInDeclarationMode) {
				Log.WarnFormat("File {0} was saved in Declaration Mode but is used for Engineering Mode!", fileName);
			}
		}

		public VehicleData CreateVehicleData(string fileName)
		{
			var json = File.ReadAllText(fileName);
			var fileInfo = GetFileVersion(json);
			CheckEngineeringMode(fileName, fileInfo);

			switch (fileInfo.Version) {
				case 5:
					var data = JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
					return CreateVehicleData(Path.GetDirectoryName(fileName), data.Body);
				default:
					throw new UnsupportedFileVersionException(fileName, fileInfo.Version);
			}
		}


		internal VehicleData CreateVehicleData(string basePath, VehicleFileV5Engineering.DataBodyEng data)
		{
			return new VehicleData {
				BasePath = basePath,
				SavedInDeclarationMode = data.SavedInDeclarationMode,
				VehicleCategory = data.VehicleCategory(),
				CurbWeight = data.CurbWeight.SI<Kilogram>(),
				CurbWeigthExtra = data.CurbWeightExtra.SI<Kilogram>(),
				Loading = data.Loading.SI<Kilogram>(),
				GrossVehicleMassRating = data.GrossVehicleMassRating.SI().Kilo.Kilo.Gramm.Cast<Kilogram>(),
				DragCoefficient = data.DragCoefficient,
				CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
				DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
				CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>(),
				DynamicTyreRadius = data.DynamicTyreRadius.SI().Milli.Meter.Cast<Meter>(),
				//  .SI<Meter>(),
				Rim = data.RimStr,
				Retarder = new RetarderData(data.Retarder, basePath),
				AxleData = data.AxleConfig.Axles.Select(axle => new Axle {
					Inertia = axle.Inertia.SI<KilogramSquareMeter>(),
					TwinTyres = axle.TwinTyres,
					RollResistanceCoefficient = axle.RollResistanceCoefficient,
					AxleWeightShare = axle.AxleWeightShare,
					TyreTestLoad = axle.TyreTestLoad.SI<Newton>()
				}).ToList()
			};
		}
	}
}