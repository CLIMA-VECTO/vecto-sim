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
		public const string AccelerationFile = @"TestData\Components\Truck.vacc";

		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		public const string AxleGearLossMap = @"TestData\Components\Axle.vtlm";
		public const string GearboxLossMap = @"TestData\Components\Indirect Gear.vtlm";
		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";

		#region Accelerate

		[TestMethod]
		public void Accelerate_20_60_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20, 0,     0",
				"1000, 60, 0,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_level.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20, 5,     0",
				"1000, 60, 5,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20, -5,     0",
				"1000, 60, -5,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20, 25,     0",
				"1000, 60, 25,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20, -25,     0",
				"1000, 60, -25,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_uphill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20, 15,     0",
				"1000, 60, 15,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_20_60_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  20, -15,     0",
				"  10, 60, -15,     0",
				"1000, 60, -15,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_20_60_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 0,     0",
				"1000, 85, 0,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_level.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_1()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 1,     0",
				"1000, 85, 1,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_uphill_1.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_2()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 2,     0",
				"1000, 85, 2,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_uphill_2.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 5,     0",
				"1000, 85, 5,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, -5,    0",
				"1000, 85, -5,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 25,    0",
				"1000, 85, 25,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, -25,    0",
				"1000, 85, -25,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_uphill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 10,    0",
				"1000, 85, 10,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_0_85_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, -15,    0",
				"1000, 85, -15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_0_85_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_stop_0_85_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  0,  0,     2",
				"  0,  85, 0,     0",
				"1000, 85, 0,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_stop_0_85_level.vmod").Run();
		}

		[TestMethod]
		public void Accelerate_85_0_level_stop()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  85, 0,     0",
				//" 999, 85, 0,     0",
				"1000,  0,  0,     2",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Accelerate_85_0_level_stop.vmod").Run();
		}

		#endregion

		#region Decelerate

		[TestMethod]
		public void Decelerate_60_20_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, 0,     0",
				"1000, 20, 0,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_level.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_uphill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, 5,     0",
				"1000, 20, 5,     0",
				"1100, 20, 5,     0"
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, -5,     0",
				"1000, 20, -5,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_uphill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, 25,     0",
				"1000, 20, 25,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, -25,     0",
				"1000, 20, -25,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_uphill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, 15,     0",
				"1000,  0, 15,     0",
				"1100,  0, 0,     0"
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_60_20_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  60, -15,     0",
				" 800, 20, -15,     0",
				"1000, 20, -15,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_60_20_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, 0,     0",
				"1000,  0,  0,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_level.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_uphill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, 5,    0",
				"1000,  0,  5,    2",
//				"1000,  0,  5,    2",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, -5,    0",
				" 500,  0,  -5,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_uphill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, 25,    0",
				"1000,  0,  25,    0",
				"1000,  0,  25,    2",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_steep_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, -25,  0",
				"1000,  0,  -25,  0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_uphill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, 15,    0",
				"1000,  0,  15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_steep_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Decelerate_80_0_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, -15,  0",
				"1000,  0,  -15,  0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Decelerate_80_0_downhill_15.vmod").Run();
		}

		#endregion

		#region Drive

		[TestMethod]
		public void Drive_80_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, 0,    0",
				"1000,  80, 0,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_80_level.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_uphill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, 5,    0",
				"1000,  80, 5,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_80_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_downhill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  80, -5,    0",
				" 1000, 80,  -5,   0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_80_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_20_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>, <v>, <grad>, <stop>
				"   0,  20,  -15,    0",
				" 500,  20,  -25,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_20_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_30_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>, <v>, <grad>, <stop>
				"   0,  30,  -15,    0",
				" 500,  30,  -15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_30_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_50_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>, <v>, <grad>, <stop>
				"   0,  50,  -15,    0",
				" 500,  50,  -15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_50_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_uphill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0, 80, 25,    0",
				" 500, 80, 25,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_80_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>, <v>, <grad>, <stop>
				"   0,  80,  -15,    0",
				" 500,  80,  -15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_80_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_80_uphill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0, 80, 15,    0",
				" 500, 80, 15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_80_uphill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  10, 0,    0",
				"1000,  10, 0,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_level.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_uphill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  10, 5,    0",
				"1000,  10, 5,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_uphill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_downhill_5()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0,  10, -5,    0",
				" 1000, 10,  -5,   0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_downhill_5.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_downhill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>, <v>, <grad>, <stop>
				"   0,  10,  -25,    0",
				" 500,  10,  -25,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_downhill_25.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_uphill_25()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0, 10, 25,    0",
				" 500, 10, 25,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_uphill_25.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_downhill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>, <v>, <grad>, <stop>
				"   0,  10,  -15,    0",
				" 500,  10,  -15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_downhill_15.vmod").Run();
		}

		[TestMethod]
		public void Drive_10_uphill_15()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"   0, 10, 15,    0",
				" 500, 10, 15,    0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_10_uphill_15.vmod").Run();
		}

		#endregion

		[TestMethod]
		public void Drive_stop_85_stop_85_level()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,   0, 0,     2",
				"1000, 85, 0,     0",
				"2000,   0, 0,     2",
				"3000, 85, 0,     0",
			});
			CreatePowerTrain(cycle, "DriverStrategy_Drive_stop_85_stop_85_level.vmod").Run();
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
			dynamic tmp = Port.AddComponent(cycle, new Driver(container, driverData, new DefaultDriverStrategy()));
			tmp = Port.AddComponent(tmp, new Vehicle(container, vehicleData));
			tmp = Port.AddComponent(tmp, new Wheels(container, vehicleData.DynamicTyreRadius));
			tmp = Port.AddComponent(tmp, new Brakes(container));
			tmp = Port.AddComponent(tmp, new AxleGear(container, axleGearData));
			tmp = Port.AddComponent(tmp, new Gearbox(container, gearboxData));
			tmp = Port.AddComponent(tmp, new Clutch(container, engineData));

			var aux = new Auxiliary(container);
			aux.AddConstant("", 0.SI<Watt>());

			tmp = Port.AddComponent(tmp, aux);

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
				Inertia = 0.SI<KilogramSquareMeter>(),
				TractionInterruption = 1.SI<Second>(),
			};
		}


		private static GearData CreateAxleGearData()
		{
			var ratio = 3.240355;
			return new GearData {
				Ratio = ratio,
				LossMap = TransmissionLossMap.ReadFromFile(AxleGearLossMap, ratio)
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