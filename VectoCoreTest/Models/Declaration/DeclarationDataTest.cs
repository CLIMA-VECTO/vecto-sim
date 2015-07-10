using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Declaration
{
	[TestClass]
	public class DeclarationDataTest
	{
		public const double Tolerance = 0.0001;
		public readonly MissionType[] Missions = Enum.GetValues(typeof(MissionType)).Cast<MissionType>().ToArray();

		[TestMethod]
		public void WheelDataTest()
		{
			var wheels = DeclarationData.Wheels;

			var tmp = wheels.Lookup("285/70 R19.5");

			Assert.AreEqual(7.9, tmp.Inertia.Double(), Tolerance);
			Assert.AreEqual(0.8943, tmp.DynamicTyreRadius.Double(), Tolerance);
			Assert.AreEqual("b", tmp.SizeClass);
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
			AssertHelper.Exception<VectoException>(() => pt1.Lookup(200.RPMtoRad()));
			AssertHelper.Exception<VectoException>(() => pt1.Lookup(0.RPMtoRad()));
		}

		[TestMethod]
		public void WHTCWeightingTest()
		{
			var whtc = DeclarationData.WHTCCorrection;

			var factors = new {
				urban = new[] { 0.11, 0.17, 0.69, 0.98, 0.62, 1.0, 1.0, 1.0, 0.45, 0.0 },
				rural = new[] { 0.0, 0.3, 0.27, 0.0, 0.32, 0.0, 0.0, 0.0, 0.36, 0.22 },
				motorway = new[] { 0.89, 0.53, 0.04, 0.02, 0.06, 0.0, 0.0, 0.0, 0.19, 0.78 }
			};

			var r = new Random();
			for (var i = 0; i < Missions.Length; i++) {
				var urban = r.NextDouble() * 2;
				var rural = r.NextDouble() * 2;
				var motorway = r.NextDouble() * 2;
				var whtcValue = whtc.Lookup(Missions[i], urban, rural, motorway);
				Assert.AreEqual(urban * factors.urban[i] + rural * factors.rural[i] + motorway * factors.motorway[i], whtcValue);
			}
		}

		[TestMethod]
		public void VCDVTest()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void DefaultTCTest()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void WHTCTest()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxElectricSystemTest()
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
				var leds = es.Lookup(expectation.Mission, technologies: new[] { "LED lights" });

				Assert.AreEqual(expectation.Base, baseConsumption.Double(), Tolerance);
				Assert.AreEqual(expectation.LED, leds.Double(), Tolerance);
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

			for (var i = 0; i < Missions.Length; i++) {
				// default tech
				Watt defaultValue = fan.Lookup(Missions[i], "");
				Assert.AreEqual(expected[defaultFan][i], defaultValue.Double(), Tolerance);

				// all fan techs
				foreach (var expect in expected) {
					Watt value = fan.Lookup(Missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Double(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxHeatingVentilationAirConditionTest()
		{
			var hvac = DeclarationData.HeatingVentilationAirConditioning;

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

			for (var i = 0; i < Missions.Length; i++) {
				foreach (var expect in expected) {
					Watt value = hvac.Lookup(Missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Double(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxPneumaticSystemTest()
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

			for (var i = 0; i < Missions.Length; i++) {
				foreach (var expect in expected) {
					Watt value = ps.Lookup(Missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Double(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxSteeringPumpTest()
		{
			var sp = DeclarationData.SteeringPump;

			var expected = new Dictionary<string, Dictionary<string, int[]>> {
				{
					"Fixed displacement", new Dictionary<string, int[]> {
						{ "1", new[] { 0, 260, 270, 0, 0, 0, 0, 0, 0, 0 } },
						{ "2", new[] { 370, 320, 310, 0, 0, 0, 0, 0, 0, 0 } },
						{ "3", new[] { 0, 340, 350, 0, 0, 0, 0, 0, 0, 0 } },
						{ "4", new[] { 610, 530, 0, 530, 0, 0, 0, 0, 0, 0 } },
						{ "5", new[] { 720, 630, 620, 0, 0, 0, 0, 0, 0, 0 } },
						{ "6", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "7", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "8", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "9", new[] { 720, 550, 0, 550, 0, 0, 0, 0, 0, 0 } },
						{ "10", new[] { 570, 530, 0, 0, 0, 0, 0, 0, 0, 0 } }
					}
				}, {
					"Variable displacement", new Dictionary<string, int[]> {
						{ "1", new[] { 0, 156, 162, 0, 0, 0, 0, 0, 0, 0 } },
						{ "2", new[] { 222, 192, 186, 0, 0, 0, 0, 0, 0, 0 } },
						{ "3", new[] { 0, 204, 210, 0, 0, 0, 0, 0, 0, 0 } },
						{ "4", new[] { 366, 318, 0, 318, 0, 0, 0, 0, 0, 0 } },
						{ "5", new[] { 432, 378, 372, 0, 0, 0, 0, 0, 0, 0 } },
						{ "6", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "7", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "8", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "9", new[] { 432, 330, 0, 330, 0, 0, 0, 0, 0, 0 } },
						{ "10", new[] { 342, 318, 0, 0, 0, 0, 0, 0, 0, 0 } }
					}
				}, {
					"Hydraulic supported by electric", new Dictionary<string, int[]> {
						{ "1", new[] { 0, 225, 235, 0, 0, 0, 0, 0, 0, 0 } },
						{ "2", new[] { 322, 278, 269, 0, 0, 0, 0, 0, 0, 0 } },
						{ "3", new[] { 0, 295, 304, 0, 0, 0, 0, 0, 0, 0 } },
						{ "4", new[] { 531, 460, 0, 460, 0, 0, 0, 0, 0, 0 } },
						{ "5", new[] { 627, 546, 540, 0, 0, 0, 0, 0, 0, 0 } },
						{ "6", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "7", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "8", new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ "9", new[] { 627, 478, 0, 478, 0, 0, 0, 0, 0, 0 } },
						{ "10", new[] { 498, 461, 0, 0, 0, 0, 0, 0, 0, 0 } }
					}
				}
			};

			foreach (var expect in expected) {
				var technology = expect.Key;
				foreach (var hdvClasses in expect.Value) {
					var hdvClass = hdvClasses.Key;
					for (var i = 0; i < Missions.Length; i++) {
						Watt value = sp.Lookup(Missions[i], hdvClass, technology);
						Assert.AreEqual(hdvClasses.Value[i], value.Double(), Tolerance);
					}
				}
			}
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
				AxleConfiguration = AxleConfiguration.AxleConfig_4x2,
				GrossVehicleMassRating = 11900.SI<Kilogram>(),
				CurbWeight = 5850.SI<Kilogram>()
			};

			var segment = DeclarationData.Segments.Lookup(vehicleData.VehicleCategory, vehicleData.AxleConfiguration,
				vehicleData.GrossVehicleMassRating, vehicleData.CurbWeight);


			Assert.AreEqual("2", segment.VehicleClass);

			var data = AccelerationCurveData.ReadFromStream(segment.AccelerationFile);
			TestAcceleration(data);

			Assert.AreEqual(3, segment.Missions.Length);

			var longHaulMission = segment.Missions[0];
			Assert.AreEqual(MissionType.LongHaul, longHaulMission.MissionType);

			Assert.AreEqual("RigidSolo", longHaulMission.CrossWindCorrection);

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

			Assert.AreEqual("RigidSolo", regionalDeliveryMission.CrossWindCorrection);

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

			Assert.AreEqual("RigidSolo", urbanDeliveryMission.CrossWindCorrection);

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

			//		var wheels = new Wheels(container, 0.SI<Meter>());

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