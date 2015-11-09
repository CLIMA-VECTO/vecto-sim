using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class SimulatorFactory : LoggingObject
	{
		public enum FactoryMode
		{
			EngineeringMode,
			DeclarationMode,
			EngineOnlyMode,
		};

		private FactoryMode _mode;

		public SimulatorFactory(FactoryMode mode, string jobFile)
		{
			Log.Fatal("########## VectoCore Version {0} ##########", Assembly.GetExecutingAssembly().GetName().Version);
			JobNumber = 0;
			_mode = mode;
			switch (mode) {
				case FactoryMode.DeclarationMode:
					DataReader = new DeclarationModeSimulationDataReader();
					break;
				case FactoryMode.EngineeringMode:
					DataReader = new EngineeringModeSimulationDataReader();
					break;
				case FactoryMode.EngineOnlyMode:
					DataReader = new EngineOnlySimulationDataReader();
					break;
				default:
					throw new VectoException("Unkown factory mode in SimulatorFactory: {0}", mode);
			}
			DataReader.SetJobFile(jobFile);
		}

		public ISimulationDataReader DataReader { get; private set; }

		public SummaryFileWriter SumWriter { get; set; }

		public int JobNumber { get; set; }

		public bool WriteModalResults { get; set; }

		/// <summary>
		/// Creates powertrain and initializes it with the component's data.
		/// </summary>
		/// <returns>new VectoRun Instance</returns>
		public IEnumerable<IVectoRun> SimulationRuns()
		{
			var i = 0;
			foreach (var data in DataReader.NextRun()) {
				var modFileName = Path.Combine(data.BasePath,
					data.JobFileName.Replace(Constants.FileExtensions.VectoJobFile, "") + "_{0}{1}" +
					Constants.FileExtensions.ModDataFile);
				var d = data;
				IModalDataWriter modWriter =
					new ModalDataWriter(string.Format(modFileName, data.Cycle.Name, data.ModFileSuffix ?? ""),
						writer => d.Report.AddResult(d.Loading, d.Mission, writer), _mode);
				modWriter.WriteModalResults = WriteModalResults;
				var builder = new PowertrainBuilder(modWriter,
					DataReader.IsEngineOnly, (writer, mass, loading) =>
						SumWriter.Write(d.IsEngineOnly, modWriter, d.JobFileName, string.Format("{0}-{1}", JobNumber, i++), d.Cycle.Name,
							mass, loading));

				VectoRun run;
				if (data.IsEngineOnly) {
					run = new TimeRun(builder.Build(data));
				} else {
					var runCaption = string.Format("{0}-{1}-{2}",
						Path.GetFileNameWithoutExtension(data.JobFileName), data.Cycle.Name, data.ModFileSuffix);
					run = new DistanceRun(runCaption, builder.Build(data));
				}

				yield return run;
			}
		}
	}
}