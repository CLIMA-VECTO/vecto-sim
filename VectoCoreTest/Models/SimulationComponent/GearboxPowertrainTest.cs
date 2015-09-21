using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;
using Wheels = TUGraz.VectoCore.Models.SimulationComponent.Impl.Wheels;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class GearboxPowertrainTest
	{
		public const string AccelerationFile = @"TestData\Components\Truck.vacc";

		public const string EngineFile = @"TestData\Components\40t_Long_Haul_Truck.veng";

		public const string AxleGearLossMap = @"TestData\Components\Axle.vtlm";
		public const string GearboxLossMap = @"TestData\Components\Indirect Gear.vtlm";
		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";

		[TestMethod]
		public void Gearbox_Initialize_Empty()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,   0, 2.95016969027809,     1",
				"1000, 60, 2.95016969027809,     0",
			});
			var container = CreatePowerTrain(cycle, "Gearbox_Initialize.vmod", 7500.0.SI<Kilogram>(), 0.SI<Kilogram>());
			var retVal = container.Cycle.Initialize();
			Assert.AreEqual(5u, container.Gear);
			Assert.IsInstanceOfType(retVal, typeof(ResponseSuccess));

			AssertHelper.AreRelativeEqual(560.RPMtoRad(), container.EngineSpeed);

			var absTime = 0.SI<Second>();
			var ds = 1.SI<Meter>();

			retVal = container.Cycle.Request(absTime, ds);
			absTime += retVal.SimulationInterval;

			AssertHelper.AreRelativeEqual(560.RPMtoRad(), container.EngineSpeed);
			container.Cycle.Request(absTime, ds);

			AssertHelper.AreRelativeEqual(593.RPMtoRad(), container.EngineSpeed);
		}

		[TestMethod]
		public void Gearbox_Initialize_RefLoad()
		{
			var cycle = CreateCycleData(new[] {
				// <s>,<v>,<grad>,<stop>
				"  0,   0, 2.95016969027809,     0",
				"1000, 60, 2.95016969027809,     0",
			});
			var container = CreatePowerTrain(cycle, "Gearbox_Initialize.vmod", 7500.0.SI<Kilogram>(), 19300.SI<Kilogram>());
			var retVal = container.Cycle.Initialize();
			Assert.AreEqual(4u, container.Gear);
			Assert.IsInstanceOfType(retVal, typeof(ResponseSuccess));
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

		public VehicleContainer CreatePowerTrain(DrivingCycleData cycleData, string modFileName, Kilogram massExtra,
			Kilogram loading)
		{
			var modalWriter = new ModalDataWriter(modFileName);
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);


			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			//var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CoachCycleFile);
			var axleGearData = CreateAxleGearData();
			var gearboxData = CreateGearboxData(engineData);
			var vehicleData = CreateVehicleData(massExtra, loading);
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

			return container;
		}


		// =========================================

		private static GearboxData CreateGearboxData(CombustionEngineData engineData)
		{
			var ratios = new[] { 14.93, 11.64, 9.02, 7.04, 5.64, 4.4, 3.39, 2.65, 2.05, 1.6, 1.28, 1.0, };

			return new GearboxData {
				Gears = ratios.Select((ratio, i) =>
					Tuple.Create((uint)i,
						new GearData {
							FullLoadCurve = FullLoadCurve.ReadFromFile(GearboxFullLoadCurveFile),
							LossMap = TransmissionLossMap.ReadFromFile(GearboxLossMap, ratio),
							Ratio = ratio,
							//ShiftPolygon = ShiftPolygon.ReadFromFile(GearboxShiftPolygonFile),
							ShiftPolygon = DeclarationData.Gearbox.ComputeShiftPolygon(engineData.FullLoadCurve, engineData.IdleSpeed)
						}))
					.ToDictionary(k => k.Item1 + 1, v => v.Item2),
				ShiftTime = 2.SI<Second>(),
				Inertia = 0.SI<KilogramSquareMeter>(),
				TractionInterruption = 1.SI<Second>(),
				StartAcceleration = 0.6.SI<MeterPerSquareSecond>(),
				StartSpeed = 2.SI<MeterPerSecond>(),
			};
		}


		private static GearData CreateAxleGearData()
		{
			var ratio = 2.59;
			return new GearData {
				Ratio = ratio,
				LossMap = TransmissionLossMap.ReadFromFile(AxleGearLossMap, ratio)
			};
		}

		private static VehicleData CreateVehicleData(Kilogram massExtra, Kilogram loading)
		{
			var axles = new List<Axle> {
				new Axle {
					AxleWeightShare = 0.2,
					Inertia = 14.9.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0055,
					TwinTyres = false,
					TyreTestLoad = 31300.SI<Newton>()
				},
				new Axle {
					AxleWeightShare = 0.2,
					Inertia = 14.9.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0065,
					TwinTyres = true,
					TyreTestLoad = 31300.SI<Newton>()
				},
				new Axle {
					AxleWeightShare = 0.55 / 3,
					TwinTyres = DeclarationData.Trailer.TwinTyres,
					RollResistanceCoefficient = DeclarationData.Trailer.RollResistanceCoefficient,
					TyreTestLoad = DeclarationData.Trailer.TyreTestLoad.SI<Newton>(),
					Inertia = DeclarationData.Wheels.Lookup(DeclarationData.Trailer.WheelsType).Inertia
				},
				new Axle {
					AxleWeightShare = 0.55 / 3,
					TwinTyres = DeclarationData.Trailer.TwinTyres,
					RollResistanceCoefficient = DeclarationData.Trailer.RollResistanceCoefficient,
					TyreTestLoad = DeclarationData.Trailer.TyreTestLoad.SI<Newton>(),
					Inertia = DeclarationData.Wheels.Lookup(DeclarationData.Trailer.WheelsType).Inertia
				},
				new Axle {
					AxleWeightShare = 0.55 / 3,
					TwinTyres = DeclarationData.Trailer.TwinTyres,
					RollResistanceCoefficient = DeclarationData.Trailer.RollResistanceCoefficient,
					TyreTestLoad = DeclarationData.Trailer.TyreTestLoad.SI<Newton>(),
					Inertia = DeclarationData.Wheels.Lookup(DeclarationData.Trailer.WheelsType).Inertia
				}
			};
			return new VehicleData {
				AxleConfiguration = AxleConfiguration.AxleConfig_4x2,
				CrossSectionArea = 9.5.SI<SquareMeter>(),
				CrossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection,
				DragCoefficient = 1,
				CurbWeight = 7100.SI<Kilogram>(),
				CurbWeigthExtra = massExtra,
				Loading = loading,
				DynamicTyreRadius = 0.4882675.SI<Meter>(),
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
					Enabled = true,
					MinSpeed = 50.KMPHtoMeterPerSecond(),
					Deceleration = -0.5.SI<MeterPerSquareSecond>(),
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