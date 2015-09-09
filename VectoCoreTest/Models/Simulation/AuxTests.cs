using System.Linq;
using TUGraz.VectoCore.Utils;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class AuxTests
	{
		[TestMethod]
		public void AuxWriteModFileSumFile()
		{
			var dataWriter = new ModalDataWriter(@"AuxWriteModFileSumFile.vmod");
			dataWriter.AddAuxiliary("FAN");
			dataWriter.AddAuxiliary("PS");
			dataWriter.AddAuxiliary("STP");
			dataWriter.AddAuxiliary("ES");
			dataWriter.AddAuxiliary("AC");

			var sumWriter = new SummaryFileWriter(@"AuxWriteModFileSumFile.vsum");
			var deco = new SumWriterDecoratorFullPowertrain(sumWriter, "", "", "");

			var container = new VehicleContainer(dataWriter, deco);
			var data = DrivingCycleDataReader.ReadFromFileDistanceBased(@"TestData\Cycles\LongHaul_short.vdri");

			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var hdvClass = VehicleClass.Class5;
			var mission = MissionType.LongHaul;

			aux.AddConstant("FAN",
				DeclarationData.Fan.Lookup(MissionType.LongHaul, "Hydraulic driven - Constant displacement pump"));
			aux.AddConstant("PS", DeclarationData.PneumaticSystem.Lookup(mission, hdvClass));
			aux.AddConstant("STP",
				DeclarationData.SteeringPump.Lookup(MissionType.LongHaul, hdvClass, "Variable displacement"));
			aux.AddConstant("ES", DeclarationData.ElectricSystem.Lookup(mission, null));
			aux.AddConstant("AC",
				DeclarationData.HeatingVentilationAirConditioning.Lookup(mission, hdvClass));

			var speed = 1400.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();
			var t = 0.SI<Second>();
			var dt = 1.SI<Second>();

			for (var i = 0; i < 11; i++) {
				aux.OutPort().Request(t, dt, torque, speed);
				container.CommitSimulationStep(t, dt);
				t += dt;
			}

			container.FinishSimulation();
			sumWriter.Finish();

			//todo: add aux columns to test
			var testColumns = new[] { "Paux_FAN", "Paux_STP", "Paux_AC", "Paux_ES", "Paux_PS", "Paux" };

			ResultFileHelper.TestModFile(@"TestData\Results\EngineOnlyCycles\40t_Long_Haul_Truck_Long_Haul_Empty Loading.vmod",
				@"AuxWriteModFileSumFile.vmod", testColumns, testRowCount: false);
			ResultFileHelper.TestSumFile(@"TestData\Results\EngineOnlyCycles\40t_Long_Haul_Truck.vsum",
				@"AuxWriteModFileSumFile.vsum");
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
			var t = 0.SI<Second>();
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
			var data = DrivingCycleDataReader.ReadFromFileTimeBased(@"TestData\Cycles\Coach time based short.vdri");
			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();
			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			aux.AddDirect(cycle);

			var speed = 2358.RPMtoRad();
			var torque = 500.SI<NewtonMeter>();

			var t = 0.SI<Second>();

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
			var data = DrivingCycleDataReader.ReadFromFileTimeBased(@"TestData\Cycles\Coach time based short.vdri");
			// cycle ALT1 is set to values to equal the first few fixed points in the auxiliary file.
			// ALT1.aux file: nAuxiliary speed 2358: 0, 0.38, 0.49, 0.64, ...
			// ALT1 in cycle file: 0, 0.3724 (=0.38*0.96), 0.4802 (=0.49*0.96), 0.6272 (0.64*0.96), ...

			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var auxData = AuxiliaryData.ReadFromFile(@"TestData\Components\24t_Coach_ALT.vaux");
			// ratio = 4.078
			// efficiency_engine = 0.96
			// efficiency_supply = 0.98

			aux.AddMapping("ALT1", cycle, auxData);
			aux.AddDirect(cycle);
			var constPower = 1200.SI<Watt>();
			aux.AddConstant("CONSTANT", constPower);

			var speed = 578.22461991.RPMtoRad(); // = 2358 (nAuxiliary) * ratio
			var torque = 500.SI<NewtonMeter>();
			var t = 0.SI<Second>();
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
			var data = DrivingCycleDataReader.ReadFromFileTimeBased(@"TestData\Cycles\Coach time based short.vdri");
			// cycle ALT1 is set to values to equal the first few fixed points in the auxiliary file.
			// ALT1.aux file: nAuxiliary speed 2358: 0, 0.38, 0.49, 0.64, ...
			// ALT1 in cycle file: 0, 0.3724 (=0.38*0.96), 0.4802 (=0.49*0.96), 0.6272 (0.64*0.96), ...

			var cycle = new MockDrivingCycle(container, data);
			var port = new MockTnOutPort();

			var aux = new Auxiliary(container);
			aux.InPort().Connect(port);

			var auxData = AuxiliaryData.ReadFromFile(@"TestData\Components\24t_Coach_ALT.vaux");
			// ratio = 4.078
			// efficiency_engine = 0.96
			// efficiency_supply = 0.98

			aux.AddMapping(auxId, cycle, auxData);

			var speed = 578.22461991.RPMtoRad(); // = 2358 (nAuxiliary) * ratio
			var torque = 500.SI<NewtonMeter>();
			var t = 0.SI<Second>();
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
			var container = new VehicleContainer();
			var data = DrivingCycleDataReader.ReadFromFileTimeBased(@"TestData\Cycles\Coach time based short.vdri");
			var cycle = new MockDrivingCycle(container, data);

			var aux = new Auxiliary(container);
			AssertHelper.Exception<VectoException>(() => aux.AddMapping("NONEXISTING_AUX", cycle, null),
				"driving cycle does not contain column for auxiliary: NONEXISTING_AUX");
		}

		[TestMethod]
		public void AuxFileMissing()
		{
			AssertHelper.Exception<VectoException>(() => AuxiliaryData.ReadFromFile(@"NOT_EXISTING_AUX_FILE.vaux"),
				"Auxiliary file not found: NOT_EXISTING_AUX_FILE.vaux");
		}

		[TestMethod, Ignore]
		public void AuxReadJobFileDeclarationMode()
		{
			var sumWriter = new SummaryFileWriter(@"AuxReadJobFileDeclarationMode.vsum");
			var jobContainer = new JobContainer(sumWriter);

			var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode,
				@"TestData\Jobs\40t_Long_Haul_Truck.vecto");

			jobContainer.AddRuns(runsFactory);
			jobContainer.Execute();

			ResultFileHelper.TestSumFile(@"TestData\Results\Declaration\40t_Long_Haul_Truck.vsum",
				@"AuxReadJobFileDeclarationMode.vsum");
		}

		[TestMethod, Ignore]
		public void AuxReadJobFileEngineeringMode()
		{
			var sumWriter = new SummaryFileWriter(@"AuxReadJobFileEngineeringMode.vsum");
			var jobContainer = new JobContainer(sumWriter);

			var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineeringMode, @"TestData\Jobs\24t Coach.vecto");

			jobContainer.AddRuns(runsFactory);
			jobContainer.Execute();

			ResultFileHelper.TestSumFile(@"TestData\Results\Engineering\24t Coach.vsum", @"AuxReadJobFileEngineeringMode.vsum");

			ResultFileHelper.TestModFile(
				@"TestData\Results\Engineering\24t Coach_Coach_24t_xshort.vmod",
				@"TestData\Jobs\24t Coach_Coach_24t_xshort.vmod");
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxDeclarationWrongConfiguration()
		{
			// test what happens if there was a wrong auxiliary configuration in declaration mode
			Assert.Inconclusive();
		}

		[TestMethod]
		public void AuxCycleAdditionalFieldMissing()
		{
			// test the case when the Padd field is missing (no direct auxiliary)
			Assert.Inconclusive();
		}
	}
}