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
		public const string EngineFile = @"TestData\Integration\MinimalPowerTrain\24t Coach.veng";
		public const string GearboxFile = @"TestData\Integration\MinimalPowerTrain\24t Coach-1Gear.vgbx";
		public const string GbxLossMap = @"TestData\Integration\MinimalPowerTrain\NoLossGbxMap.vtlm";


		public const string AccelerationFile = @"TestData\Components\Coach.vacc";


		[TestMethod]
		public void TestWheelsAndEngineInitialize()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);

			var vehicleData = CreateVehicleData(50000.SI<Kilogram>());

			var driverData = CreateDriverData();

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrainOverload.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var driver = new Driver(vehicleContainer, driverData);
			dynamic tmp = AddComponent(driver, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, new CombustionEngine(vehicleContainer, engineData));

			var gbx = new DummyGearbox(vehicleContainer);

			var driverPort = driver.OutPort();

			gbx.CurrentGear = 1;

			var response = driverPort.Initialize(18.KMPHtoMeterPerSecond(), VectoMath.InclinationToAngle(0.5 / 100));


			var absTime = 0.SI<Second>();

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
		}


		[TestMethod]
		public void TestWheelsAndEngine()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFile);

			var axleGearData = CreateAxleGearData();

			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());

			var driverData = CreateDriverData();

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			dynamic tmp = AddComponent(cycle, new Driver(vehicleContainer, driverData));
			tmp = AddComponent(tmp, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
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
			while (vehicleContainer.Distance().Value() < 200) {
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
			//var run = new DistanceRun(vehicleContainer);
			//run.Run();
		}

		private GearData CreateAxleGearData()
		{
			return new GearData() {
				Ratio = 3.0 * 3.5,
				LossMap = TransmissionLossMap.ReadFromFile(GbxLossMap, 3.0 * 3.5)
			};
		}


		[TestMethod]
		public void TestWheelsAndEngineOverload()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFile);


			var vehicleData = CreateVehicleData(50000.SI<Kilogram>());

			var driverData = CreateDriverData();

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrainOverload.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			dynamic tmp = AddComponent(cycle, new Driver(vehicleContainer, driverData));
			tmp = AddComponent(tmp, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
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
			while (vehicleContainer.Distance().Value() < 200) {
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
			//var run = new DistanceRun(vehicleContainer);
			//run.Run();
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
					TwinTyres = false,
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
				AxleConfiguration = AxleConfiguration.AxleConfig_4x2,
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

		private static DriverData CreateDriverData()
		{
			return new DriverData() {
				AccelerationCurve = AccelerationCurveData.ReadFromFile(AccelerationFile),
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