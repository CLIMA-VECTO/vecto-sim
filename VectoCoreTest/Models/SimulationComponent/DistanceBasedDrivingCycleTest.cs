using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Integration;
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
		public void TestLimitRequst()
		{
			var data = new string[] {
				// <s>,<v>,<grad>,<stop>
				"  0,  20,  0,     0",
				"  1,  20, -0.1,   0",
				"  2,  20, -0.3,   0",
				" 10,  40, -0.3,   0",
				" 20,  30, -0.1,   0"
			};
			var cycleData = SimpleDrivingCycles.CreateCycleData(data);
			var container = new VehicleContainer();
			var cycle = new DistanceBasedDrivingCycle(container, cycleData);

			var gbx = new MockGearbox(container);

			var driver = new MockDriver(container);
			cycle.InPort().Connect(driver.OutPort());

			cycle.OutPort().Initialize();

			// just in test mock driver
			driver.VehicleStopped = false;

			var absTime = 0.SI<Second>();

			// a request up to 10m succeeds, no speed change for the next 10m

			var response = cycle.OutPort().Request(absTime, 0.3.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = cycle.OutPort().Request(absTime, 1.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = cycle.OutPort().Request(absTime, 1.3.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = cycle.OutPort().Request(absTime, 2.7.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			response = cycle.OutPort().Request(absTime, 3.5.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			// a request with 12m exceeds the speed change at 10m -> maxDistance == 10m

			response = cycle.OutPort().Request(absTime, 12.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseDrivingCycleDistanceExceeded));
			Assert.AreEqual(10, ((ResponseDrivingCycleDistanceExceeded)response).MaxDistance.Value());

			response = cycle.OutPort().Request(absTime, 10.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			// drive 10m
			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			// - - - - - - - - 
			// request with 8m succeeds

			response = cycle.OutPort().Request(absTime, 8.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			container.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			// - - - - - - - - 
			// request with 3m more -> distance exceeded. maxDistance == 2m (approach next speed change, we are within 5m radius)

			response = cycle.OutPort().Request(absTime, 3.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseDrivingCycleDistanceExceeded));
			Assert.AreEqual(2, ((ResponseDrivingCycleDistanceExceeded)response).MaxDistance.Value());

			// - - - - - - - - 
			// request with 1m (18 -> 19m) => response exceeded, drive up to next sample point (at least 5m)
			response = cycle.OutPort().Request(absTime, 1.SI<Meter>());
			Assert.IsInstanceOfType(response, typeof(ResponseDrivingCycleDistanceExceeded));
			Assert.AreEqual(5, ((ResponseDrivingCycleDistanceExceeded)response).MaxDistance.Value());

			// next request with 5m, as suggested => distance exceeded. maxDistance == 2m (next speed change)....
			response = cycle.OutPort().Request(absTime, ((ResponseDrivingCycleDistanceExceeded)response).MaxDistance);
			Assert.IsInstanceOfType(response, typeof(ResponseDrivingCycleDistanceExceeded));
			Assert.AreEqual(2, ((ResponseDrivingCycleDistanceExceeded)response).MaxDistance.Value());

			// ok
			response = cycle.OutPort().Request(absTime, ((ResponseDrivingCycleDistanceExceeded)response).MaxDistance);
			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
		}

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

			var startDistance = container.CycleStartDistance.Value();
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