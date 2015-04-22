using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Models.SimulationComponent;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class DrivingCycleTests
	{
		[TestMethod]
		public void TestEngineOnly()
		{
			var container = new VehicleContainer();

			var cycleData = DrivingCycleData.ReadFromFileEngineOnly(@"TestData\Cycles\Coach Engine Only.vdri");
			var cycle = new EngineOnlyDrivingCycle(container, cycleData);

			var outPort = new MockTnOutPort();
			var inPort = cycle.InShaft();
			var cycleOut = cycle.OutPort();

			inPort.Connect(outPort);

			var absTime = new TimeSpan();
			var dt = TimeSpan.FromSeconds(1);

			var response = cycleOut.Request(absTime, dt);
			Assert.IsInstanceOfType(response, typeof (ResponseSuccess));

			var dataWriter = new TestModalDataWriter();
			container.CommitSimulationStep(dataWriter);

			Assert.AreEqual(absTime, outPort.AbsTime);
			Assert.AreEqual(dt, outPort.Dt);
			Assert.AreEqual(600.0.RPMtoRad(), outPort.AngularVelocity);
			Assert.AreEqual(0.SI<NewtonMeter>(), outPort.Torque);
		}

		[TestMethod]
		public void TestEngineOnlyWithTimestamps()
		{
			var container = new VehicleContainer();

			var cycleData = DrivingCycleData.ReadFromFileEngineOnly(@"TestData\Cycles\Coach Engine Only Paux_var-dt.vdri");
			var cycle = new EngineOnlyDrivingCycle(container, cycleData);

			var outPort = new MockTnOutPort();
			var inPort = cycle.InShaft();

			inPort.Connect(outPort);

			var absTime = TimeSpan.FromSeconds(10);
			var dt = TimeSpan.FromSeconds(1);

			var response = cycle.OutPort().Request(absTime, dt);
			Assert.IsInstanceOfType(response, typeof (ResponseFailTimeInterval));

			dt = TimeSpan.FromSeconds(0.25);
			response = cycle.OutPort().Request(absTime, dt);
			Assert.IsInstanceOfType(response, typeof (ResponseSuccess));

			var dataWriter = new TestModalDataWriter();
			container.CommitSimulationStep(dataWriter);

			Assert.AreEqual(absTime, outPort.AbsTime);
			Assert.AreEqual(dt, outPort.Dt);
			Assert.AreEqual(743.2361.RPMtoRad(), outPort.AngularVelocity);
			Assert.AreEqual(Formulas.PowerToTorque(2779.576.SI<Watt>(), 743.2361.RPMtoRad()), outPort.Torque);

			// ========================

			dt = TimeSpan.FromSeconds(1);
			absTime = TimeSpan.FromSeconds(500);
			response = cycle.OutPort().Request(absTime, dt);
			Assert.IsInstanceOfType(response, typeof (ResponseFailTimeInterval));

			dt = TimeSpan.FromSeconds(0.25);

			for (int i = 0; i < 2; i++) {
				response = cycle.OutPort().Request(absTime, dt);
				Assert.IsInstanceOfType(response, typeof (ResponseSuccess));

				dataWriter = new TestModalDataWriter();
				container.CommitSimulationStep(dataWriter);

				Assert.AreEqual(absTime, outPort.AbsTime);
				Assert.AreEqual(dt, outPort.Dt);
				Assert.AreEqual(1584.731.RPMtoRad(), outPort.AngularVelocity);
				Assert.AreEqual(Formulas.PowerToTorque(3380.548.SI<Watt>(), 1584.731.RPMtoRad()), outPort.Torque);

				absTime += dt;
			}


			// todo: test going backward in time, end of cycle
		}

		[TestMethod]
		public void Test_TimeBased_FirstCycle()
		{
			var container = new VehicleContainer();

			var cycleData = DrivingCycleData.ReadFromFileTimeBased(@"TestData\Cycles\Coach time based.vdri");
			var cycle = new TimeBasedDrivingCycle(container, cycleData);

			var outPort = new MockDriverDemandOutPort();

			var inPort = cycle.InPort();
			var cycleOut = cycle.OutPort();

			inPort.Connect(outPort);

			var absTime = new TimeSpan();
			var dt = TimeSpan.FromSeconds(1);

			var response = cycleOut.Request(absTime, dt);
			Assert.IsInstanceOfType(response, typeof (ResponseSuccess));

			Assert.AreEqual(absTime, outPort.AbsTime);
			Assert.AreEqual(dt, outPort.Dt);
			Assert.AreEqual(0.0.SI<MeterPerSecond>(), outPort.Velocity);
			Assert.AreEqual((-0.020237973).SI().GradientPercent.Cast<Radian>(), outPort.Gradient);
		}

		[TestMethod]
		public void Test_TimeBased_TimeFieldMissing()
		{
			var container = new VehicleContainer();

			var cycleData = DrivingCycleData.ReadFromFileTimeBased(@"TestData\Cycles\Cycle time field missing.vdri");
			var cycle = new TimeBasedDrivingCycle(container, cycleData);

			var outPort = new MockDriverDemandOutPort();

			var inPort = cycle.InPort();
			var cycleOut = cycle.OutPort();

			inPort.Connect(outPort);

			var dataWriter = new TestModalDataWriter();
			var absTime = new TimeSpan();
			var dt = TimeSpan.FromSeconds(1);

			while (cycleOut.Request(absTime, dt) is ResponseSuccess) {
				Assert.AreEqual(absTime, outPort.AbsTime);
				Assert.AreEqual(dt, outPort.Dt);
				container.CommitSimulationStep(dataWriter);

				absTime += dt;
			}
		}
	}
}