﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models
{
	[TestClass]
	public class DeclarationDataTest
	{
		public const double Tolerance = 0.0001;

		public static void AssertException<T>(Action func, string message = null) where T : Exception
		{
			try {
				func();
				Assert.Fail("Expected an exception.");
			} catch (T ex) {
				if (!string.IsNullOrEmpty(message)) {
					Assert.AreEqual(message, ex.Message);
				}
			}
		}


		[TestMethod]
		public void WheelDataTest()
		{
			var wheels = DeclarationData.Wheels;

			var tmp = wheels.Lookup("285/70 R 19.5");

			Assert.AreEqual(7.9, tmp.Inertia.Double(), Tolerance);
			Assert.AreEqual(0.8943, tmp.DynamicTyreRadius.Double(), Tolerance);
			Assert.AreEqual(0, tmp.SizeClass);
		}

		[TestMethod]
		public void RimsDataTest()
		{
			var rims = DeclarationData.Rims;

			var tmp = rims.Lookup("15° DC Rims");

			Assert.AreEqual(3.03, tmp.F_a, Tolerance);
			Assert.AreEqual(3.05, tmp.F_b, Tolerance);
		}

		[TestMethod]
		public void PT1Test()
		{
			var pt1 = DeclarationData.PT1;

			// FIXED POINTS
			Assert.AreEqual(0, pt1.Lookup(400.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.47, pt1.Lookup(800.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.58, pt1.Lookup(1000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.53, pt1.Lookup(1200.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.46, pt1.Lookup(1400.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.43, pt1.Lookup(1500.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.22, pt1.Lookup(1750.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.2, pt1.Lookup(1800.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.11, pt1.Lookup(2000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.11, pt1.Lookup(2500.RPMtoRad()).Double(), Tolerance);

			// INTERPOLATE
			Assert.AreEqual(0.235, pt1.Lookup(600.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.525, pt1.Lookup(900.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.555, pt1.Lookup(1100.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.495, pt1.Lookup(1300.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.445, pt1.Lookup(1450.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.325, pt1.Lookup(1625.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.21, pt1.Lookup(1775.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.155, pt1.Lookup(1900.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.11, pt1.Lookup(2250.RPMtoRad()).Double(), Tolerance);


			// EXTRAPOLATE 
			Assert.AreEqual(0.11, pt1.Lookup(3000.RPMtoRad()).Double(), Tolerance);
			AssertException<VectoException>(() => pt1.Lookup(200.RPMtoRad()));
			AssertException<VectoException>(() => pt1.Lookup(0.RPMtoRad()));
		}


		[TestMethod]
		public void WHTCTest()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxESTechTest()
		{
			var es = DeclarationData.ElectricSystem;

			var expected = new[] {
				new { Mission = MissionType.LongHaul, Base = 1240, LED = 1190 },
				new { Mission = MissionType.RegionalDelivery, Base = 1055, LED = 1005 },
				new { Mission = MissionType.UrbanDelivery, Base = 974, LED = 924 },
				new { Mission = MissionType.MunicipalUtility, Base = 974, LED = 924 },
				new { Mission = MissionType.Construction, Base = 975, LED = 925 },
				new { Mission = MissionType.HeavyUrban, Base = 0, LED = 0 },
				new { Mission = MissionType.Urban, Base = 0, LED = 0 },
				new { Mission = MissionType.Suburban, Base = 0, LED = 0 },
				new { Mission = MissionType.Interurban, Base = 0, LED = 0 },
				new { Mission = MissionType.Coach, Base = 0, LED = 0 }
			};
			Assert.AreEqual(expected.Length, Enum.GetValues(typeof(MissionType)).Length);

			foreach (var expectation in expected) {
				var baseConsumption = es.Lookup(expectation.Mission, technologies: new string[] { });
				var withLEDs = es.Lookup(expectation.Mission, technologies: new[] { "LED lights" });

				Assert.AreEqual(expectation.Base, baseConsumption.Double(), Tolerance);
				Assert.AreEqual(expectation.LED, withLEDs.Double(), Tolerance);
			}
		}

		[TestMethod]
		public void AuxFanTechTest()
		{
			var fan = DeclarationData.Fan;

			const string defaultFan = "Crankshaft mounted - Electronically controlled visco clutch (Default)";
			var expected = new Dictionary<string, int[]> {
				{
					"Crankshaft mounted - Electronically controlled visco clutch (Default)",
					new[] { 618, 671, 516, 566, 1037, 0, 0, 0, 0, 0 }
				}, {
					"Crankshaft mounted - Bimetallic controlled visco clutch",
					new[] { 818, 871, 676, 766, 1277, 0, 0, 0, 0, 0 }
				}, {
					"Crankshaft mounted - Discrete step clutch",
					new[] { 668, 721, 616, 616, 1157, 0, 0, 0, 0, 0 }
				}, {
					"Crankshaft mounted - On/Off clutch",
					new[] { 718, 771, 666, 666, 1237, 0, 0, 0, 0, 0 }
				}, {
					"Belt driven or driven via transm. - Electronically controlled visco clutch",
					new[] { 889, 944, 733, 833, 1378, 0, 0, 0, 0, 0 }
				}, {
					"Belt driven or driven via transm. - Bimetallic controlled visco clutch",
					new[] { 1089, 1144, 893, 1033, 1618, 0, 0, 0, 0, 0 }
				}, {
					"Belt driven or driven via transm. - Discrete step clutch",
					new[] { 939, 994, 883, 883, 1498, 0, 0, 0, 0, 0 }
				}, {
					"Belt driven or driven via transm. - On/Off clutch",
					new[] { 989, 1044, 933, 933, 1578, 0, 0, 0, 0, 0 }
				}, {
					"Hydraulic driven - Variable displacement pump",
					new[] { 738, 955, 632, 717, 1672, 0, 0, 0, 0, 0 }
				}, {
					"Hydraulic driven - Constant displacement pump",
					new[] { 1000, 1200, 800, 900, 2100, 0, 0, 0, 0, 0 }
				}, {
					"Hydraulic driven - Electronically controlled",
					new[] { 700, 800, 600, 600, 1400, 0, 0, 0, 0, 0 }
				}
			};

			var missions = new[] {
				MissionType.LongHaul, MissionType.RegionalDelivery, MissionType.UrbanDelivery, MissionType.MunicipalUtility,
				MissionType.Construction, MissionType.HeavyUrban, MissionType.Urban,
				MissionType.Suburban, MissionType.Interurban, MissionType.Coach
			};

			Assert.AreEqual(missions.Length, Enum.GetValues(typeof(MissionType)).Length, "something wrong in the mission list.");
			Assert.IsTrue(expected.All(kv => kv.Value.Length == missions.Length), "something wrong in the test values lists.");

			for (var i = 0; i < missions.Length; i++) {
				// default
				Watt defaultValue = fan.Lookup(missions[i], "");
				Assert.AreEqual(expected[defaultFan][i], defaultValue.Double(), Tolerance);

				// all fan techs
				foreach (var expect in expected) {
					Watt value = fan.Lookup(missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Double(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxHVACTest()
		{
			var hvac = DeclarationData.HVAC;

			var expected = new Dictionary<string, int[]> {
				{ "1", new[] { 0, 150, 150, 0, 0, 0, 0, 0, 0, 0 } },
				{ "2", new[] { 200, 200, 150, 0, 0, 0, 0, 0, 0, 0 } },
				{ "3", new[] { 0, 200, 150, 0, 0, 0, 0, 0, 0, 0 } },
				{ "4", new[] { 350, 200, 0, 300, 0, 0, 0, 0, 0, 0 } },
				{ "5", new[] { 350, 200, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "6", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "7", new[] { 0, 0, 0, 0, 200, 0, 0, 0, 0, 0 } },
				{ "8", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "9", new[] { 350, 200, 0, 300, 0, 0, 0, 0, 0, 0 } },
				{ "10", new[] { 350, 200, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "11", new[] { 0, 0, 0, 0, 200, 0, 0, 0, 0, 0 } },
				{ "12", new[] { 0, 0, 0, 0, 200, 0, 0, 0, 0, 0 } }
			};

			var missions = new[] {
				MissionType.LongHaul, MissionType.RegionalDelivery, MissionType.UrbanDelivery, MissionType.MunicipalUtility,
				MissionType.Construction, MissionType.HeavyUrban, MissionType.Urban,
				MissionType.Suburban, MissionType.Interurban, MissionType.Coach
			};

			Assert.AreEqual(missions.Length, Enum.GetValues(typeof(MissionType)).Length, "something wrong in the mission list.");
			Assert.IsTrue(expected.All(kv => kv.Value.Length == missions.Length), "something wrong in the test values lists.");

			for (var i = 0; i < missions.Length; i++) {
				foreach (var expect in expected) {
					Watt value = hvac.Lookup(missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Double(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxPSTest()
		{
			var ps = DeclarationData.PneumaticSystem;

			var expected = new Dictionary<string, int[]> {
				{ "1", new[] { 0, 1300, 1240, 0, 0, 0, 0, 0, 0, 0 } },
				{ "2", new[] { 1180, 1280, 1320, 0, 0, 0, 0, 0, 0, 0 } },
				{ "3", new[] { 0, 1360, 1380, 0, 0, 0, 0, 0, 0, 0 } },
				{ "4", new[] { 1300, 1340, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "5", new[] { 1340, 1820, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "6", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "7", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "8", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "9", new[] { 1340, 1540, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "10", new[] { 1340, 1820, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "11", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ "12", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }
			};

			var missions = new[] {
				MissionType.LongHaul, MissionType.RegionalDelivery, MissionType.UrbanDelivery, MissionType.MunicipalUtility,
				MissionType.Construction, MissionType.HeavyUrban, MissionType.Urban,
				MissionType.Suburban, MissionType.Interurban, MissionType.Coach
			};

			Assert.AreEqual(missions.Length, Enum.GetValues(typeof(MissionType)).Length, "something wrong in the mission list.");
			Assert.IsTrue(expected.All(kv => kv.Value.Length == missions.Length), "something wrong in the test values lists.");

			for (var i = 0; i < missions.Length; i++) {
				foreach (var expect in expected) {
					Watt value = ps.Lookup(missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Double(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxSPTableTest()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxSPTechTest()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void SegmentTest()
		{
			//var factory = DeclarationModeFactory.Instance();
			//var job = factory.ReadJobFile("12t Delivery Truck.vecto");
			//var vehicleData = factory.ReadVehicleData(job.VehicleFile);

			//mock vehicleData
			var vehicleData = new {
				VehicleCategory = VehicleCategory.RigidTruck,
				AxleConfiguration = AxleConfiguration.AxleConfig4x2,
				GrossVehicleMassRating = 11900.SI<Kilogram>(),
				CurbWeight = 5850.SI<Kilogram>()
			};

			var segment = DeclarationData.Segments.Lookup(vehicleData.VehicleCategory, vehicleData.AxleConfiguration,
				vehicleData.GrossVehicleMassRating, vehicleData.CurbWeight);


			Assert.AreEqual("2", segment.HDVClass);

			var data = AccelerationCurveData.ReadFromStream(segment.AccelerationFile);
			TestAcceleration(data);

			Assert.AreEqual(3, segment.Missions.Length);

			var longHaulMission = segment.Missions[0];
			Assert.AreEqual(MissionType.LongHaul, longHaulMission.MissionType);

			Assert.IsNotNull(longHaulMission.CrossWindCorrectionFile);
			Assert.IsTrue(!string.IsNullOrEmpty(new StreamReader(longHaulMission.CrossWindCorrectionFile).ReadLine()));

			Assert.IsTrue(new[] { 0.4, 0.6 }.SequenceEqual(longHaulMission.AxleWeightDistribution));
			Assert.IsTrue(new double[] { }.SequenceEqual(longHaulMission.TrailerAxleWeightDistribution));
			Assert.AreEqual(1900.SI<Kilogram>(), longHaulMission.MassExtra);

			Assert.IsNotNull(longHaulMission.CycleFile);
			Assert.IsTrue(!string.IsNullOrEmpty(new StreamReader(longHaulMission.CycleFile).ReadLine()));

			Assert.AreEqual(0.SI<Kilogram>(), longHaulMission.MinLoad);
			Assert.AreEqual(0.5882 * vehicleData.GrossVehicleMassRating - 2511.8, longHaulMission.RefLoad);
			Assert.AreEqual(vehicleData.GrossVehicleMassRating - longHaulMission.MassExtra - vehicleData.CurbWeight,
				longHaulMission.MaxLoad);

			var regionalDeliveryMission = segment.Missions[1];
			Assert.AreEqual(MissionType.RegionalDelivery, regionalDeliveryMission.MissionType);

			Assert.IsNotNull(regionalDeliveryMission.CrossWindCorrectionFile);
			Assert.IsTrue(!string.IsNullOrEmpty(new StreamReader(regionalDeliveryMission.CrossWindCorrectionFile).ReadLine()));

			Assert.IsTrue(new[] { 0.45, 0.55 }.SequenceEqual(regionalDeliveryMission.AxleWeightDistribution));
			Assert.IsTrue(new double[] { }.SequenceEqual(regionalDeliveryMission.TrailerAxleWeightDistribution));
			Assert.AreEqual(1900.SI<Kilogram>(), regionalDeliveryMission.MassExtra);

			Assert.IsNotNull(regionalDeliveryMission.CycleFile);
			Assert.IsTrue(!string.IsNullOrEmpty(new StreamReader(regionalDeliveryMission.CycleFile).ReadLine()));

			Assert.AreEqual(0.SI<Kilogram>(), regionalDeliveryMission.MinLoad);
			Assert.AreEqual(0.3941 * vehicleData.GrossVehicleMassRating - 1705.9, regionalDeliveryMission.RefLoad);
			Assert.AreEqual(vehicleData.GrossVehicleMassRating - regionalDeliveryMission.MassExtra - vehicleData.CurbWeight,
				regionalDeliveryMission.MaxLoad);

			var urbanDeliveryMission = segment.Missions[2];
			Assert.AreEqual(MissionType.UrbanDelivery, urbanDeliveryMission.MissionType);

			Assert.IsNotNull(urbanDeliveryMission.CrossWindCorrectionFile);
			Assert.IsTrue(!string.IsNullOrEmpty(new StreamReader(urbanDeliveryMission.CrossWindCorrectionFile).ReadLine()));

			Assert.IsTrue(new[] { 0.45, 0.55 }.SequenceEqual(urbanDeliveryMission.AxleWeightDistribution));
			Assert.IsTrue(new double[] { }.SequenceEqual(urbanDeliveryMission.TrailerAxleWeightDistribution));
			Assert.AreEqual(1900.SI<Kilogram>(), urbanDeliveryMission.MassExtra);

			Assert.IsNotNull(urbanDeliveryMission.CycleFile);
			Assert.IsTrue(!string.IsNullOrEmpty(new StreamReader(urbanDeliveryMission.CycleFile).ReadLine()));

			Assert.AreEqual(0.SI<Kilogram>(), urbanDeliveryMission.MinLoad);
			Assert.AreEqual(0.3941 * vehicleData.GrossVehicleMassRating - 1705.9, urbanDeliveryMission.RefLoad);
			Assert.AreEqual(vehicleData.GrossVehicleMassRating - urbanDeliveryMission.MassExtra - vehicleData.CurbWeight,
				urbanDeliveryMission.MaxLoad);


			//// FACTORY
			//var runs = new List<IVectoRun>();

			//foreach (var mission in segment.Missions) {
			//	foreach (var loading in mission.Loadings) {
			//		var container = new VehicleContainer();

			//		// connect cycle --> driver --> vehicle --> wheels --> axleGear --> gearBox
			//		//         --> retarder --> clutch --> aux --> ... --> aux_XXX --> directAux --> engine
			//		var engineData = factory.ReadEngineData(job.EngineFile);
			//		var engine = new CombustionEngine(container, engineData);

			//		// todo AUX
			//		// todo clutch
			//		// todo retarder

			//		var gearboxData = factory.ReadGearboxData(job.GearboxFile);
			//		var gearbox = new Gearbox(container, gearboxData);
			//		gearbox.InShaft().Connect(engine.OutShaft());

			//		// todo axleGear

			//		var wheels = new DeclarationWheels(container, 0.SI<Meter>());

			//		var missionVehicleData = new VehicleData(vehicleData, loading, mission.AxleWeightDistribution);
			//		var vehicle = new Vehicle(container, missionVehicleData);
			//		vehicle.InPort().Connect(wheels.OutPort());

			//		var driverData = new DriverData();
			//		var driver = new Driver(driverData);
			//		driver.InShaft().Connect(vehicle.OutShaft());

			//		var cycleData = DrivingCycleData.ReadFromFileEngineOnly(mission.CycleFile);
			//		var cycle = new DistanceBasedDrivingCycle(container, cycleData);
			//		cycle.InShaft().Connect(driver.OutShaft());

			//		var simulator = new VectoRun(container, cycle);
			//		runs.Add(simulator);
			//	}
			//}
		}

		public void EqualAcceleration(AccelerationCurveData data, double velocity, double acceleration, double deceleration)
		{
			var entry = data.Lookup(velocity.SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>());
			Assert.AreEqual(entry.Acceleration.Double(), acceleration, Tolerance);
			Assert.AreEqual(entry.Deceleration.Double(), deceleration, Tolerance);
		}

		public void TestAcceleration(AccelerationCurveData data)
		{
			// FIXED POINTS
			EqualAcceleration(data, 0, 1, -1);
			EqualAcceleration(data, 25, 1, -1);
			EqualAcceleration(data, 50, 0.642857143, -1);
			EqualAcceleration(data, 60, 0.5, -0.5);
			EqualAcceleration(data, 120, 0.5, -0.5);

			// INTERPOLATED POINTS
			EqualAcceleration(data, 20, 1, -1);
			EqualAcceleration(data, 40, 0.785714286, -1);
			EqualAcceleration(data, 55, 0.571428572, -0.75);
			EqualAcceleration(data, 80, 0.5, -0.5);
			EqualAcceleration(data, 100, 0.5, -0.5);

			// EXTRAPOLATE 
			EqualAcceleration(data, -20, 1, -1);
			EqualAcceleration(data, 140, 0.5, -0.5);
		}
	}
}