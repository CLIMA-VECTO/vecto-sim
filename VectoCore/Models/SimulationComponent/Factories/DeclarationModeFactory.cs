using System;
using System.IO;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories
{
	public class DeclarationModeFactory : InputFileReader
	{
		protected static DeclarationModeFactory _instance;

		public static DeclarationModeFactory Instance()
		{
			return _instance ?? (_instance = new DeclarationModeFactory());
		}

		protected void CheckDeclarationMode(VersionInfo fileInfo)
		{
			if (!fileInfo.SavedInDeclarationMode) {
				throw new VectoException("File not saved in Declaration Mode!");
			}
		}

		public VehicleData ReadVehicleData(string filename)
		{
			var json = File.ReadAllText(filename);
			var fileInfo = GetFileVersion(json);
			CheckDeclarationMode(fileInfo);

			switch (fileInfo.Version) {
				case 5:
					return CreateVehicleData(Path.GetDirectoryName(filename), Deserialize<VehicleFileV5Declaration>(json));
				default:
					throw new UnsupportedFileVersionException(filename, fileInfo.Version);
			}
		}

		protected VehicleData CreateVehicleData(string basePath, VehicleFileV5Declaration data)
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

		public VectoJobData ReadJobFile(string filename)
		{
			var json = File.ReadAllText(filename);
			var fileInfo = GetFileVersion(json);
			CheckDeclarationMode(fileInfo);

			switch (fileInfo.Version) {
				case 5:
					return CreateVectoJobData(Path.GetDirectoryName(filename), Deserialize<VectoJobFileV2Declaration>(json));
				default:
					throw new UnsupportedFileVersionException(filename, fileInfo.Version);
			}
		}

		private VectoJobData CreateVectoJobData(string basePath, VectoJobFileV2Declaration data)
		{
			throw new NotImplementedException();
		}
	}

	public class VectoJobFileV2Declaration {}
}