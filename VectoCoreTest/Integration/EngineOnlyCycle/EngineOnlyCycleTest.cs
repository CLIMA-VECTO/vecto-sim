using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Integration.EngineOnlyCycle
{
	[TestClass]
	public class EngineOnlyCycleTest
	{
		private const string EngineFile = @"TestData\Components\24t Coach.veng";
		public TestContext TestContext { get; set; }

		[DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\TestData\\EngineTests.csv",
			"EngineTests#csv", DataAccessMethod.Sequential)]
		[TestMethod]
		public void TestEngineOnlyDrivingCycle()
		{
			var data = DrivingCycleDataReader.ReadFromFileEngineOnly(TestContext.DataRow["CycleFile"].ToString());
			var cycle = new MockDrivingCycle(null, data);
			var expectedResults = ModalResults.ReadFromFile(TestContext.DataRow["ModalResultFile"].ToString());

			var vehicle = new VehicleContainer();
			var engineData =
				EngineeringModeSimulationDataReader.CreateEngineDataFromFile(TestContext.DataRow["EngineFile"].ToString());

			var aux = new Auxiliary(vehicle);
			aux.AddDirect(cycle);
			var gearbox = new EngineOnlyGearbox(vehicle);


			var engine = new CombustionEngine(vehicle, engineData);

			aux.InPort().Connect(engine.OutPort());
			gearbox.InPort().Connect(aux.OutPort());
			var port = aux.OutPort();

//			IVectoJob job = SimulationFactory.CreateTimeBasedEngineOnlyRun(TestContext.DataRow["EngineFile"].ToString(),
//				TestContext.DataRow["CycleFile"].ToString(), "test2.csv");

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();

			var dataWriter = new MockModalDataWriter();

			var i = 0;
			var results = new[] {
				ModalResultField.n, ModalResultField.PaEng, ModalResultField.Tq_drag, ModalResultField.Pe_drag,
				ModalResultField.Pe_eng, ModalResultField.Tq_eng, ModalResultField.Tq_full, ModalResultField.Pe_full
			};
			//, ModalResultField.FC };
			//var siFactor = new[] { 1, 1000, 1, 1000, 1000, 1, 1, 1000, 1 };
			//var tolerances = new[] { 0.0001, 0.1, 0.0001, 0.1, 0.1, 0.001, 0.001, 0.1, 0.01 };
			foreach (var cycleEntry in data.Entries) {
				port.Request(absTime, dt, cycleEntry.EngineTorque, cycleEntry.EngineSpeed);
				foreach (var sc in vehicle.SimulationComponents()) {
					sc.CommitSimulationStep(dataWriter);
				}

				// TODO: handle initial state of engine
				var row = expectedResults.Rows[i++];
				if (i > 2) {
					for (var j = 0; j < results.Length; j++) {
						var field = results[j];
						Assert.AreEqual((double)row[field.GetName()], dataWriter.GetDouble(field),
							0.0001, string.Format("t: {0}  field: {1}", i, field));
					}
					if (row[ModalResultField.FCMap.GetName()] is double &&
						!double.IsNaN(double.Parse(row[ModalResultField.FCMap.GetName()].ToString()))) {
						Assert.AreEqual((double)row[ModalResultField.FCMap.GetName()],
							dataWriter.GetDouble(ModalResultField.FCMap), 0.01,
							"t: {0}  field: {1}", i, ModalResultField.FCMap);
					} else {
						Assert.IsTrue(double.IsNaN(dataWriter.GetDouble(ModalResultField.FCMap)),
							string.Format("t: {0}", i));
					}
				}

				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
			}
			dataWriter.Data.WriteToFile(string.Format("result_{0}.csv", TestContext.DataRow["TestName"].ToString()));
		}

		[TestMethod]
		public void AssembleEngineOnlyPowerTrain()
		{
			var dataWriter = new MockModalDataWriter();

			var vehicleContainer = new VehicleContainer();

			var gearbox = new EngineOnlyGearbox(vehicleContainer);
			var engine = new CombustionEngine(vehicleContainer,
				EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile));

			gearbox.InPort().Connect(engine.OutPort());

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();

			var angularVelocity = 644.4445.RPMtoRad();
			var power = 2329.973.SI<Watt>();

			gearbox.OutPort().Request(absTime, dt, Formulas.PowerToTorque(power, angularVelocity), angularVelocity);

			foreach (var sc in vehicleContainer.SimulationComponents()) {
				sc.CommitSimulationStep(dataWriter);
			}

			Assert.IsNotNull(dataWriter.CurrentRow);
		}
	}
}