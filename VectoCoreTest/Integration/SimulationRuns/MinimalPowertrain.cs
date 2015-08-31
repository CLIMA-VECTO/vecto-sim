using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;
using Wheels = TUGraz.VectoCore.Models.SimulationComponent.Impl.Wheels;

namespace TUGraz.VectoCore.Tests.Integration.SimulationRuns
{
	[TestClass]
	public class MinimalPowertrain
	{
		public const string CycleFile = @"TestData\Integration\MinimalPowerTrain\1-Gear-Test-dist.vdri";
		public const string CycleFileStop = @"TestData\Integration\MinimalPowerTrain\1-Gear-StopTest-dist.vdri";
		public const string EngineFile = @"TestData\Integration\MinimalPowerTrain\24t Coach.veng";
		public const string GearboxFile = @"TestData\Integration\MinimalPowerTrain\24t Coach-1Gear.vgbx";
		public const string GbxLossMap = @"TestData\Integration\MinimalPowerTrain\NoLossGbxMap.vtlm";


		public const string AccelerationFile = @"TestData\Components\Coach.vacc";
		public const string AccelerationFile2 = @"TestData\Components\Truck.vacc";

		public const double Tolerance = 0.001;


		[TestMethod]
		public void TestWheelsAndEngineInitialize()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);

			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());

			var axleGearData = CreateAxleGearData();

			var driverData = CreateDriverData(AccelerationFile);

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrainOverload.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var driver = new Driver(vehicleContainer, driverData);
			dynamic tmp = AddComponent(driver, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new AxleGear(vehicleContainer, axleGearData));

			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			var engine = new CombustionEngine(vehicleContainer, engineData);
			AddComponent(tmp, engine);

			var gbx = new DummyGearbox(vehicleContainer);

			var driverPort = driver.OutPort();

			gbx.CurrentGear = 1;

			var response = driverPort.Initialize(18.KMPHtoMeterPerSecond(), VectoMath.InclinationToAngle(2.842372037 / 100));


			var absTime = 0.SI<Second>();

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

