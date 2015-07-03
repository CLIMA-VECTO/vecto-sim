using System.Collections.Generic;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.FileIO.Reader
{
	public interface ISimulationDataReader
	{
		void SetJobFile(string fileName);

		IEnumerable<VectoRunData> NextRun();

		//	void SetJobJson(string jsonData, string basePath);
	}

	//public interface IDataFileReader
	//{
	//	VectoRunData ReadVectoJobFile(string fileName);

	//	VehicleData ReadVehicleDataFile(string fileName);

	//	VehicleData ReadVehicleDataJson(string jsonData, string basePath);

	//	void ReadEngineFile(string fileName);

	//	void ReadEngineJson(string jsonData, string basePath);

	//	//void AddCycle(string fileName);
	//}
}