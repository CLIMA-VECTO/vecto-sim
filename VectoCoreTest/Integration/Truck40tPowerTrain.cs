using System;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.FileIO.Reader.DataObjectAdaper;
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
	// ReSharper disable once InconsistentNaming
	public class Truck40tPowerTrain
	{
		public const string ShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string AccelerationFile = @"TestData\Components\Truck.vacc";
		public const string EngineFile = @"TestData\Components\40t_Long_Haul_Truck.veng";
		public const string AxleGearLossMap = @"TestData\Components\Axle 40t Truck.vtlm";
		public const string GearboxIndirectLoss = @"TestData\Components\Indirect Gear.vtlm";
		public const string GearboxDirectLoss = @"TestData\Components\Direct Gear.vtlm";
		public const string GearboxShiftPolygonFile = @"TestData\Components\ShiftPolygons.vgbs";
		public const string GearboxFullLoadCurveFile = @"TestData\Components\Gearbox.vfld";

		public static VectoRun CreateEngineeringRun(DrivingCycleData cycleData, string modFileName, bool overspeed = false)
		{
			var container = CreatePowerTrain(cycleData, modFileName, 7500.SI<Kilogram>(), 19300.SI<Kilogram>(), overspeed);

			return new DistanceRun(container);
		}

		public static VectoRun CreateEngineeringRun(DrivingCycleData cycleData, string modFileName, Kilogram massExtra,
			Kilogram loading)
		{
			var container = CreatePowerTrain(cycleData, modFileName, massExtra, loading);

			return new DistanceRun(container);
		}

		public static VehicleContainer CreatePowerTrain(DrivingCycleData cycleData, string modFileName, Kilogram massExtra,
			Kilogram loading, bool overspeed = false)
		{
			var modalWriter = new ModalDataWriter(modFileName);
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(modalWriter, sumWriter);

			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var axleGearData = CreateAxleGearData();
			var gearboxData = CreateGearboxData(engineData);
			var vehicleData = CreateVehicleData(massExtra, loading);
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

		private static GearboxData CreateGearboxData(CombustionEngineData engineData)
		{
			var ratios = new[] { 14.93, 11.64, 9.02, 7.04, 5.64, 4.4, 3.39, 2.65, 2.05, 1.6, 1.28, 1.0, };

			return new GearboxData {
				Gears = ratios.Select((ratio, i) =>
					Tuple.Create((uint)i,
						new GearData {
							FullLoadCurve = FullLoadCurve.ReadFromFile(GearboxFullLoadCurveFile),
							LossMap =
								(ratio != 1.0)
									? TransmissionLossMap.ReadFromFile(GearboxIndirectLoss, ratio)
									: TransmissionLossMap.ReadFromFile(GearboxDirectLoss, ratio),
							Ratio = ratio,
							ShiftPolygon = ShiftPolygon.ReadFromFile(ShiftPolygonFile),
							//ShiftPolygon =    DeclarationData.Gearbox.ComputeShiftPolygon(engineData.FullLoadCurve, engineData.IdleSpeed)
						}))
					.ToDictionary(k => k.Item1 + 1, v => v.Item2),
				ShiftTime = 2.SI<Second>(),
				Inertia = 0.SI<KilogramSquareMeter>(),
				TractionInterruption = 1.SI<Second>(),
				StartAcceleration = 0.6.SI<MeterPerSquareSecond>(),
				StartSpeed = 2.SI<MeterPerSecond>(),
				TorqueReserve = 0.2,
				StartTorqueReserve = 0.2,
				SkipGears = true,
				EarlyShiftUp = true,
			};
		}

		private static GearData CreateAxleGearData()
		{
			const double ratio = 2.59;
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
					AxleWeightShare = 0.25,
					Inertia = 14.9.SI<KilogramSquareMeter>(),
					RollResistanceCoefficient = 0.0065,
					TwinTyres = true,
					TyreTestLoad = 31300.SI<Newton>()
				},

				// trailer - declaration wheel data
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
				AerodynamicDragAera = 6.2985.SI<SquareMeter>(),
				CrossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection,
				CurbWeight = 7100.SI<Kilogram>(),
				CurbWeigthExtra = massExtra,
				Loading = loading,
				DynamicTyreRadius = 0.4882675.SI<Meter>(),
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