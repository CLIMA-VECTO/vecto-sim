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
		[TestMethod]
		public void AuxWriteModFileSumFile()
		{
			var dataWriter = new ModalDataWriter(@"24t Coach AUX.vmod", true);
			dataWriter.AddAuxiliary("ALT1");
			dataWriter.AddAuxiliary("CONSTANT");

			var sumWriter = new SummaryFileWriter(@"24t Coach AUX.vsum");
			var deco = new SumWriterDecoratorEngineOnly(sumWriter, "", "", "");

			var container = new VehicleContainer(dataWriter, deco);
			var data = DrivingCycleData.ReadFromFile(@"TestData\Cycles\Coach time based short.vdri",
				DrivingCycleData.CycleType.TimeBased);

			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var auxData = MappingAuxiliaryData.ReadFromFile(@"TestData\Components\24t_Coach_ALT.vaux");

			aux.AddMapping("ALT1", cycle, auxData);
			aux.AddDirect(cycle);
			var constPower = 1200.SI<Watt>();
			aux.AddConstant("CONSTANT", constPower);

			var speed = 578.22461991.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();
			var t = new TimeSpan();

			for (var i = 0; i < data.Entries.Count; i++) {
				aux.OutPort().Request(t, t, torque, speed);
				container.CommitSimulationStep(0, 0);
			}

			container.FinishSimulation();
			sumWriter.Finish();
		}


		[TestMethod]
		public void AuxConstant()
		{
			var dataWriter = new MockModalDataWriter();
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var port = new MockTnOutPort();
			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var constPower = 1200.SI<Watt>();
			aux.AddConstant("CONSTANT", constPower);

			var speed = 2358.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();
			var t = new TimeSpan();
			aux.OutPort().Request(t, t, torque, speed);
			Assert.AreEqual(speed, port.AngularVelocity);
			var newTorque = torque + constPower / speed;
			AssertHelper.AreRelativeEqual(port.Torque, newTorque);

			speed = 2358.RPMtoRad();
			torque = 1500.SI<NewtonMeter>();
			aux.OutPort().Request(t, t, torque, speed);
			Assert.AreEqual(speed, port.AngularVelocity);
			newTorque = torque + constPower / speed;
			AssertHelper.AreRelativeEqual(port.Torque, newTorque);

			speed = 1500.RPMtoRad();
			torque = 1500.SI<NewtonMeter>();
			aux.OutPort().Request(t, t, torque, speed);
			Assert.AreEqual(speed, port.AngularVelocity);
			newTorque = torque + constPower / speed;
			AssertHelper.AreRelativeEqual(port.Torque, newTorque);
		}

		[TestMethod]
		public void AuxDirect()
		{
			var dataWriter = new MockModalDataWriter();
			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var data = DrivingCycleData.ReadFromFile(@"TestData\Cycles\Coach time based short.vdri",
				DrivingCycleData.CycleType.TimeBased);
			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();
			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			aux.AddDirect(cycle);

			var speed = 2358.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();

			var t = new TimeSpan();

			var expected = new[] { 6100, 3100, 2300, 4500, 6100 };
			foreach (var e in expected) {
				aux.OutPort().Request(t, t, torque, speed);
				Assert.AreEqual(speed, port.AngularVelocity);
				var newTorque = torque + e.SI<Watt>() / speed;
				AssertHelper.AreRelativeEqual(port.Torque, newTorque);

				cycle.CommitSimulationStep(null);
			}
		}

		[TestMethod]
		public void AuxAllCombined()
		{
			var dataWriter = new MockModalDataWriter();
			dataWriter.AddAuxiliary("ALT1");
			dataWriter.AddAuxiliary("CONSTANT");

			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var data = DrivingCycleData.ReadFromFile(@"TestData\Cycles\Coach time based short.vdri",
				DrivingCycleData.CycleType.TimeBased);
			// cycle ALT1 is set to values to equal the first few fixed points in the auxiliary file.
			// ALT1.aux file: nAuxiliary speed 2358: 0, 0.38, 0.49, 0.64, ...
			// ALT1 in cycle file: 0, 0.3724 (=0.38*0.96), 0.4802 (=0.49*0.96), 0.6272 (0.64*0.96), ...

			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var auxData = MappingAuxiliaryData.ReadFromFile(@"TestData\Components\24t_Coach_ALT.vaux");
			// ratio = 4.078
			// efficiency_engine = 0.96
			// efficiency_supply = 0.98

			aux.AddMapping("ALT1", cycle, auxData);
			aux.AddDirect(cycle);
			var constPower = 1200.SI<Watt>();
			aux.AddConstant("CONSTANT", constPower);

			var speed = 578.22461991.RPMtoRad(); // = 2358 (nAuxiliary) * ratio
			var torque = 500.SI<NewtonMeter>();
			var t = new TimeSpan();
			var expected = new[] {
				1200 + 6100 + 72.9166666666667, // = 1000 * 0.07 (nAuxiliary=2358 and psupply=0) / 0.98 (efficiency_supply)
				1200 + 3100 + 677.083333333333, // = 1000 * 0.65 (nAuxiliary=2358 and psupply=0.38) / 0.98 (efficiency_supply)
				1200 + 2300 + 822.916666666667, // = 1000 * 0.79 (nAuxiliary=2358 and psupply=0.49) / 0.98 (efficiency_supply)
				1200 + 4500 + 1031.25, // = ...
				1200 + 6100 + 1166.66666666667,
				1200 + 6100 + 1656.25,
				1200 + 6100 + 2072.91666666667,
				1200 + 6100 + 2510.41666666667,
				1200 + 6100 + 2979.16666666667,
				1200 + 6100 + 3322.91666666667,
				1200 + 6100 + 3656.25
			};

			foreach (var e in expected) {
				aux.OutPort().Request(t, t, torque, speed);
				Assert.AreEqual(speed, port.AngularVelocity);

				AssertHelper.AreRelativeEqual(port.Torque, torque + e.SI<Watt>() / speed);

				cycle.CommitSimulationStep(null);
			}
		}

		[TestMethod]
		public void AuxMapping()
		{
			var auxId = "ALT1";
			var dataWriter = new MockModalDataWriter();
			dataWriter.AddAuxiliary(auxId);

			var sumWriter = new TestSumWriter();
			var container = new VehicleContainer(dataWriter, sumWriter);
			var data = DrivingCycleData.ReadFromFile(@"TestData\Cycles\Coach time based short.vdri",
				DrivingCycleData.CycleType.TimeBased);
			// cycle ALT1 is set to values to equal the first few fixed points in the auxiliary file.
			// ALT1.aux file: nAuxiliary speed 2358: 0, 0.38, 0.49, 0.64, ...
			// ALT1 in cycle file: 0, 0.3724 (=0.38*0.96), 0.4802 (=0.49*0.96), 0.6272 (0.64*0.96), ...

			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var auxData = MappingAuxiliaryData.ReadFromFile(@"TestData\Components\24t_Coach_ALT.vaux");
			// ratio = 4.078
			// efficiency_engine = 0.96
			// efficiency_supply = 0.98

			aux.AddMapping(auxId, cycle, auxData);

			var speed = 578.22461991.RPMtoRad(); // = 2358 (nAuxiliary) * ratio
			var torque = 500.SI<NewtonMeter>();
			var t = new TimeSpan();
			var expected = new[] {
				72.9166666666667, // = 1000 * 0.07 (pmech from aux file at nAuxiliary=2358 and psupply=0) / 0.98 (efficiency_supply)
				677.083333333333, // = 1000 * 0.65 (nAuxiliary=2358 and psupply=0.38) / 0.98
				822.916666666667, // = 1000 * 0.79 (nAuxiliary=2358 and psupply=0.49) / 0.98
				1031.25, // = ...
				1166.66666666667,
				1656.25,
				2072.91666666667,
				2510.41666666667,
				2979.16666666667,
				3322.91666666667,
				3656.25
			};

			foreach (var e in expected) {
				aux.OutPort().Request(t, t, torque, speed);
				Assert.AreEqual(speed, port.AngularVelocity);

				AssertHelper.AreRelativeEqual(port.Torque, torque + e.SI<Watt>() / speed);

				cycle.CommitSimulationStep(null);
			}
		}

		[TestMethod]
		public void AuxColumnMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxFileMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxReadJobFile()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void AuxDeclaration()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void AuxDeclarationWrongConfiguration()
		{
			Assert.Inconclusive();
		}


		[TestMethod]
		public void AuxEngineering()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxCycleAdditionalFieldMissing()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxCycleAdditionalFieldOnly()
		{
			Assert.Inconclusive();
		}
	}
}