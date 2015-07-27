using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class DriverTest
	{
		public const string JobFile = @"TestData\Jobs\24t Coach EngineOnly.vecto";

		public const double Tolerance = 0.001;

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
	}
}