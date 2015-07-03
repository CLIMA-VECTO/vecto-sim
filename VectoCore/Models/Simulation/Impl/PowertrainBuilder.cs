using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	/// <summary>
	/// Provides Methods to build a simulator with a powertrain step by step.
	/// </summary>
	public class PowertrainBuilder
	{
		private readonly bool _engineOnly;
		private readonly VehicleContainer _container;
		//private ICombustionEngine _engine;
		//private IGearbox _gearBox;
		//private IVehicle _vehicle;
		//private IWheels _wheels;
		//private IDriver _driver;
		//private readonly Dictionary<string, AuxiliaryData> _auxDict = new Dictionary<string, AuxiliaryData>();
		//private IPowerTrainComponent _retarder;
		//private IClutch _clutch;
		//private IPowerTrainComponent _axleGear;

		public PowertrainBuilder(IModalDataWriter dataWriter, ISummaryDataWriter sumWriter, bool engineOnly)
		{
			_engineOnly = engineOnly;
			_container = new VehicleContainer(dataWriter, sumWriter);
		}

		public VehicleContainer Build(VectoRunData data)
		{
			return _engineOnly ? BuildEngineOnly(data) : BuildFullPowertrain(data);
		}

		private VehicleContainer BuildFullPowertrain(VectoRunData data)
		{
			//throw new NotImplementedException("FullPowertrain is not fully implemented yet.");

			//todo: make distinction between time based and distance based driving cycle!
			var cycle = new TimeBasedDrivingCycle(_container, data.Cycle);

			var axleGear = new AxleGear(data.GearboxData.AxleGearData);
			var wheels = new Wheels(_container, data.VehicleData.DynamicTyreRadius);
			var driver = new Driver(_container, data.DriverData);
			var vehicle = new Vehicle(_container, data.VehicleData);
			var gearBox = new Gearbox(_container, data.GearboxData);
			var clutch = new Clutch(_container, data.EngineData);
			var retarder = new Retarder(_container, data.VehicleData.Retarder.LossMap);
			var engine = new CombustionEngine(_container, data.EngineData);


			// connect cycle --> driver --> vehicle --> wheels --> axleGear --> gearBox --> retarder --> clutch
			cycle.InShaft().Connect(driver.OutShaft());
			driver.InShaft().Connect(vehicle.OutShaft());
			vehicle.InPort().Connect(wheels.OutPort());
			wheels.InShaft().Connect(axleGear.OutShaft());
			axleGear.InShaft().Connect(gearBox.OutShaft());
			gearBox.InShaft().Connect(retarder.OutShaft());
			retarder.InShaft().Connect(clutch.OutShaft());

			// connect directAux --> engine
			IAuxiliary directAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(data.Cycle));
			directAux.InShaft().Connect(engine.OutShaft());

			// connect aux --> ... --> aux_XXX --> directAux
			var previousAux = directAux;
			foreach (var auxData in data.Aux) {
				var auxCycleData = new AuxiliaryCycleDataAdapter(data.Cycle, auxData.ID);
				IAuxiliary auxiliary = new MappingAuxiliary(_container, auxCycleData, auxData.Data);
				auxiliary.InShaft().Connect(previousAux.OutShaft());
				previousAux = auxiliary;
			}

			// connect clutch --> aux
			clutch.InShaft().Connect(previousAux.OutShaft());

			return _container;
		}

		private VehicleContainer BuildEngineOnly(VectoRunData data)
		{
			var cycle = new EngineOnlyDrivingCycle(_container, data.Cycle);

			var engine = new CombustionEngine(_container, data.EngineData);
			var gearBox = new EngineOnlyGearbox(_container);

			IAuxiliary addAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(data.Cycle));
			addAux.InShaft().Connect(engine.OutShaft());

			gearBox.InShaft().Connect(addAux.OutShaft());

			cycle.InShaft().Connect(gearBox.OutShaft());

			return _container;
			//var simulator = new VectoRun(_container, cycle);
			//return simulator;
		}


//		public void AddCycle(string cycleFile) {}

//		public void AddEngine(CombustionEngineData data)
//		{
//			_engine = new CombustionEngine(_container, data);

//			AddClutch(data);
//		}

//		public void AddClutch(CombustionEngineData engineData)
//		{
//			_clutch = new Clutch(_container, engineData);
//		}

		public IGearbox AddGearbox(GearboxData gearboxData)
		{
			//_axleGear = new AxleGear(gearboxData.AxleGearData);

			switch (gearboxData.Type) {
				case GearboxData.GearboxType.MT:
					return new Gearbox(_container, gearboxData);
				case GearboxData.GearboxType.AMT:
					return new Gearbox(_container, gearboxData);
				/*					case GearboxData.GearboxType.AT:
								_dataWriter.HasTorqueConverter = gearboxData.HasTorqueConverter;
								break;
		*/
				default:
					throw new VectoException(String.Format("Gearboxtype {0} not implemented", gearboxData.Type));
			}
		}

//		public void AddAuxiliary(string auxFileName, string auxID)
//		{
//			_auxDict[auxID] = AuxiliaryData.ReadFromFile(auxFileName);
//		}

//		public void AddDriver(VectoRunData.StartStopData startStop,
//			VectoRunData.OverSpeedEcoRollData overSpeedEcoRoll,
//			VectoRunData.LACData lookAheadCoasting, string accelerationLimitingFile)
//		{
//			if (_engineOnly) {
//				return;
//			}
//			var driverData = new DriverData(startStop, overSpeedEcoRoll, lookAheadCoasting, accelerationLimitingFile);
//			_driver = new Driver(driverData);
//		}

//		public void AddVehicle(VehicleData data)
//		{
//			if (_engineOnly) {
//				return;
//			}

//			_vehicle = new Vehicle(_container, data);
//		}

//		public void AddRetarder(string retarderFile)
//		{
//			var retarderData = RetarderLossMap.ReadFromFile(retarderFile);
//			_retarder = new Retarder(_container, retarderData);
//		}
	}
}