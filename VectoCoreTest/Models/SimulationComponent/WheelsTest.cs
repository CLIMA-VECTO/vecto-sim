using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class WheelsTest
	{
		private const string VehicleDataFile = @"TestData\Components\24t Coach.vveh";

		[TestMethod]
		public void WheelsRequestTest()
		{
			var container = new VehicleContainer();
			//var reader = new EngineeringModeSimulationDataReader();
			var vehicleData = EngineeringModeSimulationDataReader.CreateVehicleDataFromFile(VehicleDataFile);

			IWheels wheels = new Wheels(container, vehicleData.DynamicTyreRadius);
			var mockPort = new MockTnOutPort();

			wheels.InShaft().Connect(mockPort);

			var requestPort = wheels.OutPort();

			var absTime = TimeSpan.FromSeconds(0);
			var dt = TimeSpan.FromSeconds(1);

			var force = 5000.SI<Newton>();
			var velocity = 20.SI<MeterPerSecond>();

			var retVal = requestPort.Request(absTime, dt, force, velocity);

			Assert.AreEqual(2600.0, mockPort.Torque.Double(), 0.0001);
			Assert.AreEqual(38.4615384615, mockPort.AngularVelocity.Double(), 0.0001);
		}
	}
}