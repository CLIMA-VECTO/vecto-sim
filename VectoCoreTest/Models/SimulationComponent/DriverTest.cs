using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;
using Wheels = TUGraz.VectoCore.Models.SimulationComponent.Impl.Wheels;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class DriverTest
	{
		public const string JobFile = @"TestData\Jobs\24t Coach EngineOnly.vecto";

		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		public const string AccelerationFile = @"TestData\Components\Coach.vacc";


		public const double Tolerance = 0.001;


		[TestMethod]
		public void DriverCoastingTest()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);

			var vehicleData = CreateVehicleData(33000.SI<Kilogram>());

			var driverData = CreateDriverData();

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain_Coasting.vmod");
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var driver = new Driver(vehicleContainer, driverData);
			var engine = new CombustionEngine(vehicleContainer, engineData);

			dynamic tmp = AddComponent(driver, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, engine);

			var gbx = new MockGearbox(vehicleContainer);
			gbx.CurrentGear = 1;

			var driverPort = driver.OutPort();

			driverPort.Initialize(5.SI<MeterPerSecond>(), 0.SI<Radian>());

			var absTime = 0.SI<Second>();

			var response = driver.DoCoast(absTime, 1.SI<Meter>(), 0.SI<Radian>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			Assert.AreEqual(4.9812, vehicleContainer.VehicleSpeed().Value(), Tolerance);
			Assert.AreEqual(0.2004, response.SimulationInterval.Value(), Tolerance);
			Assert.AreEqual(engine._previousState.FullDragPower.Value(), engine._previousState.EnginePower.Value(),
				Constants.SimulationSettings.EngineFLDPowerTolerance);

			while (vehicleContainer.VehicleSpeed() > 1) {
				response = driver.DoCoast(absTime, 1.SI<Meter>(), 0.SI<Radian>());

				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

				vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
				absTime += response.SimulationInterval;
				modalWriter.Finish();
			}
			modalWriter.Finish();
		}

		[TestMethod]
		public void DriverCoastingTest2()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);

			var vehicleData = CreateVehicleData(33000.SI<Kilogram>());

			var driverData = CreateDriverData();

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain_Coasting.vmod"); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var driver = new Driver(vehicleContainer, driverData);
			var engine = new CombustionEngine(vehicleContainer, engineData);

			dynamic tmp = AddComponent(driver, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, engine);

			var gbx = new MockGearbox(vehicleContainer);
			gbx.CurrentGear = 1;

			var driverPort = driver.OutPort();

			var gradient = VectoMath.InclinationToAngle(-0.020237973 / 100.0);
			driverPort.Initialize(5.SI<MeterPerSecond>(), gradient);

			var absTime = 0.SI<Second>();

			var response = driver.DoCoast(absTime, 1.SI<Meter>(), gradient);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			Assert.AreEqual(4.9812, vehicleContainer.VehicleSpeed().Value(), Tolerance);
			Assert.AreEqual(0.2004, response.SimulationInterval.Value(), Tolerance);
			Assert.AreEqual(engine._previousState.FullDragPower.Value(), engine._previousState.EnginePower.Value(),
				Constants.SimulationSettings.EngineFLDPowerTolerance);

			while (vehicleContainer.VehicleSpeed() > 1) {
				response = driver.DoCoast(absTime, 1.SI<Meter>(), gradient);

				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

				vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
				absTime += response.SimulationInterval;
				modalWriter.Finish();
			}
			modalWriter.Finish();
		}


		[TestMethod]
		public void DriverOverloadTest()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);

			var vehicleData = CreateVehicleData(33000.SI<Kilogram>());

			var driverData = CreateDriverData();

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain.vmod", SimulatorFactory.FactoryMode.EngineeringMode);
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var driver = new Driver(vehicleContainer, driverData);

			dynamic tmp = AddComponent(driver, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, new CombustionEngine(vehicleContainer, engineData));

			var gbx = new MockGearbox(vehicleContainer);
			gbx.CurrentGear = 1;

			var driverPort = driver.OutPort();

			driverPort.Initialize(0.SI<MeterPerSecond>(), 0.SI<Radian>());

			var absTime = 0.SI<Second>();

			var response = driverPort.Request(absTime, 1.SI<Meter>(), 10.SI<MeterPerSecond>(), 0.SI<Radian>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			Assert.AreEqual(0.908, modalWriter.GetValues<SI>(ModalResultField.acc).Last().Value(), Tolerance);

			response = driverPort.Request(absTime, 1.SI<Meter>(), 10.SI<MeterPerSecond>(), 0.SI<Radian>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			Assert.AreEqual(0.7973, modalWriter.GetValues<SI>(ModalResultField.acc).Last().Value(), Tolerance);

			// change vehicle weight, cannot reach minimum acceleration...
			vehicleData.Loading = 70000.SI<Kilogram>();

			try {
				response = driverPort.Request(absTime, 1.SI<Meter>(), 10.SI<MeterPerSecond>(), 0.05.SI<Radian>());
				Assert.Fail();
			} catch (VectoSimulationException e) {
				Assert.AreEqual("Could not achieve minimum acceleration", e.Message);
			}
			//Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
		}

		[TestMethod]
		public void DriverAccelerationTest()
		{
			var vehicleContainer = new VehicleContainer();
			var vehicle = new MockVehicle(vehicleContainer);

			var driverData = EngineeringModeSimulationDataReader.CreateDriverDataFromFile(JobFile);
			var driver = new Driver(vehicleContainer, driverData);

			driver.Connect(vehicle.OutPort());

			vehicle.MyVehicleSpeed = 0.SI<MeterPerSecond>();
			var absTime = 0.SI<Second>();
			var ds = 1.SI<Meter>();
			var gradient = 0.SI<Radian>();

			var targetVelocity = 5.SI<MeterPerSecond>();

//			var response = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

			var accelerations = new[] {
				1.01570922, 1.384540943, 1.364944972, 1.350793466, 1.331848649, 1.314995215, 1.2999934,
				1.281996392, 1.255462262
			};
			var simulationIntervals = new[]
			{ 1.403234648, 0.553054094, 0.405255346, 0.33653593, 0.294559444, 0.26555781, 0.243971311, 0.22711761, 0.213554656 };


			// accelerate from 0 to just below the target velocity and test derived simulation intervals & accelerations
			for (var i = 0; i < accelerations.Length; i++) {
				var tmpResponse = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

				Assert.IsInstanceOfType(tmpResponse, typeof(ResponseSuccess));
				Assert.AreEqual(accelerations[i], vehicle.LastRequest.acceleration.Value(), Tolerance);
				Assert.AreEqual(simulationIntervals[i], tmpResponse.SimulationInterval.Value(), Tolerance);

				vehicleContainer.CommitSimulationStep(absTime, tmpResponse.SimulationInterval);
				absTime += tmpResponse.SimulationInterval;
				vehicle.MyVehicleSpeed += (tmpResponse.SimulationInterval * vehicle.LastRequest.acceleration).Cast<MeterPerSecond>();
			}

			// full acceleration would exceed target velocity, driver should limit acceleration such that target velocity is reached...
			var response = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(0.899715479, vehicle.LastRequest.acceleration.Value(), Tolerance);
			Assert.AreEqual(0.203734517, response.SimulationInterval.Value(), Tolerance);

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;
			vehicle.MyVehicleSpeed +=
				(response.SimulationInterval * vehicle.LastRequest.acceleration).Cast<MeterPerSecond>();

			Assert.AreEqual(targetVelocity.Value(), vehicle.MyVehicleSpeed.Value(), Tolerance);


			// vehicle has reached target velocity, no further acceleration necessary...

			response = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(0, vehicle.LastRequest.acceleration.Value(), Tolerance);
			Assert.AreEqual(0.2, response.SimulationInterval.Value(), Tolerance);
		}


		[TestMethod]
		public void DriverDecelerationTest()
		{
			var vehicleContainer = new VehicleContainer();
			var vehicle = new MockVehicle(vehicleContainer);

			var driverData = EngineeringModeSimulationDataReader.CreateDriverDataFromFile(JobFile);
			var driver = new Driver(vehicleContainer, driverData);

			driver.Connect(vehicle.OutPort());

			vehicle.MyVehicleSpeed = 5.SI<MeterPerSecond>();
			var absTime = 0.SI<Second>();
			var ds = 1.SI<Meter>();
			var gradient = 0.SI<Radian>();

			var targetVelocity = 0.SI<MeterPerSecond>();

//			var response = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

			var accelerations = new[] {
				-0.68799597, -0.690581291, -0.693253225, -0.696020324, -0.698892653, -0.701882183, -0.695020765, -0.677731071,
				-0.660095846, -0.642072941, -0.623611107, -0.604646998, -0.58510078, -0.56497051, -0.547893288, -0.529859078,
				-0.510598641, -0.489688151, -0.466386685, -0.425121905
			};
			var simulationIntervals = new[] {
				0.202830428, 0.20884052, 0.215445127, 0.222749141, 0.230885341, 0.240024719, 0.250311822, 0.26182762, 0.274732249,
				0.289322578, 0.305992262, 0.325276486, 0.34792491, 0.37502941, 0.408389927, 0.451003215, 0.5081108, 0.590388012,
				0.724477573, 1.00152602
			};


			// accelerate from 0 to just below the target velocity and test derived simulation intervals & accelerations
			for (var i = 0; i < accelerations.Length; i++) {
				var tmpResponse = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

				Assert.IsInstanceOfType(tmpResponse, typeof(ResponseSuccess));
				Assert.AreEqual(accelerations[i], vehicle.LastRequest.acceleration.Value(), Tolerance);
				Assert.AreEqual(simulationIntervals[i], tmpResponse.SimulationInterval.Value(), Tolerance);

				vehicleContainer.CommitSimulationStep(absTime, tmpResponse.SimulationInterval);
				absTime += tmpResponse.SimulationInterval;
				vehicle.MyVehicleSpeed += (tmpResponse.SimulationInterval * vehicle.LastRequest.acceleration).Cast<MeterPerSecond>();
			}

			var response = driver.OutPort().Request(absTime, ds, targetVelocity, gradient);

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(-0.308576594, vehicle.LastRequest.acceleration.Value(), Tolerance);
			Assert.AreEqual(2.545854078, response.SimulationInterval.Value(), Tolerance);

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;
			vehicle.MyVehicleSpeed +=
				(response.SimulationInterval * vehicle.LastRequest.acceleration).Cast<MeterPerSecond>();

			Assert.AreEqual(targetVelocity.Value(), vehicle.MyVehicleSpeed.Value(), Tolerance);
		}


		//==================

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
					TwinTyres = false,
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
				AxleConfiguration = AxleConfiguration.AxleConfig_4x2,
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

		private static DriverData CreateDriverData()
		{
			return new DriverData {
				AccelerationCurve = AccelerationCurveData.ReadFromFile(AccelerationFile),
				LookAheadCoasting = new DriverData.LACData {
					Enabled = false,
				},
				OverSpeedEcoRoll = new DriverData.OverSpeedEcoRollData {
					Mode = VectoCore.Models.SimulationComponent.Data.DriverData.DriverMode.Off
				},
				StartStop = new VectoRunData.StartStopData {
					Enabled = false,
				}
			};
		}


		// ========================

		protected virtual IDriver AddComponent(IDrivingCycle prev, IDriver next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IVehicle AddComponent(IDriver prev, IVehicle next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IWheels AddComponent(IFvInProvider prev, IWheels next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}


		protected virtual ITnOutProvider AddComponent(IWheels prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IPowerTrainComponent AddComponent(IPowerTrainComponent prev, IPowerTrainComponent next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual void AddComponent(IPowerTrainComponent prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
		}
	}
}