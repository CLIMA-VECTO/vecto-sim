using System.IO;
using System.Collections.Generic;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class SimulatorFactory
	{
		private static IModalDataWriter _dataWriter;

		/// <summary>
		/// Creates a simulation job for time based engine only powertrain.
		/// </summary>
		public static IVectoSimulator CreateTimeBasedEngineOnlyJob(string engineFile, string cycleFile, string jobFileName,
			string jobName, IModalDataWriter dataWriter, SummaryFileWriter sumWriter)
		{
			var sumWriterDecorator = new SumWriterDecoratorEngineOnly(sumWriter, jobFileName, jobName, cycleFile);
			var builder = new SimulatorBuilder(dataWriter, sumWriterDecorator, engineOnly: true);

			builder.AddEngine(engineFile);

			return builder.Build(cycleFile);
		}

		/// <summary>
		/// Creates powertrains and jobs from a VectoJobData Object.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="sumWriter">The sum writer.</param>
		/// <param name="jobNumber">The job number.</param>
		/// <returns></returns>
		public static IEnumerable<IVectoSimulator> CreateJobs(VectoJobData data, SummaryFileWriter sumWriter, int jobNumber)
		{
			for (var i = 0; i < data.Cycles.Count; i++) {
				var cycleFile = data.Cycles[i];
				var jobName = string.Format("{0}-{1}", jobNumber, i);
				var modFileName = string.Format("{0}_{1}.vmod", Path.GetFileNameWithoutExtension(data.JobFileName),
					Path.GetFileNameWithoutExtension(cycleFile));

				_dataWriter = new ModalDataWriter(modFileName, data.IsEngineOnly);

				var sumWriterDecorator = DecorateSumWriter(data.IsEngineOnly, sumWriter, data.JobFileName, jobName, cycleFile);
				var builder = new SimulatorBuilder(_dataWriter, sumWriterDecorator, data.IsEngineOnly);

				builder.AddEngine(data.EngineFile);

				if (!data.IsEngineOnly) {
					builder.AddVehicle(data.VehicleFile);
					builder.AddGearbox(data.GearboxFile);

					foreach (var aux in data.Aux) {
						builder.AddAuxiliary(aux.Path, aux.ID);
					}

					builder.AddDriver(data.StartStop, data.OverSpeedEcoRoll, data.LookAheadCoasting, data.AccelerationLimitingFile);
				}
				yield return builder.Build(cycleFile);
			}
		}

		/// <summary>
		/// Decorates the sum writer with a correct decorator (either EngineOnly or FullPowertrain).
		/// </summary>
		/// <param name="engineOnly">if set to <c>true</c> [engine only].</param>
		/// <param name="sumWriter">The sum writer.</param>
		/// <param name="jobFileName">Name of the job file.</param>
		/// <param name="jobName">Name of the job.</param>
		/// <param name="cycleFile">The cycle file.</param>
		/// <returns></returns>
		private static ISummaryDataWriter DecorateSumWriter(bool engineOnly, SummaryFileWriter sumWriter,
			string jobFileName, string jobName, string cycleFile)
		{
			if (engineOnly) {
				return new SumWriterDecoratorEngineOnly(sumWriter, jobFileName, jobName, cycleFile);
			}

			return new SumWriterDecoratorFullPowertrain(sumWriter, jobFileName, jobName, cycleFile);
		}

		/// <summary>
		/// Provides Methods to build a simulator with a powertrain step by step.
		/// </summary>
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

			public SimulatorBuilder(IModalDataWriter dataWriter, ISummaryDataWriter sumWriter, bool engineOnly)
			{
				_engineOnly = engineOnly;
				_container = new VehicleContainer(dataWriter, sumWriter);
			}

			public IVectoSimulator Build(string cycleFile)
			{
				return _engineOnly ? BuildEngineOnly(cycleFile) : BuildFullPowertrain(cycleFile);
			}

			private IVectoSimulator BuildFullPowertrain(string cycleFile)
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
				_vehicle.InPort().Connect(_wheels.OutPort());
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

				var simulator = new VectoSimulator(_container, cycle);
				return simulator;
			}

			private IVectoSimulator BuildEngineOnly(string cycleFile)
			{
				var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
				var cycle = new EngineOnlyDrivingCycle(_container, cycleData);

				IAuxiliary addAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(cycleData));
				addAux.InShaft().Connect(_engine.OutShaft());

				if (_gearBox == null) {
					_gearBox = new EngineOnlyGearbox(_container);
				}

				_gearBox.InShaft().Connect(addAux.OutShaft());

				cycle.InShaft().Connect(_gearBox.OutShaft());

				var simulator = new VectoSimulator(_container, cycle);
				return simulator;
			}


			public void AddCycle(string cycleFile) {}

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

			public void AddGearbox(string gearboxFile)
			{
				var gearboxData = GearboxData.ReadFromFile(gearboxFile);
				_axleGear = new AxleGear(gearboxData.AxleGearData);

				_dataWriter.HasTorqueConverter = gearboxData.HasTorqueConverter;

				//todo init gearbox with gearbox data
				_gearBox = new Gearbox(_container);
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

				var vehicleData = EngineeringModeFactory.Instance().CreateVehicleData(vehicleFile);
				//VehicleData.ReadFromFile(vehicleFile);
				_vehicle = new Vehicle(_container, vehicleData);
			}

			public void AddRetarder(string retarderFile)
			{
				var retarderData = RetarderLossMap.ReadFromFile(retarderFile);
				_retarder = new Retarder(_container, retarderData);
			}
		}
	}
}