using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TUGraz.VectoCore.Tests.Integration
{
	public class CoachPowerTrain
	{
		public const string AccelerationFile = @"TestData\Components\Truck.vacc";

		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		public const string AxleGearLossMap = @"TestData\Components\Axle.vtlm";
		public const string GearboxIndirectLoss = @"TestData\Components\Indirect Gear.vtlm";
		public const string GearboxDirectLoss = @"TestData\Components\Direct Gear.vtlm";

		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";

		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";

		public static VectoRun CreateEngineeringRun(DrivingCycleData cycleData, string modFileName, bool overspeed = false)
		{
			var container = CreatePowerTrain(cycleData, modFileName, overspeed);

			return new DistanceRun(container);
		}


		public static VehicleContainer CreatePowerTrain(DrivingCycleData cycleData, string modFileName, bool overspeed = false)
		{
			var modalWriter = new ModalDataWriter(modFileName);
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);

			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var axleGearData = CreateAxleGearData();
			var gearboxData = CreateGearboxData();
			var vehicleData = CreateVehicleData(3300.SI<Kilogram>());
			var driverData = CreateDriverData(AccelerationFile, overspeed);

			var cycle = new DistanceBasedDrivingCycle(container, cycleData);
			var engine = new CombustionEngine(container, engineData);
			var clutch = new Clutch(container, engineData, engine.IdleController);

			dynamic tmp = Port.AddComponent(cycle, new Driver(container, driverData, new DefaultDriverStrategy()));
			tmp = Port.AddComponent(tmp, new Vehicle(container, vehicleData));
			tmp = Port.AddComponent(tmp, new Wheels(container, vehicleData.DynamicTyreRadius));
			tmp = Port.AddComponent(tmp, new Brakes(container));
			tmp = Port.AddComponent(tmp, new AxleGear(container, axleGearData));
			tmp = Port.AddComponent(tmp, new Gearbox(container, gearboxData, new AMTShiftStrategy(gearboxData, container)));
			tmp = Port.AddComponent(tmp, clutch);

			var aux = new Auxiliary(container);
			aux.AddConstant("", 0.SI<Watt>());

			tmp = Port.AddComponent(tmp, aux);

			Port.AddComponent(tmp, engine);

			engine.IdleController.RequestPort = clutch.IdleControlPort;

			return container;
		}


		private static GearboxData CreateGearboxData()
		{
			var ratios = new[] { 6.38, 4.63, 3.44, 2.59, 1.86, 1.35, 1, 0.76 };

			return new GearboxData {
				Gears = ratios.Select((ratio, i) =>
					Tuple.Create((uint)i,
						new GearData {
							FullLoadCurve = FullLoadCurve.ReadFromFile(GearboxFullLoadCurveFile),
							LossMap = (ratio != 1.0)
								? TransmissionLossMap.ReadFromFile(GearboxIndirectLoss, ratio)
								: TransmissionLossMap.ReadFromFile(GearboxDirectLoss, ratio),
							Ratio = ratio,
							ShiftPolygon = ShiftPolygon.ReadFromFile(GearboxShiftPolygonFile)
						}))
					.ToDictionary(k => k.Item1 + 1, v => v.Item2),
				ShiftTime = 2.SI<Second>(),
				Inertia = 0.SI<KilogramSquareMeter>(),
				TractionInterruption = 1.SI<Second>(),
				StartSpeed = 2.SI<MeterPerSecond>(),
				StartAcceleration = 0.6.SI<MeterPerSquareSecond>(),
				StartTorqueReserve = 0.2,
				SkipGears = true,
				TorqueReserve = 0.2,
			};
		}

		private static GearData CreateAxleGearData()
		{
			const double ratio = 3.240355;
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

		private static DriverData CreateDriverData(string accelerationFile, bool overspeed = false)
		{
			return new DriverData {
				AccelerationCurve = AccelerationCurveData.ReadFromFile(accelerationFile),
				LookAheadCoasting = new DriverData.LACData {
					Enabled = true,
					MinSpeed = 50.KMPHtoMeterPerSecond(),
					Deceleration = -0.5.SI<MeterPerSquareSecond>(),
				},
				OverSpeedEcoRoll = overspeed
					? new DriverData.OverSpeedEcoRollData() {
						Mode = DriverData.DriverMode.Overspeed,
						MinSpeed = 50.KMPHtoMeterPerSecond(),
						OverSpeed = 5.KMPHtoMeterPerSecond(),
					}
					: new DriverData.OverSpeedEcoRollData {
						Mode = DriverData.DriverMode.Off
					},
				StartStop = new VectoRunData.StartStopData {
					Enabled = false,
				}
			};
		}
	}
}