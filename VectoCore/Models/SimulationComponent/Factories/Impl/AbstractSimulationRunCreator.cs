using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.FileIO.DeclarationFile;
using TUGraz.VectoCore.Models.Simulation;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl
{
	public abstract class AbstractSimulationRunCreator : InputFileReader
	{
		protected string JobBasePath = "";

		protected VectoJobFile Job;

		protected VectoVehicleFile Vehicle;

		protected VectoGearboxFile Gearbox;

		protected VectoEngineFile Engine;

		public void SetJobFile(string filenname)
		{
			JobBasePath = Path.GetDirectoryName(filenname) + Path.DirectorySeparatorChar;
			SetJobJson(File.ReadAllText(filenname));
		}

		public void SetJobJson(string json)
		{
			ReadJob(json);
			ProcessJob((dynamic) Job);
		}

		public abstract IEnumerable<IVectoRun> NextRun();


		protected void ProcessJob(VectoJobFile job)
		{
			throw new VectoException("Invalid JobFile Format");
		}

		protected void ProcessJob(VectoJobFileV2Declaration job)
		{
			ReadVehicle(File.ReadAllText(JobBasePath + job.Body.VehicleFile));

			ReadEngine(File.ReadAllText(JobBasePath + job.Body.EngineFile));

			ReadGearbox(File.ReadAllText(JobBasePath + job.Body.GearboxFile));
		}

		// has to read the json string and create file-container
		protected abstract void ReadJob(string json);

		// has to read the json string and create file-container
		protected abstract void ReadVehicle(string json);

		protected abstract void ReadEngine(string json);

		protected abstract void ReadGearbox(string json);


		protected abstract void ReadRetarder(string json);
	}
}