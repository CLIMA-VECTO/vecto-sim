using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class SimulatorFactory
	{
		/// <summary>
		/// Creates a simulation job for time based engine only powertrain.
		/// </summary>
		public static IVectoSimulator CreateTimeBasedEngineOnlyJob(string engineFile, string cycleFile,
			string resultFile)
		{
			var builder = new SimulatorBuilder(engineOnly: true);

			builder.AddEngine(engineFile);
			builder.AddGearbox();

			var simulator = builder.Build(cycleFile, resultFile, jobName: "", jobFileName: "");
			return simulator;
		}

		public static IEnumerable<IVectoSimulator> CreateJobs(VectoJobData data)
		{
			foreach (var cycle in data.Cycles) {
				var builder = new SimulatorBuilder(data.IsEngineOnly);
				builder.AddVehicle(data.VehicleFile);
				builder.AddEngine(data.EngineFile);
				builder.AddGearbox(data.GearboxFile);
				foreach (var aux in data.Aux) {
					builder.AddAux(aux);
				}

				builder.AddDriver(data.StartStop, data.OverSpeedEcoRoll, data.LookAheadCoasting,
					data.AccelerationLimitingFile);

				var job = builder.Build(cycle, resultFile: "", jobName: "", jobFileName: data.FileName);

				yield return job;
			}
		}

		public class SimulatorBuilder
		{
			private bool _engineOnly;
			private VehicleContainer _container;
			private ICombustionEngine _engine;
			private IGearbox _gearBox;

			public SimulatorBuilder(bool engineOnly)
			{
				_engineOnly = engineOnly;
				_container = new VehicleContainer();
			}

			public void AddVehicle(string vehicleFile)
			{
				throw new NotImplementedException();
			}

			public void AddEngine(string engineFile)
			{
				var engineData = CombustionEngineData.ReadFromFile(engineFile);
				_engine = new CombustionEngine(_container, engineData);
			}

			public void AddGearbox(string gearboxFile = null)
			{
				if (_engineOnly) {
					_gearBox = new EngineOnlyGearbox(_container);
				} else {
					_gearBox = new Gearbox(_container);
				}
			}

			public void AddAux(VectoJobData.Data.DataBody.AuxData aux)
			{
				throw new NotImplementedException();
			}

			public void AddDriver(VectoJobData.Data.DataBody.StartStopData startStop,
				VectoJobData.Data.DataBody.OverSpeedEcoRollData overSpeedEcoRoll,
				VectoJobData.Data.DataBody.LACData lookAheadCoasting, string accelerationLimitingFile)
			{
				throw new NotImplementedException();
			}

			public IVectoSimulator Build(string cycleFile, string resultFile, string jobName, string jobFileName)
			{
				var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
				var cycle = new EngineOnlyDrivingCycle(_container, cycleData);

				var aux = new EngineOnlyAuxiliary(_container, new AuxiliariesDemandAdapter(cycleData));
				aux.InShaft().Connect(_engine.OutShaft());

				//todo: connect other auxiliaries

				// todo: connect retarder
				// todo: connect clutch

				_gearBox.InShaft().Connect(aux.OutShaft());

				// todo: connect Axle Gear
				// todo: connect wheels
				// todo: connect vehicle
				// todo: connect driver

				cycle.InShaft().Connect(_gearBox.OutShaft());

				var dataWriter = new ModalDataWriter(resultFile);
				var simulator = new VectoSimulator(jobName, jobFileName, _container, cycle, dataWriter);
				return simulator;
			}
		}
	}
}