using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;
using Wheels = TUGraz.VectoCore.Models.SimulationComponent.Impl.Wheels;

namespace TUGraz.VectoCore.Tests.Integration.DriverStrategy
{
	[TestClass]
	public class DriverStrategyTest
	{
		public const string AccelerationFile = @"TestData\Components\Coach.vacc";

		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		public const string GearboxLossMap = @"TestData\Components\Indirect Gear.vtlm";
		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";


		[TestMethod]
		public void Accelerate_0_80_level()
		{
			var cycle = CreateCycleData(new string[] {
				"  0, 0,0,2",
				"  0,80,0,0",
				"900,80,0,0",
			});

			var run = CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_80_level.vmod");

			run.Run();
		}


		[TestMethod]
		public void Accelerate_80_0_level()
		{
			var cycle = CreateCycleData(new string[] {
				"  0,80,0,0",
				"900, 0,0,0",
			});

			var run = CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_80_level.vmod");

			run.Run();
		}

		// ===============================

		public DrivingCycleData CreateCycleData(string[] entries)
		{
			var cycleData = new MemoryStream();
			var writer = new StreamWriter(cycleData);
			writer.WriteLine("<s>,<v>,<grad>,<stop>");
			foreach (var entry in entries) {
				writer.WriteLine(entry);
			}
			writer.Flush();
			cycleData.Seek(0, SeekOrigin.Begin);
			return DrivingCycleDataReader.ReadFromStream(cycleData, CycleType.DistanceBased);
		}

		public VectoRun CreatePowerTrain(DrivingCycleData cycleData, string modFileName)
		{
			var modalWriter = new ModalDataWriter(modFileName);
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);

			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			//var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CoachCycleFile);
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

			return new DistanceRun(container);
		}


		// =========================================

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