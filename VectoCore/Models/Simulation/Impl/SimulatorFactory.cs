using System.Collections.Generic;
using System.IO;
using NLog;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class SimulatorFactory
	{
		//private IModalDataWriter _dataWriter;

		public enum FactoryMode
		{
			EngineeringMode,
			DeclarationMode,
			EngineOnlyMode,
		};

		private FactoryMode _mode;

		public SimulatorFactory(FactoryMode mode, string jobFile)
		{
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
				IModalDataWriter modWriter =
					new ModalDataWriter(string.Format(modFileName, data.Cycle.Name, data.ModFileSuffix ?? ""), _mode);
				var jobName = string.Format("{0}-{1}", JobNumber, i++);
				var sumWriterDecorator = DecorateSumWriter(data.IsEngineOnly, SumWriter, data.JobFileName, jobName, data.Cycle.Name);
				var builder = new PowertrainBuilder(modWriter, sumWriterDecorator, DataReader.IsEngineOnly);

				VectoRun run;
				if (data.IsEngineOnly) {
					run = new TimeRun(builder.Build(data));
				} else {
					run = new DistanceRun(builder.Build(data));
				}

				yield return run;
			}
		}

		/// <summary>
		/// Decorates the sum writer with a correct decorator (either EngineOnly or FullPowertrain).
		/// </summary>
		/// <param name="engineOnly">if set to <c>true</c> [engine only].</param>
		/// <param name="sumWriter">The sum writer.</param>
		/// <param name="jobFileName">Name of the job file.</param>
		/// <param name="jobName">Name of the job.</param>
		/// <param name="cycleName">The cycle file.</param>
		/// <returns></returns>
		private static ISummaryDataWriter DecorateSumWriter(bool engineOnly, SummaryFileWriter sumWriter,
			string jobFileName, string jobName, string cycleName)
		{
			if (engineOnly) {
				return new SumWriterDecoratorEngineOnly(sumWriter, jobFileName, jobName, cycleName);
			}

			return new SumWriterDecoratorFullPowertrain(sumWriter, jobFileName, jobName, cycleName);
		}
	}
}