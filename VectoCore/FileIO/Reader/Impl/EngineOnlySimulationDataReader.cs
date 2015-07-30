using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.EngineeringFile;
using TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.FileIO.Reader.Impl
{
	public class EngineOnlySimulationDataReader : EngineeringModeSimulationDataReader
	{
		public override IEnumerable<VectoRunData> NextRun()
		{
			var job = Job as VectoJobFileV2Engineering;
			if (job == null) {
				Log.Warn("Job-file is null or unsupported version");
				yield break;
			}
			var dao = new EngineeringDataAdapter();
			foreach (var cycle in job.Body.Cycles) {
				var simulationRunData = new VectoRunData() {
					BasePath = job.BasePath,
					JobFileName = job.JobFile,
					EngineData = dao.CreateEngineData(Engine),
					Cycle = DrivingCycleDataReader.ReadFromFileEngineOnly(Path.Combine(job.BasePath, cycle)),
					IsEngineOnly = IsEngineOnly
				};
				yield return simulationRunData;
			}
		}

		protected override void ProcessJob(VectoJobFile vectoJob)
		{
			var declaration = vectoJob as VectoJobFileV2Engineering;
			if (declaration == null) {
				throw new VectoException("Unhandled Job File Format");
			}
			var job = declaration;

			ReadEngine(Path.Combine(job.BasePath, job.Body.EngineFile));
		}

		public override bool IsEngineOnly
		{
			get { return true; }
		}
	}
}