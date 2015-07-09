using System;
using System.Collections.Generic;
using System.IO;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public abstract class AbstractSimulationDataReader : InputFileReader, ISimulationDataReader
	{
		//protected string JobBasePath = "";

		protected VectoJobFile Job;

		protected VectoVehicleFile Vehicle;

		protected VectoGearboxFile Gearbox;

		protected VectoEngineFile Engine;


		public void SetJobFile(string filename)
		{
			ReadJobFile(filename);
			ProcessJob(Job);
		}

		public abstract IEnumerable<VectoRunData> NextRun();


		protected abstract void ProcessJob(VectoJobFile job);


		// has to read the file string and create file-container
		protected abstract void ReadJobFile(string file);

		// has to read the file string and create file-container
		protected abstract void ReadVehicle(string file);

		protected abstract void ReadEngine(string file);

		protected abstract void ReadGearbox(string file);
	}
}