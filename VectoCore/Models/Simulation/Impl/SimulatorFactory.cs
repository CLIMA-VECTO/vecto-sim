using System;
using System.IO;
using System.Collections.Generic;
using TUGraz.VectoCore.Configuration;
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

		public SimulatorFactory(FactoryMode mode)
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
			}
		}

		///// <summary>
		///// Creates a simulation run for time based engine only powertrain.
		///// </summary>
		//public static IVectoRun CreateTimeBasedEngineOnlyRun(string engineFile, string cycleName, string jobFileName,
		//	string jobName, IModalDataWriter dataWriter, SummaryFileWriter sumWriter)
		//{
		//	var sumWriterDecorator = new SumWriterDecoratorEngineOnly(sumWriter, jobFileName, jobName, cycleName);
		//	var builder = new PowertrainBuilder(dataWriter, sumWriterDecorator, engineOnly: true);

		//	// @@@ TODO: builder.AddEngine(engineFile);

		//	return builder.Build(cycleName);
		//}

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
				IModalDataWriter modWriter = null;

				if (_mode != FactoryMode.DeclarationMode) {
					var modFileName = Path.Combine(data.BasePath, data.JobFileName.Replace(Constants.FileExtensions.VectoJobFile, "") +
						Constants.FileExtensions.ModDataFile);
					modWriter = new ModalDataWriter(modFileName, _mode == FactoryMode.EngineOnlyMode);
				}
				var jobName = string.Format("{0}-{1}", JobNumber, i++);
				var sumWriterDecorator = DecorateSumWriter(data.IsEngineOnly, SumWriter, data.JobFileName, jobName, data.Cycle.Name);
				var builder = new PowertrainBuilder(modWriter, sumWriterDecorator, data.IsEngineOnly);

				yield return new VectoRun(builder.Build(data));
			}
			//_runCreator.SetJobFile(jobFile);
			//foreach (var data in _runCreator.Runs()) {
			//	//for (var i = 0; i < data.Cycles.Count; i++) {
			//	var cycleName = data.Cycle;
			//	var jobName = string.Format("{0}-{1}", jobNumber, i);
			//	var modFileName = string.Format("{0}_{1}.vmod", Path.GetFileNameWithoutExtension(data.JobFileName),
			//		Path.GetFileNameWithoutExtension(cycleName));

			//	_dataWriter = new ModalDataWriter(modFileName, data.IsEngineOnly);

			//	var sumWriterDecorator = DecorateSumWriter(data.IsEngineOnly, sumWriter, data.JobFileName, jobName, cycleName);
			//	var builder = new PowertrainBuilder(_dataWriter, sumWriterDecorator, data.IsEngineOnly);

			//	builder.AddEngine(data.EngineData);

			//	if (!data.IsEngineOnly) {
			//		builder.AddVehicle(data.VehicleData);
			//		builder.AddGearbox(data.GearboxData);

			//		foreach (var aux in data.Aux) {
			//			builder.AddAuxiliary(aux.Path, aux.ID);
			//		}

			//		// @@@ TODO builder.AddDriver(data.StartStop, data.OverSpeedEcoRoll, data.LookAheadCoasting, data.AccelerationLimitingFile);
			//	}
			//	yield return builder.Build(cycleName);
			//}
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