//			time [s] , dist [m] , v_act [km/h] , v_targ [km/h] , acc [m/s²] , grad [%] , n [1/min] , Tq_eng [Nm] , Tq_clutch [Nm] , Tq_full [Nm] , Tq_drag [Nm] , Pe_eng [kW] , Pe_full [kW] , Pe_drag [kW] , Pe_clutch [kW] , Pa Eng [kW] , Paux [kW] , Gear [-] , Ploss GB [kW] , Ploss Diff [kW] , Ploss Retarder [kW] , Pa GB [kW] , Pa Veh [kW] , Proll [kW] , Pair [kW] , Pgrad [kW] , Pwheel [kW] , Pbrake [kW] , FC-Map [g/h] , FC-AUXc [g/h] , FC-WHTCc [g/h]
//			1.5      , 5        , 18           , 18            , 0          , 2.842372 , 964.1117  , 323.7562    , 323.7562       , 2208.664     , -158.0261    , 32.68693    , 222.9902     , -15.95456    , 32.68693       , 0           , 0         , 1        , 0             , 0               , 0                   , 0          , 0           , 5.965827   , 0.2423075 , 26.47879   , 32.68693    , 0           , 7574.113     , -             , -

			AssertHelper.AreRelativeEqual(964.1117.RPMtoRad().Value(), vehicleContainer.Engine.EngineSpeed().Value());
			Assert.AreEqual(2208.664, engine._previousState.StationaryFullLoadTorque.Value(), Tolerance);
			Assert.AreEqual(-158.0261, engine._previousState.FullDragTorque.Value(), Tolerance);

			Assert.AreEqual(323.7562, engine._previousState.EngineTorque.Value(), Tolerance);
		}


		[TestMethod]
		public void TestWheelsAndEngine()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFile);

			var axleGearData = CreateAxleGearData();

			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());

			var driverData = CreateDriverData(AccelerationFile);

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			dynamic tmp = AddComponent(cycle, new Driver(vehicleContainer, driverData));
			tmp = AddComponent(tmp, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Breaks(vehicleContainer));
			tmp = AddComponent(tmp, new AxleGear(vehicleContainer, axleGearData));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, new CombustionEngine(vehicleContainer, engineData));

			var gbx = new DummyGearbox(vehicleContainer);

			var cyclePort = cycle.OutPort();

			cyclePort.Initialize();

			gbx.CurrentGear = 0;

			var absTime = 0.SI<Second>();
			var response = cyclePort.Request(absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			gbx.CurrentGear = 1;
			var ds = Constants.SimulationSettings.DriveOffDistance;
			var cnt = 0;
			var doRun = true;
			while (doRun && vehicleContainer.Distance().Value() < 17000) {
				response = cyclePort.Request(absTime, ds);

				switch (response.ResponseType) {
					case ResponseType.DrivingCycleDistanceExceeded:
						var rsp = response as ResponseDrivingCycleDistanceExceeded;
						ds = rsp.MaxDistance;
						continue;
					case ResponseType.CycleFinished:
						doRun = false;
						break;
				}
				if (doRun) {
					Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
	
					vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
					absTime += response.SimulationInterval;

					ds = vehicleContainer.VehicleSpeed().IsEqual(0)
						? Constants.SimulationSettings.DriveOffDistance
						: (Constants.SimulationSettings.TargetTimeInterval * vehicleContainer.VehicleSpeed()).Cast<Meter>();

					if (cnt++ % 100 == 0) {
						modalWriter.Finish();
					}
				}
			}

			Assert.IsInstanceOfType(response, typeof(ResponseCycleFinished));

			modalWriter.Finish();
			//var run = new DistanceRun(vehicleContainer);
			//run.Run();
		}

		[TestMethod]
		public void TestWheelsAndEngineLookahead()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFileStop);

			var axleGearData = CreateAxleGearData();

			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());

			var driverData = CreateDriverData(AccelerationFile2);

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain_Stop.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			dynamic tmp = AddComponent(cycle, new Driver(vehicleContainer, driverData));
			tmp = AddComponent(tmp, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Breaks(vehicleContainer));
			tmp = AddComponent(tmp, new AxleGear(vehicleContainer, axleGearData));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, new CombustionEngine(vehicleContainer, engineData));

			var gbx = new DummyGearbox(vehicleContainer);

			var cyclePort = cycle.OutPort();

			cyclePort.Initialize();

			gbx.CurrentGear = 0;

			var absTime = 0.SI<Second>();

			gbx.CurrentGear = 1;
			IResponse response;
			var ds = Constants.SimulationSettings.DriveOffDistance;
			while (vehicleContainer.Distance().Value() < 100) {
				response = cyclePort.Request(absTime, ds);

				switch (response.ResponseType) {
					case ResponseType.DrivingCycleDistanceExceeded:
						var rsp = response as ResponseDrivingCycleDistanceExceeded;
						ds = rsp.MaxDistance;
						continue;
				}
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));


				vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
				absTime += response.SimulationInterval;

				ds = vehicleContainer.VehicleSpeed().IsEqual(0)
					? Constants.SimulationSettings.DriveOffDistance
					: (Constants.SimulationSettings.TargetTimeInterval * vehicleContainer.VehicleSpeed()).Cast<Meter>();

				modalWriter.Finish();
			}

			modalWriter.Finish();
		}

		private static GearData CreateAxleGearData()
		{
			return new GearData() {
				Ratio = 3.0 * 3.5,
				LossMap = TransmissionLossMap.ReadFromFile(GbxLossMap, 3.0 * 3.5)
			};
		}


		private static VehicleData CreateVehicleData(Kilogram loading)
		{
			var axles = new List<Axle>() {
				new Axle() {
					AxleWeightShare = 0.4375,
					Inertia = 21.66667.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0055,
					TwinTyres = false,
					TyreTestLoad = 62538.75.SI<Newton>()
				},
				new Axle() {
					AxleWeightShare = 0.375,
					Inertia = 10.83333.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0065,
					TwinTyres = true,
					TyreTestLoad = 52532.55.SI<Newton>()
				},
				new Axle() {
					AxleWeightShare = 0.1875,
					Inertia = 21.66667.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0055,
					TwinTyres = false,
					TyreTestLoad = 62538.75.SI<Newton>()
				}
			};
			return new VehicleData() {
				AxleConfiguration = AxleConfiguration.AxleConfig_6x2,
				CrossSectionArea = 3.2634.SI<SquareMeter>(),
				CrossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection,
				DragCoefficient = 1,
				CurbWeight = 15700.SI<Kilogram>(),
				CurbWeigthExtra = 0.SI<Kilogram>(),
				Loading = loading,
				DynamicTyreRadius = 0.52.SI<Meter>(),
				Retarder = new RetarderData() { Type = RetarderData.RetarderType.None },
				AxleData = axles,
				SavedInDeclarationMode = false,
			};
		}

		private static DriverData CreateDriverData(string accelerationFile)
		{
			return new DriverData() {
				AccelerationCurve = AccelerationCurveData.ReadFromFile(accelerationFile),
				LookAheadCoasting = new DriverData.LACData() {
					Enabled = false,
				},
				OverSpeedEcoRoll = new DriverData.OverSpeedEcoRollData() {
					Mode = VectoCore.Models.SimulationComponent.Data.DriverData.DriverMode.Off
				},
				StartStop = new VectoRunData.StartStopData() {
					Enabled = false,
				}
			};
		}


		// ========================

		protected virtual IDriver AddComponent(IDrivingCycle prev, IDriver next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IVehicle AddComponent(IDriver prev, IVehicle next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IWheels AddComponent(IFvInProvider prev, IWheels next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}


		protected virtual ITnOutProvider AddComponent(IWheels prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IPowerTrainComponent AddComponent(IPowerTrainComponent prev, IPowerTrainComponent next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual void AddComponent(IPowerTrainComponent prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
		}
	}
}