using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class DistanceBasedDrivingCycleTest
	{
		public const string ShortCycle = @"TestData\Cycles\Coach_24t_xshort.vdri";

		public const double Tolerance = 0.0001;

		[TestMethod]
		public void TestDistanceRequest()
		{
			var cycleData = DrivingCycleDataReader.ReadFromFile(ShortCycle, DrivingCycleData.CycleType.DistanceBased);

			var vehicleContainer = new VehicleContainer();
			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			var driver = new MockDriver(vehicleContainer);
			cycle.InPort().Connect(driver.OutPort());

			cycle.OutPort().Initialize();

			var startDistance = cycleData.Entries.First().Distance.Value();
			var absTime = 0.SI<Second>();
			var response = cycle.OutPort().Request(absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			Assert.AreEqual(0, driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.028416069495827, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(40, driver.LastRequest.dt.Value(), Tolerance);

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;


			response = cycle.OutPort().Request((Second)absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.02667562971628240, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(1 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;


			response = cycle.OutPort().Request((Second)absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.02667562971628240, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(2 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;


			response = cycle.OutPort().Request((Second)absTime, 300.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseDrivingCycleDistanceExceeded));
			var tmp = response as ResponseDrivingCycleDistanceExceeded;
			Assert.AreEqual(36, tmp.MaxDistance.Value(), Tolerance);

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.02667562971628240, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(2 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);

			try {
				vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
				absTime += response.SimulationInterval;
				Assert.Fail();
			} catch (VectoSimulationException e) {
				Assert.AreEqual("Previous request did not succeed!", e.Message);
			}
			response = cycle.OutPort().Request((Second)absTime, tmp.MaxDistance);

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.02667562971628240, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(38 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);


			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;
		}
	}
}