using System.Collections.Generic;
using System.IO;
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

			var simulator = builder.Build(cycleFile, resultFile);
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

				var job = builder.Build(cycle, data.FileName);

				yield return job;
			}
		}

		public class SimulatorBuilder
		{
			private readonly bool _engineOnly;
			private readonly VehicleContainer _container;
			private ICombustionEngine _engine;
			private IGearbox _gearBox;
			private IVehicle _vehicle;
			private IWheels _wheels;
			private IDriver _driver;
			private readonly Dictionary<string, AuxiliaryData> _auxDict = new Dictionary<string, AuxiliaryData>();
			private IPowerTrainComponent _retarder;
			private IClutch _clutch;
			private IPowerTrainComponent _axleGear;

			public SimulatorBuilder(bool engineOnly)
			{
				_engineOnly = engineOnly;
				_container = new VehicleContainer();
			}

			public IVectoSimulator Build(string cycleFile, string jobFile)
			{
				if (_engineOnly) {
					return BuildEngineOnly(cycleFile, jobFile);
				}
				return BuildFullPowertrain(cycleFile, jobFile);
			}

			public void AddEngine(string engineFile)
			{
				var engineData = CombustionEngineData.ReadFromFile(engineFile);
				_engine = new CombustionEngine(_container, engineData);

				AddClutch(engineFile);
			}

			public void AddClutch(string engineFile)
			{
				var engineData = CombustionEngineData.ReadFromFile(engineFile);
				_clutch = new Clutch(_container, engineData);
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
				if (_engineOnly) {
					return;
				}
				var driverData = new DriverData(startStop, overSpeedEcoRoll, lookAheadCoasting, accelerationLimitingFile);
				_driver = new Driver(driverData);
			}

			public void AddVehicle(string vehicleFile)
			{
				if (_engineOnly) {
					return;
				}

				var vehicleData = VehicleData.ReadFromFile(vehicleFile);
				_vehicle = new Vehicle(_container, vehicleData);
			}

			public void AddRetarder(string retarderFile)
			{
				var retarderData = RetarderLossMap.ReadFromFile(retarderFile);
				_retarder = new Retarder(_container, retarderData);
			}

			private IVectoSimulator BuildFullPowertrain(string cycleFile, string jobFile)
			{
				//throw new NotImplementedException("FullPowertrain is not fully implemented yet.");
				var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
				//todo: make distinction between time based and distance based driving cycle!
				var cycle = new TimeBasedDrivingCycle(_container, cycleData);

				_axleGear = null;
				_wheels = null;

				// connect cycle --> driver --> vehicle --> wheels --> axleGear --> gearBox --> retarder --> clutch
				cycle.InShaft().Connect(_driver.OutShaft());
				_driver.InShaft().Connect(_vehicle.OutShaft());
				_vehicle.InShaft().Connect(_wheels.OutShaft());
				_wheels.InShaft().Connect(_axleGear.OutShaft());
				_axleGear.InShaft().Connect(_gearBox.OutShaft());
				_gearBox.InShaft().Connect(_retarder.OutShaft());
				_retarder.InShaft().Connect(_clutch.OutShaft());

				// connect directAux --> engine
				IAuxiliary directAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(cycleData));
				directAux.InShaft().Connect(_engine.OutShaft());

				// connect aux --> ... --> aux_XXX --> directAux
				var previousAux = directAux;
				foreach (var auxData in _auxDict) {
					var auxCycleData = new AuxiliaryCycleDataAdapter(cycleData, auxData.Key);
					IAuxiliary auxiliary = new MappingAuxiliary(_container, auxCycleData, auxData.Value);
					auxiliary.InShaft().Connect(previousAux.OutShaft());
					previousAux = auxiliary;
				}

				// connect clutch --> aux
				_clutch.InShaft().Connect(previousAux.OutShaft());

				var dataWriter =
					new ModalDataWriter(string.Format("{0}_{1}.vmod", Path.GetFileNameWithoutExtension(jobFile),
						Path.GetFileNameWithoutExtension(cycleFile)));
				var simulator = new VectoSimulator(Path.GetFileNameWithoutExtension(jobFile), jobFile, _container, cycle, dataWriter);
				return simulator;
			}

			private IVectoSimulator BuildEngineOnly(string cycleFile, string jobFile)
			{
				var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
				var cycle = new EngineOnlyDrivingCycle(_container, cycleData);

				IAuxiliary addAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(cycleData));
				addAux.InShaft().Connect(_engine.OutShaft());

				_gearBox.InShaft().Connect(addAux.OutShaft());

				cycle.InShaft().Connect(_gearBox.OutShaft());

				var dataWriter =
					new ModalDataWriter(string.Format("{0}_{1}.vmod", Path.GetFileNameWithoutExtension(jobFile),
						Path.GetFileNameWithoutExtension(cycleFile)));
				var simulator = new VectoSimulator(Path.GetFileNameWithoutExtension(jobFile), jobFile, _container, cycle, dataWriter);
				return simulator;
			}
		}
	}
}