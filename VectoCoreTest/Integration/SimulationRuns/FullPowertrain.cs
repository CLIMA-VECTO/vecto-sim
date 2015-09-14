using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;
using Wheels = TUGraz.VectoCore.Models.SimulationComponent.Impl.Wheels;

namespace TUGraz.VectoCore.Tests.Integration.SimulationRuns
{
	[TestClass]
	public class FullPowerTrain
	{
		private static Logger Log = LogManager.GetLogger(typeof(FullPowerTrain).ToString());

		public const string CycleFile = @"TestData\Integration\FullPowerTrain\1-Gear-Test-dist.vdri";
		public const string CoachCycleFile = @"TestData\Integration\FullPowerTrain\Coach.vdri";
		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		public const string AccelerationFile = @"TestData\Components\Coach.vacc";

		public const string GearboxLossMap = @"TestData\Components\Indirect Gear.vtlm";
		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";

		[TestMethod]
		public void Test_FullPowertrain_SimpleGearbox()
		{
			var modalWriter = new ModalDataWriter("Coach_FullPowertrain_SimpleGearbox.vmod");
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);

			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFile);
			var axleGearData = CreateAxleGearData();
			var gearboxData = CreateSimpleGearboxData();
			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());
			var driverData = CreateDriverData(AccelerationFile);

			var cycle = new DistanceBasedDrivingCycle(container, cycleData);
			var cyclePort = cycle.OutPort();
			dynamic tmp = Port.AddComponent(cycle, new Driver(container, driverData, new DefaultDriverStrategy()));
			tmp = Port.AddComponent(tmp, new Vehicle(container, vehicleData));
			tmp = Port.AddComponent(tmp, new Wheels(container, vehicleData.DynamicTyreRadius));
			tmp = Port.AddComponent(tmp, new Brakes(container));
			tmp = Port.AddComponent(tmp, new AxleGear(container, axleGearData));
			var gbx = new Gearbox(container, gearboxData);
			tmp = Port.AddComponent(tmp, gbx);
			tmp = Port.AddComponent(tmp, new Clutch(container, engineData));
			Port.AddComponent(tmp, new CombustionEngine(container, engineData));

			cyclePort.Initialize();

			var absTime = 0.SI<Second>();
			var ds = Constants.SimulationSettings.DriveOffDistance;
			IResponse response;

			var cnt = 0;
			do {
				response = cyclePort.Request(absTime, ds);
				response.Switch().
					Case<ResponseDrivingCycleDistanceExceeded>(r => ds = r.MaxDistance).
					Case<ResponseCycleFinished>(r => {}).
					Case<ResponseSuccess>(r => {
						container.CommitSimulationStep(absTime, r.SimulationInterval);
						absTime += r.SimulationInterval;

						ds = container.VehicleSpeed.IsEqual(0)
							? Constants.SimulationSettings.DriveOffDistance
							: Constants.SimulationSettings.TargetTimeInterval * container.VehicleSpeed;

						if (cnt++ % 100 == 0) {
							modalWriter.Finish();
						}
					}).
					Default(r => Assert.Fail("Unexpected Response: {0}", r));
			} while (!(response is ResponseCycleFinished));
			modalWriter.Finish();
			Assert.IsInstanceOfType(response, typeof(ResponseCycleFinished));
		}

		[TestMethod, Ignore]
		public void Test_FullPowertrain()
		{
			var modalWriter = new ModalDataWriter("Coach_FullPowertrain.vmod");
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);

			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CoachCycleFile);
			var axleGearData = CreateAxleGearData();
			var gearboxData = CreateGearboxData();
			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());
			var driverData = CreateDriverData(AccelerationFile);

			var cycle = new DistanceBasedDrivingCycle(container, cycleData);
			var cyclePort = cycle.OutPort();
			dynamic tmp = Port.AddComponent(cycle, new Driver(container, driverData, new DefaultDriverStrategy()));
			tmp = Port.AddComponent(tmp, new Vehicle(container, vehicleData));
			tmp = Port.AddComponent(tmp, new Wheels(container, vehicleData.DynamicTyreRadius));
			tmp = Port.AddComponent(tmp, new Brakes(container));
			tmp = Port.AddComponent(tmp, new AxleGear(container, axleGearData));
			var gbx = new Gearbox(container, gearboxData);
			tmp = Port.AddComponent(tmp, gbx);
			tmp = Port.AddComponent(tmp, new Clutch(container, engineData));
			Port.AddComponent(tmp, new CombustionEngine(container, engineData));

			cyclePort.Initialize();

			gbx.Gear = 0;

			var absTime = 0.SI<Second>();
			var ds = Constants.SimulationSettings.DriveOffDistance;
			var response = cyclePort.Request(absTime, ds);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			gbx.Gear = 1;
			var cnt = 0;
			while (!(response is ResponseCycleFinished) && container.Distance < 17000) {
				Log.Info("Test New Request absTime: {0}, ds: {1}", absTime, ds);
				try {
					response = cyclePort.Request(absTime, ds);
				} catch (Exception) {
					modalWriter.Finish();
					throw;
				}
				Log.Info("Test Got Response: {0},", response);

				response.Switch().
					Case<ResponseDrivingCycleDistanceExceeded>(r => ds = r.MaxDistance).
					Case<ResponseCycleFinished>(r => {}).
					Case<ResponseGearShift>(r => {
						Log.Debug("Gearshift");
					}).
					Case<ResponseSuccess>(r => {
						container.CommitSimulationStep(absTime, r.SimulationInterval);
						absTime += r.SimulationInterval;

						ds = container.VehicleSpeed.IsEqual(0)
							? Constants.SimulationSettings.DriveOffDistance
							: Constants.SimulationSettings.TargetTimeInterval * container.VehicleSpeed;

						if (cnt++ % 100 == 0) {
							modalWriter.Finish();
						}
					}).
					Default(r => Assert.Fail("Unexpected Response: {0}", r));
			}
			modalWriter.Finish();
			Assert.IsInstanceOfType(response, typeof(ResponseCycleFinished));
		}

		[TestMethod]
		public void Test_FullPowertrain_LowSpeed()
		{
			var modalWriter = new ModalDataWriter("Coach_FullPowertrain_LowSpeed.vmod");
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);

			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFile);
			var axleGearData = CreateAxleGearData();
			var gearboxData = CreateGearboxData();
			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());
			var driverData = CreateDriverData(AccelerationFile);

			var cycle = new DistanceBasedDrivingCycle(container, cycleData);
			var cyclePort = cycle.OutPort();
			dynamic tmp = Port.AddComponent(cycle, new Driver(container, driverData, new DefaultDriverStrategy()));
			tmp = Port.AddComponent(tmp, new Vehicle(container, vehicleData));
			tmp = Port.AddComponent(tmp, new Wheels(container, vehicleData.DynamicTyreRadius));
			tmp = Port.AddComponent(tmp, new Brakes(container));
			tmp = Port.AddComponent(tmp, new AxleGear(container, axleGearData));
			tmp = Port.AddComponent(tmp, new Gearbox(container, gearboxData));
			tmp = Port.AddComponent(tmp, new Clutch(container, engineData));
			Port.AddComponent(tmp, new CombustionEngine(container, engineData));

			cyclePort.Initialize();

			//container.Gear = 0;
			var absTime = 0.SI<Second>();
			var ds = Constants.SimulationSettings.DriveOffDistance;
			var response = cyclePort.Request(absTime, ds);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			//container.Gear = 1;
			var cnt = 0;
			while (!(response is ResponseCycleFinished) && container.Distance < 17000) {
				Log.Info("Test New Request absTime: {0}, ds: {1}", absTime, ds);
				try {
					response = cyclePort.Request(absTime, ds);
				} catch (Exception) {
					modalWriter.Finish();
					throw;
				}
				Log.Info("Test Got Response: {0},", response);

				response.Switch().
					Case<ResponseDrivingCycleDistanceExceeded>(r => ds = r.MaxDistance).
					Case<ResponseCycleFinished>(r => {}).
					Case<ResponseGearShift>(r => {
						Log.Debug("Gearshift");
					}).
					Case<ResponseSuccess>(r => {
						container.CommitSimulationStep(absTime, r.SimulationInterval);
						absTime += r.SimulationInterval;

						ds = container.VehicleSpeed.IsEqual(0)
							? Constants.SimulationSettings.DriveOffDistance
							: Constants.SimulationSettings.TargetTimeInterval * container.VehicleSpeed;

						if (cnt++ % 100 == 0) {
							modalWriter.Finish();
						}
					}).
					Default(r => {
						modalWriter.Finish();
						Assert.Fail("Unexpected Response: {0}", r);
					});
			}
			modalWriter.Finish();
			Assert.IsInstanceOfType(response, typeof(ResponseCycleFinished));
		}

		[TestMethod]
		public void Test_FullPowerTrain_JobFile()
		{
			var sumWriter = new SummaryFileWriter(@"job.vsum");
			var jobContainer = new JobContainer(sumWriter);

			var factory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineeringMode, @"TestData\job.vecto");

			jobContainer.AddRuns(factory);
			jobContainer.Execute();

			ResultFileHelper.TestSumFile(@"TestData\Results\Integration\job.vsum", @"job.vsum");

			ResultFileHelper.TestModFile(@"TestData\Results\Integration\job_1-Gear-Test-dist.vmod",
				@"TestData\job_1-Gear-Test-dist.vmod");
		}


		// todo: add realistic FullLoadCurve
		private static GearboxData CreateGearboxData()
		{
			var ratios = new[] { 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };

			return new GearboxData {
				Gears = ratios.Select((ratio, i) =>
					Tuple.Create((uint)i,
						new GearData {
							FullLoadCurve = FullLoadCurve.ReadFromFile(GearboxFullLoadCurveFile),
							LossMap = TransmissionLossMap.ReadFromFile(GearboxLossMap, ratio),
							Ratio = ratio,
							ShiftPolygon = ShiftPolygon.ReadFromFile(GearboxShiftPolygonFile)
						}))
					.ToDictionary(k => k.Item1 + 1, v => v.Item2),
				ShiftTime = 2.SI<Second>(),
				TractionInterruption = 1.SI<Second>(),
			};
		}


		private static GearData CreateAxleGearData()
		{
			var ratio = 3.240355;
			return new GearData {
				Ratio = ratio,
				LossMap = TransmissionLossMap.ReadFromFile(GearboxLossMap, ratio)
			};
		}

		private static GearboxData CreateSimpleGearboxData()
		{
			var ratio = 3.44;
			return new GearboxData {
				Gears = new Dictionary<uint, GearData> {
					{
						1, new GearData {
							FullLoadCurve = null,
							LossMap = TransmissionLossMap.ReadFromFile(GearboxLossMap, ratio),
							Ratio = ratio,
							ShiftPolygon = ShiftPolygon.ReadFromFile(GearboxShiftPolygonFile)
						}
					}
				},
				Inertia = 0.SI<KilogramSquareMeter>(),
				TractionInterruption = 0.SI<Second>(),
				ShiftTime = 2.SI<Second>()
			};
		}

		private static VehicleData CreateVehicleData(Kilogram loading)
		{
			var axles = new List<Axle> {
				new Axle {
					AxleWeightShare = 0.4375,
					Inertia = 21.66667.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0055,
					TwinTyres = false,
					TyreTestLoad = 62538.75.SI<Newton>()
				},
				new Axle {
					AxleWeightShare = 0.375,
					Inertia = 10.83333.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0065,
					TwinTyres = true,
					TyreTestLoad = 52532.55.SI<Newton>()
				},
				new Axle {
					AxleWeightShare = 0.1875,
					Inertia = 21.66667.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0055,
					TwinTyres = false,
					TyreTestLoad = 62538.75.SI<Newton>()
				}
			};
			return new VehicleData {
				AxleConfiguration = AxleConfiguration.AxleConfig_6x2,
				CrossSectionArea = 3.2634.SI<SquareMeter>(),
				CrossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection,
				DragCoefficient = 1,
				CurbWeight = 15700.SI<Kilogram>(),
				CurbWeigthExtra = 0.SI<Kilogram>(),
				Loading = loading,
				DynamicTyreRadius = 0.52.SI<Meter>(),
				Retarder = new RetarderData { Type = RetarderData.RetarderType.None },
				AxleData = axles,
				SavedInDeclarationMode = false,
			};
		}

		private static DriverData CreateDriverData(string accelerationFile)
		{
			return new DriverData {
				AccelerationCurve = AccelerationCurveData.ReadFromFile(accelerationFile),
				LookAheadCoasting = new DriverData.LACData {
					Enabled = false,
				},
				OverSpeedEcoRoll = new DriverData.OverSpeedEcoRollData {
					Mode = DriverData.DriverMode.Off
				},
				StartStop = new VectoRunData.StartStopData {
					Enabled = false,
				}
			};
		}
	}
}