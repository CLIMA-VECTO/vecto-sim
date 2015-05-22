using System;
using System.IO;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories
{
	public class DeclarationModeSimulationComponentFactory : InputFileReader
	{
		protected static DeclarationModeSimulationComponentFactory _instance;

		public static DeclarationModeSimulationComponentFactory Instance()
		{
			return _instance ?? (_instance = new DeclarationModeSimulationComponentFactory());
		}

		public VehicleData CreateVehicleData(string fileName)
		{
			var json = File.ReadAllText(fileName);
			var fileInfo = GetFileVersion(json);

			if (!fileInfo.Item2) {
				throw new VectoException("File not saved in Declaration Mode!");
			}

			switch (fileInfo.Item1) {
				case 5:
					var data = JsonConvert.DeserializeObject<VehicleFileV5Declaration>(json);
					return CreateVehicleData(Path.GetDirectoryName(fileName), data.Body);
				default:
					throw new UnsupportedFileVersionException("Unsupported Version of .vveh file. Got Version " + fileInfo.Item1);
			}
		}

		protected VehicleData CreateVehicleData(string basePath, VehicleFileV5Declaration.DataBodyDecl data)
		{
			//return new VehicleData {
			//	SavedInDeclarationMode = data.SavedInDeclarationMode,
			//	VehicleCategory = data.VehicleCategory(),
			//	GrossVehicleMassRating = data.GrossVehicleMassRating.SI<Kilogram>(),
			//	DragCoefficient = data.DragCoefficient,
			//	CrossSectionArea = data.CrossSectionArea.SI<SquareMeter>(),
			//	DragCoefficientRigidTruck = data.DragCoefficientRigidTruck,
			//	CrossSectionAreaRigidTruck = data.CrossSectionAreaRigidTruck.SI<SquareMeter>()
			//};
			return null;
		}
	}
}