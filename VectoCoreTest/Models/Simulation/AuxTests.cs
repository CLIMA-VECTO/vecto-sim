using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class AuxTests
	{
		private const string EngineFile = @"TestData\Components\24t Coach.veng";

		[TestMethod]
		public void Test_Aux_WriteModFileSumFile()
		{
			var sumWriter = new SummaryFileWriter(@"24t Coach.vsum");
			var jobContainer = new JobContainer(sumWriter);

			var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineOnlyMode);
			runsFactory.DataReader.SetJobFile(@"TestData\Jobs\24t Coach.vecto");

			jobContainer.AddRuns(runsFactory);
			jobContainer.Execute();

			ResultFileHelper.TestSumFile(@"TestData\Results\EngineOnlyCycles\24t Coach.vsum", @"TestData\Jobs\24t Coach.vsum");
			ResultFileHelper.TestModFiles(new[] {
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only1.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only2.vmod",
				@"TestData\Results\EngineOnlyCycles\24t Coach_Engine Only3.vmod"
			}, new[] { "24t Coach_Engine Only1.vmod", "24t Coach_Engine Only2.vmod", "24t Coach_Engine Only3.vmod" });
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxConstant()
		{
			var dataWriter = new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var port = new MockTnOutPort();
			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var constPower = 1200.SI<Watt>();
			aux.AddConstant("CONSTANT", constPower);

			var speed = 1400.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();
			var t = new TimeSpan();
			aux.OutPort().Request(t, t, torque, speed);
			Assert.AreEqual(speed, port.AngularVelocity);
			var newTorque = torque + constPower / speed;
			// 8.18511... = 1200/(1400*2*math.pi/60)
			AssertHelper.AreRelativeEqual(port.Torque - newTorque, 8.1851113590118.SI<NewtonMeter>());

			speed = 1400.RPMtoRad();
			torque = 1500.SI<NewtonMeter>();
			aux.OutPort().Request(t, t, torque, speed);
			Assert.AreEqual(speed, port.AngularVelocity);
			newTorque = torque + constPower / speed;
			// 8.18511... = 1200/(1400*2*math.pi/60)
			AssertHelper.AreRelativeEqual(port.Torque - newTorque, 8.1851113590118.SI<NewtonMeter>());

			speed = 900.RPMtoRad();
			torque = 1500.SI<NewtonMeter>();
			aux.OutPort().Request(t, t, torque, speed);
			Assert.AreEqual(speed, port.AngularVelocity);
			newTorque = torque + constPower / speed;
			// 12.73239... = 1200/(900*2*math.pi/60)
			AssertHelper.AreRelativeEqual(port.Torque - newTorque, 12.732395447351628.SI<NewtonMeter>());
		}

		[TestMethod]
		public void Test_AuxDirect()
		{
			var dataWriter = new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var data = DrivingCycleData.ReadFromFile(@"TestData\Cycles\Coach time based short.vdri",
				DrivingCycleData.CycleType.TimeBased);
			var cycle = new MockDrivingCycle(data);
			var port = new MockTnOutPort();
			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			aux.AddDirect(cycle);

			var speed = 1400.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();

			var t = new TimeSpan();

			var expected = new[] { 6100, 3100, 2300, 4500, 6100 };
			foreach (var e in expected) {
				aux.OutPort().Request(t, t, torque, speed);
				Assert.AreEqual(speed, port.AngularVelocity);
				var newTorque = torque + e.SI<Watt>() / speed;
				AssertHelper.AreRelativeEqual(port.Torque, newTorque);
			}
		}

		[TestMethod]
		public void Test_AuxMapping()
		{
			var dataWriter = new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var data = DrivingCycleData.ReadFromFile(@"TestData\Cycles\Coach time based short.vdri",
				DrivingCycleData.CycleType.TimeBased);
			var cycle = new MockDrivingCycle(data);
			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var auxData = MappingAuxiliaryData.ReadFromFile(@"TestData\Components\24t_Coach_ALT.vaux");
			aux.AddMapping("ALT1", cycle, auxData);

			var speed = 1400.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();
			var t = new TimeSpan();
			aux.OutPort().Request(t, t, torque, speed);

			Assert.AreEqual(speed, port.AngularVelocity);
			Assert.IsTrue(port.Torque > torque);
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxColumnMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxFileMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxReadJobFile()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxDeclaration()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxDeclarationWrongConfiguration()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void Test_AuxEngineering()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxCycleAdditionalFieldMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Test_AuxCycleAdditionalFieldOnly()
		{
			Assert.Inconclusive();
		}
	}
}