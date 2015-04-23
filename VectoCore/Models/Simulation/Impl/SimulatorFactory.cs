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
					builder.AddAuxiliary(aux.Path, aux.ID);
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
			//private IVehicle _vehicle;
			//private IWheels _wheels;
			//private IDriver _driver;
			private Dictionary<string, AuxiliaryData> _auxDict = new Dictionary<string, AuxiliaryData>();

			public SimulatorBuilder(bool engineOnly)
			{
				_engineOnly = engineOnly;
				_container = new VehicleContainer();
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

			public void AddAuxiliary(string auxFileName, string auxID)
			{
				_auxDict[auxID] = AuxiliaryData.ReadFromFile(auxFileName);
			}


			public void AddDriver(VectoJobData.Data.DataBody.StartStopData startStop,
				VectoJobData.Data.DataBody.OverSpeedEcoRollData overSpeedEcoRoll,
				VectoJobData.Data.DataBody.LACData lookAheadCoasting, string accelerationLimitingFile)
			{
				throw new NotImplementedException("Vehicle is not implemented yet.");
				//var driverData = new DriverData(startStop, overSpeedEcoRoll, lookAheadCoasting, accelerationLimitingFile);
				//_driver = new Driver(driverData);
			}

			public void AddVehicle(string vehicleFile)
			{
				throw new NotImplementedException("Vehicle is not implemented yet.");
				//var vehicleData = VehicleData.ReadFromFile(vehicleFile);
				//_vehicle = new Vehicle(_container, vehicleData);
			}

			public IVectoSimulator Build(string cycleFile, string resultFile, string jobName, string jobFileName)
			{
				if (_engineOnly) {
					return BuildEngineOnly(cycleFile, resultFile, jobName, jobFileName);
				}
				return BuildFullPowertrain(cycleFile, resultFile, jobName, jobFileName);
			}

			private IVectoSimulator BuildFullPowertrain(string cycleFile, string resultFile, string jobName, string jobFileName)
			{
				throw new NotImplementedException("FullPowertrain is not fully implemented yet.");
				//var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
				////todo: make distinction between time based and distance based driving cycle!
				//var cycle = new TimeBasedDrivingCycle(_container, cycleData);

				//IAuxiliary aux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(cycleData));
				//aux.InShaft().Connect(_engine.OutShaft());
				//var previousAux = aux;

				//foreach (var auxData in _auxDict) {
				//	var auxCycleData = new AuxiliaryCycleDataAdapter(cycleData, auxData.Key);
				//	IAuxiliary auxiliary = new MappingAuxiliary(_container, auxCycleData, auxData.Value);
				//	auxiliary.InShaft().Connect(previousAux.OutShaft());
				//	previousAux = auxiliary;
				//}

				//retarder.InShaft().Connect(previousAux.OutShaft());
				//clutch.InShaft().Connect(retarder.OutShaft());
				//_gearBox.InShaft().Connect(prevAux.OutShaft());
				//_axleGear.InShaft().Connect(_gearBox.OutShaft());
				//_wheels.InShaft().Connect(_axleGear.OutShaft());
				//_vehicle.InShaft().Connect(_wheels.OutShaft());
				//_driver.InShaft().Connect(_vehicle.OutShaft());
				//cycle.InShaft().Connect(_gearBox.OutShaft());

				//var dataWriter = new ModalDataWriter(resultFile);
				//var simulator = new VectoSimulator(jobName, jobFileName, _container, cycle, dataWriter);
				//return simulator;
			}

			private IVectoSimulator BuildEngineOnly(string cycleFile, string resultFile, string jobName, string jobFileName)
			{
				var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
				var cycle = new EngineOnlyDrivingCycle(_container, cycleData);

				IAuxiliary addAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(cycleData));
				addAux.InShaft().Connect(_engine.OutShaft());

				_gearBox.InShaft().Connect(addAux.OutShaft());

				cycle.InShaft().Connect(_gearBox.OutShaft());

				var dataWriter = new ModalDataWriter(resultFile);
				var simulator = new VectoSimulator(jobName, jobFileName, _container, cycle, dataWriter);
				return simulator;
			}
		}
	}
}