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

			Assert.AreEqual(7.9, tmp.Inertia.Value(), Tolerance);
			Assert.AreEqual(0.8943, tmp.DynamicTyreRadius.Value(), Tolerance);
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
			Assert.AreEqual(0, pt1.Lookup(400.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.47, pt1.Lookup(800.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.58, pt1.Lookup(1000.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.53, pt1.Lookup(1200.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.46, pt1.Lookup(1400.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.43, pt1.Lookup(1500.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.22, pt1.Lookup(1750.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.2, pt1.Lookup(1800.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.11, pt1.Lookup(2000.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.11, pt1.Lookup(2500.RPMtoRad()).Value(), Tolerance);

			// INTERPOLATE
			Assert.AreEqual(0.235, pt1.Lookup(600.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.525, pt1.Lookup(900.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.555, pt1.Lookup(1100.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.495, pt1.Lookup(1300.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.445, pt1.Lookup(1450.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.325, pt1.Lookup(1625.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.21, pt1.Lookup(1775.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.155, pt1.Lookup(1900.RPMtoRad()).Value(), Tolerance);
			Assert.AreEqual(0.11, pt1.Lookup(2250.RPMtoRad()).Value(), Tolerance);


			// EXTRAPOLATE 
			Assert.AreEqual(0.11, pt1.Lookup(3000.RPMtoRad()).Value(), Tolerance);
			AssertHelper.Exception<VectoException>(() => pt1.Lookup(200.RPMtoRad()));
			AssertHelper.Exception<VectoException>(() => pt1.Lookup(0.RPMtoRad()));
		}

		[TestMethod]
		public void WHTCTest()
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
		public void AirDragTest()
		{
			var airDrag = DeclarationData.AirDrag;

			var expected = new Dictionary<string, AirDrag.AirDragEntry> {
				{ "RigidSolo", new AirDrag.AirDragEntry { A1 = 0.013526, A2 = 0.017746, A3 = -0.000666 } },
				{ "RigidTrailer", new AirDrag.AirDragEntry { A1 = 0.017125, A2 = 0.072275, A3 = -0.004148 } },
				{ "TractorSemitrailer", new AirDrag.AirDragEntry { A1 = 0.034767, A2 = 0.039367, A3 = -0.001897 } },
				{ "CoachBus", new AirDrag.AirDragEntry { A1 = -0.000794, A2 = 0.02109, A3 = -0.00109 } }
			};

			foreach (var kv in expected) {
				Assert.AreEqual(kv.Value, airDrag.Lookup(kv.Key));
			}

			var expectedCat = new Dictionary<VehicleCategory, AirDrag.AirDragEntry> {
				{ VehicleCategory.RigidTruck, new AirDrag.AirDragEntry { A1 = 0.013526, A2 = 0.017746, A3 = -0.000666 } },
				{ VehicleCategory.Tractor, new AirDrag.AirDragEntry { A1 = 0.034767, A2 = 0.039367, A3 = -0.001897 } },
				{ VehicleCategory.CityBus, new AirDrag.AirDragEntry { A1 = -0.000794, A2 = 0.02109, A3 = -0.00109 } },
				{ VehicleCategory.Coach, new AirDrag.AirDragEntry { A1 = -0.000794, A2 = 0.02109, A3 = -0.00109 } },
				{ VehicleCategory.InterurbanBus, new AirDrag.AirDragEntry { A1 = -0.000794, A2 = 0.02109, A3 = -0.00109 } }
			};

			foreach (var kv in expectedCat) {
				Assert.AreEqual(kv.Value, airDrag.Lookup(kv.Key));
			}
		}

		[TestMethod]
		public void DefaultTCTest()
		{
			var tc = DeclarationData.TorqueConverter;

			var expected = new[] {
				// fixed points
				new { nu = 1.000, mu = 1.000, torque = 0.00 },
				new { nu = 1.005, mu = 1.000, torque = 0.00 },
				new { nu = 1.100, mu = 1.000, torque = -40.34 },
				new { nu = 1.222, mu = 1.000, torque = -80.34 },
				new { nu = 1.375, mu = 1.000, torque = -136.11 },
				new { nu = 1.571, mu = 1.000, torque = -216.52 },
				new { nu = 1.833, mu = 1.000, torque = -335.19 },
				new { nu = 2.200, mu = 1.000, torque = -528.77 },
				new { nu = 2.750, mu = 1.000, torque = -883.40 },
				new { nu = 4.400, mu = 1.000, torque = -2462.17 },
				new { nu = 11.000, mu = 1.000, torque = -16540.98 },

				// interpolated points
				new { nu = 1.0025, mu = 1.0, torque = 0.0 },
				new { nu = 1.0525, mu = 1.0, torque = -20.17 },
				new { nu = 1.161, mu = 1.0, torque = -60.34 },
				new { nu = 1.2985, mu = 1.0, torque = -108.225 },
				new { nu = 1.2985, mu = 1.0, torque = -108.225 },
				new { nu = 1.473, mu = 1.0, torque = -176.315 },
				new { nu = 1.702, mu = 1.0, torque = -275.855 },
				new { nu = 2.0165, mu = 1.0, torque = -431.98 },
				new { nu = 2.475, mu = 1.0, torque = -706.085 },
				new { nu = 3.575, mu = 1.0, torque = -1672.785 },
				new { nu = 7.7, mu = 1.0, torque = -9501.575 },

				// extrapolated points
				new { nu = 0.5, mu = 1.0, torque = 0.0 },
				new { nu = 12.0, mu = 1.0, torque = -18674.133 }, // = (12-4.4)*(-16540.98- -2462.17)/(11-4.4)+ -2462.17
			};

			var referenceSpeed = 150.SI<PerSecond>();

			var r = new Random();

			foreach (var exp in expected) {
				var mu = tc.LookupMu(exp.nu);
				Assert.AreEqual(mu, exp.mu);

				var angularSpeed = r.Next(1000).SI<PerSecond>();
				var torque = tc.LookupTorque(exp.nu, angularSpeed, referenceSpeed);
				AssertHelper.AreRelativeEqual(
					exp.torque.SI<NewtonMeter>() * Math.Pow((angularSpeed / referenceSpeed).Cast<Scalar>(), 2), torque);
			}
		}

		[TestMethod]
		public void AuxElectricSystemTest()
		{
			var es = DeclarationData.ElectricSystem;

			var expected = new[] {
				new { Mission = MissionType.LongHaul, Base = 1240.SI<Watt>(), LED = 1190.SI<Watt>(), Efficiency = 0.7 },
				new { Mission = MissionType.RegionalDelivery, Base = 1055.SI<Watt>(), LED = 1005.SI<Watt>(), Efficiency = 0.7 },
				new { Mission = MissionType.UrbanDelivery, Base = 974.SI<Watt>(), LED = 924.SI<Watt>(), Efficiency = 0.7 },
				new { Mission = MissionType.MunicipalUtility, Base = 974.SI<Watt>(), LED = 924.SI<Watt>(), Efficiency = 0.7 },
				new { Mission = MissionType.Construction, Base = 975.SI<Watt>(), LED = 925.SI<Watt>(), Efficiency = 0.7 },
				new { Mission = MissionType.HeavyUrban, Base = 0.SI<Watt>(), LED = 0.SI<Watt>(), Efficiency = 1.0 },
				new { Mission = MissionType.Urban, Base = 0.SI<Watt>(), LED = 0.SI<Watt>(), Efficiency = 1.0 },
				new { Mission = MissionType.Suburban, Base = 0.SI<Watt>(), LED = 0.SI<Watt>(), Efficiency = 1.0 },
				new { Mission = MissionType.Interurban, Base = 0.SI<Watt>(), LED = 0.SI<Watt>(), Efficiency = 1.0 },
				new { Mission = MissionType.Coach, Base = 0.SI<Watt>(), LED = 0.SI<Watt>(), Efficiency = 1.0 }
			};
			Assert.AreEqual(expected.Length, Enum.GetValues(typeof(MissionType)).Length);

			foreach (var expectation in expected) {
				var baseConsumption = es.Lookup(expectation.Mission, technologies: null);
				var leds = es.Lookup(expectation.Mission, technologies: new[] { "LED lights" });

				AssertHelper.AreRelativeEqual(expectation.Base / expectation.Efficiency, baseConsumption);
				AssertHelper.AreRelativeEqual(expectation.LED / expectation.Efficiency, leds);
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
				Assert.AreEqual(expected[defaultFan][i], defaultValue.Value(), Tolerance);

				// all fan techs
				foreach (var expect in expected) {
					Watt value = fan.Lookup(Missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Value(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxHeatingVentilationAirConditionTest()
		{
			var hvac = DeclarationData.HeatingVentilationAirConditioning;

			var expected = new Dictionary<VehicleClass, int[]> {
				{ VehicleClass.Class1, new[] { 0, 150, 150, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class2, new[] { 200, 200, 150, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class3, new[] { 0, 200, 150, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class4, new[] { 350, 200, 0, 300, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class5, new[] { 350, 200, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class6, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class7, new[] { 0, 0, 0, 0, 200, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class8, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class9, new[] { 350, 200, 0, 300, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class10, new[] { 350, 200, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class11, new[] { 0, 0, 0, 0, 200, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class12, new[] { 0, 0, 0, 0, 200, 0, 0, 0, 0, 0 } }
			};

			for (var i = 0; i < Missions.Length; i++) {
				foreach (var expect in expected) {
					Watt value = hvac.Lookup(Missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Value(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxPneumaticSystemTest()
		{
			var ps = DeclarationData.PneumaticSystem;

			var expected = new Dictionary<VehicleClass, int[]> {
				{ VehicleClass.Class1, new[] { 0, 1300, 1240, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class2, new[] { 1180, 1280, 1320, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class3, new[] { 0, 1360, 1380, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class4, new[] { 1300, 1340, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class5, new[] { 1340, 1820, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class6, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class7, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class8, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class9, new[] { 1340, 1540, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class10, new[] { 1340, 1820, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class11, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
				{ VehicleClass.Class12, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }
			};

			for (var i = 0; i < Missions.Length; i++) {
				foreach (var expect in expected) {
					Watt value = ps.Lookup(Missions[i], expect.Key);
					Assert.AreEqual(expect.Value[i], value.Value(), Tolerance);
				}
			}
		}

		[TestMethod]
		public void AuxSteeringPumpTest()
		{
			var sp = DeclarationData.SteeringPump;

			var expected = new Dictionary<string, Dictionary<VehicleClass, int[]>> {
				{
					"Fixed displacement", new Dictionary<VehicleClass, int[]> {
						{ VehicleClass.Class1, new[] { 0, 260, 270, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class2, new[] { 370, 320, 310, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class3, new[] { 0, 340, 350, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class4, new[] { 610, 530, 0, 530, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class5, new[] { 720, 630, 620, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class6, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class7, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class8, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class9, new[] { 720, 550, 0, 550, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class10, new[] { 570, 530, 0, 0, 0, 0, 0, 0, 0, 0 } }
					}
				}, {
					"Variable displacement", new Dictionary<VehicleClass, int[]> {
						{ VehicleClass.Class1, new[] { 0, 156, 162, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class2, new[] { 222, 192, 186, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class3, new[] { 0, 204, 210, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class4, new[] { 366, 318, 0, 318, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class5, new[] { 432, 378, 372, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class6, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class7, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class8, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class9, new[] { 432, 330, 0, 330, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class10, new[] { 342, 318, 0, 0, 0, 0, 0, 0, 0, 0 } }
					}
				}, {
					"Hydraulic supported by electric", new Dictionary<VehicleClass, int[]> {
						{ VehicleClass.Class1, new[] { 0, 225, 235, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class2, new[] { 322, 278, 269, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class3, new[] { 0, 295, 304, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class4, new[] { 531, 460, 0, 460, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class5, new[] { 627, 546, 540, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class6, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class7, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class8, new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class9, new[] { 627, 478, 0, 478, 0, 0, 0, 0, 0, 0 } },
						{ VehicleClass.Class10, new[] { 498, 461, 0, 0, 0, 0, 0, 0, 0, 0 } }
					}
				}
			};

			foreach (var expect in expected) {
				var technology = expect.Key;
				foreach (var hdvClasses in expect.Value) {
					var hdvClass = hdvClasses.Key;
					for (var i = 0; i < Missions.Length; i++) {
						Watt value = sp.Lookup(Missions[i], hdvClass, technology);
						Assert.AreEqual(hdvClasses.Value[i], value.Value(), Tolerance);
					}
				}
			}
		}

		[TestMethod]
		public void SegmentTest()
		{
			//mock vehicleData
			var vehicleData = new {
				VehicleCategory = VehicleCategory.RigidTruck,
				AxleConfiguration = AxleConfiguration.AxleConfig_4x2,
				GrossVehicleMassRating = 11900.SI<Kilogram>(),
				CurbWeight = 5850.SI<Kilogram>()
			};

			var segment = DeclarationData.Segments.Lookup(vehicleData.VehicleCategory, vehicleData.AxleConfiguration,
				vehicleData.GrossVehicleMassRating, vehicleData.CurbWeight);


			Assert.AreEqual(VehicleClass.Class2, segment.VehicleClass);

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
			Assert.AreEqual(0.5882 * vehicleData.GrossVehicleMassRating - 2511.8.SI<Kilogram>(), longHaulMission.RefLoad);
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
			Assert.AreEqual(0.3941 * vehicleData.GrossVehicleMassRating - 1705.9.SI<Kilogram>(), regionalDeliveryMission.RefLoad);
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
			Assert.AreEqual(0.3941 * vehicleData.GrossVehicleMassRating - 1705.9.SI<Kilogram>(), urbanDeliveryMission.RefLoad);
			Assert.AreEqual(vehicleData.GrossVehicleMassRating - urbanDeliveryMission.MassExtra - vehicleData.CurbWeight,
				urbanDeliveryMission.MaxLoad);
		}

		public void EqualAcceleration(AccelerationCurveData data, double velocity, double acceleration, double deceleration)
		{
			var entry = data.Lookup(velocity.SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>());
			Assert.AreEqual(entry.Acceleration.Value(), acceleration, Tolerance);
			Assert.AreEqual(entry.Deceleration.Value(), deceleration, Tolerance);
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