using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
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
			var wheels = DeclarationData.Instance().Wheels;

			var tmp = wheels.Lookup("285/70 R 19.5");

			Assert.AreEqual(7.9, tmp.Inertia.Double(), Tolerance);
			Assert.AreEqual(0.8943, tmp.DynamicTyreRadius.Double(), Tolerance);
			Assert.AreEqual(0, tmp.SizeClass);
		}

		[TestMethod]
		public void RimsDataTest()
		{
			var rims = DeclarationData.Instance().Rims;

			var tmp = rims.Lookup("15° DC Rims");

			Assert.AreEqual(3.03, tmp.F_a, Tolerance);
			Assert.AreEqual(3.05, tmp.F_b, Tolerance);
		}

		[TestMethod]
		public void SegmentTest()
		{
			//var factory = DeclarationModeFactory.Instance();
			//var job = factory.ReadJobFile("12t Delivery Truck.vecto");
			//var vehicleData = factory.ReadVehicleData(job.VehicleFile);

			var vehicleData = new {
				VehicleCategory = VehicleCategory.RigidTruck,
				AxleConfiguration = AxleConfiguration.AxleConfig4x2,
				GrossVehicleMassRating = 11900.SI<Kilogram>(),
				CurbWeight = 5850.SI<Kilogram>()
			};

			var segment = DeclarationData.GetSegment(vehicleData.VehicleCategory, vehicleData.AxleConfiguration,
				vehicleData.GrossVehicleMassRating);


			Assert.AreEqual(2, segment.HDVClass);
			Assert.AreEqual("Truck.vacc", segment.VACC);
			Assert.AreEqual(3, segment.Missions.Length);

			var longHaulMission = segment.Missions[0];
			Assert.AreEqual("LongHaul", longHaulMission.Name);
			Assert.AreEqual("RigidSolo.vcdv", longHaulMission.VCDV);
			Assert.AreEqual(new[] { 40, 60 }, longHaulMission.AxleWeightDistribution);
			Assert.AreEqual(1900, longHaulMission.MassExtra);
			Assert.AreEqual(588.2 * vehicleData.GrossVehicleMassRating - 2511.8, longHaulMission.RefLoad);
			Assert.AreEqual("Long_Haul.vdri", longHaulMission.CycleFile);

			Assert.AreEqual(
				new[] {
					0.SI<Kilogram>(), longHaulMission.RefLoad,
					vehicleData.GrossVehicleMassRating - longHaulMission.MassExtra - vehicleData.CurbWeight
				},
				longHaulMission.Loadings);

			var regionalDeliveryMission = segment.Missions[1];
			Assert.AreEqual("RegionalDelivery", regionalDeliveryMission.Name);
			Assert.AreEqual("RigidSolo.vcdv", regionalDeliveryMission.VCDV);
			Assert.AreEqual(new[] { 45, 55 }, regionalDeliveryMission.AxleWeightDistribution);
			Assert.AreEqual(1900, regionalDeliveryMission.MassExtra);
			Assert.AreEqual(394.1 * vehicleData.GrossVehicleMassRating - 1705.9, regionalDeliveryMission.RefLoad);
			Assert.AreEqual("Regional_Delivery.vdri", regionalDeliveryMission.CycleFile);

			Assert.AreEqual(
				new[] {
					0.SI<Kilogram>(), regionalDeliveryMission.RefLoad,
					vehicleData.GrossVehicleMassRating - regionalDeliveryMission.MassExtra - vehicleData.CurbWeight
				},
				regionalDeliveryMission.Loadings);

			var urbanDeliveryMission = segment.Missions[1];
			Assert.AreEqual("UrbanDelivery", urbanDeliveryMission.Name);
			Assert.AreEqual("RigidSolo.vcdv", urbanDeliveryMission.VCDV);
			Assert.AreEqual(new[] { 45, 55 }, urbanDeliveryMission.AxleWeightDistribution);
			Assert.AreEqual(1900, urbanDeliveryMission.MassExtra);
			Assert.AreEqual(394.1 * vehicleData.GrossVehicleMassRating - 1705.9, urbanDeliveryMission.RefLoad);
			Assert.AreEqual("Urban_Delivery.vdri", urbanDeliveryMission.CycleFile);

			Assert.AreEqual(
				new[] {
					0.SI<Kilogram>(), urbanDeliveryMission.RefLoad,
					vehicleData.GrossVehicleMassRating - urbanDeliveryMission.MassExtra - vehicleData.CurbWeight
				},
				urbanDeliveryMission.Loadings);


			//var runs = new List<IVectoSimulator>();

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

			//		var missionVehicleData = new VehicleData(vehicleData, loading);
			//		var vehicle = new Vehicle(container, missionVehicleData);
			//		vehicle.InPort().Connect(wheels.OutPort());

			//		var driverData = new DriverData();
			//		var driver = new Driver(driverData);
			//		driver.InShaft().Connect(vehicle.OutShaft());

			//		var cycleData = DrivingCycleData.ReadFromFileEngineOnly(mission.CycleFile);
			//		var cycle = new DistanceBasedDrivingCycle(container, cycleData);
			//		cycle.InShaft().Connect(driver.OutShaft());

			//		var simulator = new VectoSimulator(container, cycle);
			//		runs.Add(simulator);
			//	}
			//}
		}
	}
}