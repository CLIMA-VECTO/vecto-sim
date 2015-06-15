using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models
{
	[TestClass]
	public class DeclarationDataTest
	{
		public const double Tolerance = 0.0001;

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
			Assert.AreEqual("Truck.vacc", segment.VACC);
			Assert.AreEqual(3, segment.Missions.Length);

			var longHaulMission = segment.Missions[0];
			Assert.AreEqual(MissionType.LongHaul, longHaulMission.MissionType);
			Assert.AreEqual("RigidSolo.vcdv", longHaulMission.VCDV);
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
			Assert.AreEqual("RigidSolo.vcdv", regionalDeliveryMission.VCDV);
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
			Assert.AreEqual("RigidSolo.vcdv", urbanDeliveryMission.VCDV);
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
	}
}