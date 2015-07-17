using System.Collections.Generic;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public abstract class AbstractSimulationDataReader : InputFileReader, ISimulationDataReader
	{
		//protected string JobBasePath = "";

		protected VectoJobFile Job { get; set; }

		protected VectoVehicleFile Vehicle { get; set; }

		protected VectoGearboxFile Gearbox { get; set; }

		protected VectoEngineFile Engine { get; set; }

		protected IList<VectoRunData.AuxData> Aux { get; set; }

		public void SetJobFile(string filename)
		{
			ReadJobFile(filename);
			ProcessJob(Job);
		}

		public abstract bool IsEngineOnly { get; }

		public abstract IEnumerable<VectoRunData> NextRun();


		protected abstract void ProcessJob(VectoJobFile job);


		/// <summary>
		/// has to read the file string and create file-container
		/// </summary>
		protected abstract void ReadJobFile(string file);

		/// <summary>
		/// has to read the file string and create file-container
		/// </summary>
		protected abstract void ReadVehicle(string file);

		protected abstract void ReadEngine(string file);

		protected abstract void ReadGearbox(string file);
	}
}