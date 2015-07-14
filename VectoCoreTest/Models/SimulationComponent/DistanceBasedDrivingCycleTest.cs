using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
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
			var cycleData = DrivingCycleData.ReadFromFile(ShortCycle, DrivingCycleData.CycleType.DistanceBased);

			var vehicleContainer = new VehicleContainer();
			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			var driver = new MockDriver(vehicleContainer);
			cycle.InPort().Connect(driver.OutPort());

			cycle.OutPort().Initialize();

			cycle.OutPort().Request(TimeSpan.FromSeconds(0), 1.SI<Meter>());

			Assert.AreEqual(18, driver.LastRequest.TargetVelocity.Value(), Tolerance);
			Assert.AreEqual(0, driver.LastRequest.Gradient.Value(), Tolerance);
		}
	}
}