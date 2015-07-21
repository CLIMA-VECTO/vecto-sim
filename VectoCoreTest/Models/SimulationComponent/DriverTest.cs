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
		public const string JobFile = @"TestData\Jobs\24t Coach.vecto";

		public const double Tolerance = 0.001;

		[TestMethod]
		public void DriverRequestTest()
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
				Assert.AreEqual(Math.Round(accelerations[i], 4), vehicle.LastRequest.acceleration.Value(), Tolerance);
				Assert.AreEqual(Math.Round(simulationIntervals[i], 4), tmpResponse.SimulationInterval.Value(), Tolerance);

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
	}
}