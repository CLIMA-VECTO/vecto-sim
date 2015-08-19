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
		public const string CycleFile = @"TestData\Integration\FullPowerTrain\1-Gear-Test-dist.vdri";
		public const string CycleFileStop = @"TestData\Integration\FullPowerTrain\1-Gear-StopTest-dist.vdri";
		public const string EngineFile = @"TestData\Integration\FullPowerTrain\24t Coach.veng";
		public const string GearboxFile = @"TestData\Integration\FullPowerTrain\24t Coach-1Gear.vgbx";
		public const string GbxLossMap = @"TestData\Integration\FullPowerTrain\NoLossGbxMap.vtlm";

		public const string AccelerationFile = @"TestData\Components\Coach.vacc";
		public const string AccelerationFile2 = @"TestData\Components\Truck.vacc";

		public const double Tolerance = 0.001;

		[TestMethod]
		public void Test_FullPowertrain()
		{
			var modalWriter = new ModalDataWriter("Coach_FullPowertrain.vmod");
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
			dynamic tmp = Port.AddComponent(cycle, new Driver(container, driverData));
			tmp = Port.AddComponent(tmp, new Vehicle(container, vehicleData));
			tmp = Port.AddComponent(tmp, new Wheels(container, vehicleData.DynamicTyreRadius));
			tmp = Port.AddComponent(tmp, new Breaks(container));
			tmp = Port.AddComponent(tmp, new AxleGear(container, axleGearData));
			tmp = Port.AddComponent(tmp, new Gearbox(container, gearboxData));
			tmp = Port.AddComponent(tmp, new Clutch(container, engineData));
			Port.AddComponent(tmp, new CombustionEngine(container, engineData));

			cyclePort.Initialize();

			var absTime = 0.SI<Second>();
			var response = cyclePort.Request(absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			container.Gear = 1;
			var ds = Constants.SimulationSettings.DriveOffDistance;
			var cnt = 0;
			var doRun = true;
			while (doRun && container.Distance().Value() < 17000) {
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

					container.CommitSimulationStep(absTime, response.SimulationInterval);
					absTime += response.SimulationInterval;

					ds = container.VehicleSpeed().IsEqual(0)
						? Constants.SimulationSettings.DriveOffDistance
						: (Constants.SimulationSettings.TargetTimeInterval * container.VehicleSpeed()).Cast<Meter>();

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

		private static GearData CreateAxleGearData()
		{
			//todo change gear ratio!
			return new GearData {
				Ratio = 3.0 * 3.5,
				LossMap = TransmissionLossMap.ReadFromFile(GbxLossMap, 3.0 * 3.5)
			};
		}

		private static GearboxData CreateGearboxData()
		{
			var gear = new GearData {
				FullLoadCurve = null,
				LossMap = TransmissionLossMap.ReadFromFile(GbxLossMap, 1),
				Ratio = 6.38,
				ShiftPolygon = null
			};

			return new GearboxData {
				Gears =
					new Dictionary<uint, GearData> {
						{ 1, gear },
						{ 2, gear },
						{ 3, gear },
						{ 4, gear },
						{ 5, gear },
						{ 6, gear },
						{ 7, gear },
						{ 8, gear }
					},
				ShiftTime = 2.SI<Second>()
			};

			//return new GearboxData {
			//	FullLoadCurve = null,
			//	LossMap = TransmissionLossMap.ReadFromFile(GbxLossMap, 3.0 * 3.5),
			//	Ratio = null,
			//	ShiftPolygon = null
			//};
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