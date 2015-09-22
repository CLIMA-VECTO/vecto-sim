using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader;
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
			var cycleData = DrivingCycleDataReader.ReadFromFile(ShortCycle, CycleType.DistanceBased);

			var container = new VehicleContainer();
			var cycle = new DistanceBasedDrivingCycle(container, cycleData);

			var gbx = new MockGearbox(container);

			var driver = new MockDriver(container);
			cycle.InPort().Connect(driver.OutPort());

			cycle.OutPort().Initialize();

			// just in test mock driver
			driver.VehicleStopped = false;

			var startDistance = cycleData.Entries.First().Distance.Value();
			var absTime = 0.SI<Second>();

			// waiting time of 40 seconds is split up to 3 steps: 0.5, 39, 0.5
			var response = cycle.OutPort().Request(absTime, 1.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(0, driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.028416069495827, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(0.5, driver.LastRequest.dt.Value(), Tolerance);
			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			response = cycle.OutPort().Request(absTime, 1.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(0, driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.028416069495827, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(39, driver.LastRequest.dt.Value(), Tolerance);
			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			response = cycle.OutPort().Request(absTime, 1.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
			Assert.AreEqual(0, driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.028416069495827, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(0.5, driver.LastRequest.dt.Value(), Tolerance);
			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			response = cycle.OutPort().Request(absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.0284160694958265, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(1 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);

			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;
			
			response = cycle.OutPort().Request(absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.0284160694958265, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(2 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);

			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;


			var exceeded = (ResponseDrivingCycleDistanceExceeded)cycle.OutPort().Request(absTime, 300.SI<Meter>());
			Assert.AreEqual(16, exceeded.MaxDistance.Value(), Tolerance);
			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.0284160694958265, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(2 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);

			AssertHelper.Exception<VectoSimulationException>(() => {
				container.CommitSimulationStep(absTime, exceeded.SimulationInterval);
				absTime += exceeded.SimulationInterval;
			}, "Previous request did not succeed!");

			response = cycle.OutPort().Request(absTime, exceeded.MaxDistance);

			Assert.AreEqual(5.SI<MeterPerSecond>().Value(), driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0.0284160694958265, driver.LastRequest.Gradient.Value(), 1E-12);
			Assert.AreEqual(18 + startDistance, cycle.CurrentState.Distance.Value(), Tolerance);


			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;
		}
	}
}