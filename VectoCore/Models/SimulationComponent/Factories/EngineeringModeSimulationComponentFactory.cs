using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories
{
	public class EngineeringModeSimulationComponentFactory : InputFileReader
	{
		protected static EngineeringModeSimulationComponentFactory _instance;

		public static EngineeringModeSimulationComponentFactory Instance()
		{
			return _instance ?? (_instance = new EngineeringModeSimulationComponentFactory());
		}

		public VehicleData CreateVehicleData(string fileName)
		{
			var json = File.ReadAllText(fileName);
			var fileInfo = GetFileVersion(json);

			if (fileInfo.Item2) {
				Log.WarnFormat("File {0} was saved in Declaration Mode but is used for Engineering Mode!", fileName);
			}

			switch (fileInfo.Item1) {
				case 5:
					var data = JsonConvert.DeserializeObject<VehicleFileV5Engineering>(json);
					return CreateVehicleData(Path.GetDirectoryName(fileName), data.Body);
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Item1);
			}
		}

		protected VehicleData CreateVehicleData(string basePath, VehicleFileV5Engineering.DataBodyEng data)
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