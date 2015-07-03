using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
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
			var data = DrivingCycleData.ReadFromFileEngineOnly(TestContext.DataRow["CycleFile"].ToString());
			var expectedResults = ModalResults.ReadFromFile(TestContext.DataRow["ModalResultFile"].ToString());

			var vehicle = new VehicleContainer();
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);

			var aux = new DirectAuxiliary(vehicle, new AuxiliaryCycleDataAdapter(data));
			var gearbox = new EngineOnlyGearbox(vehicle);


			var engine = new CombustionEngine(vehicle, engineData);

			aux.InShaft().Connect(engine.OutShaft());
			gearbox.InShaft().Connect(aux.OutShaft());
			var port = aux.OutShaft();

//			IVectoJob job = SimulationFactory.CreateTimeBasedEngineOnlyRun(TestContext.DataRow["EngineData"].ToString(),
//				TestContext.DataRow["CycleFile"].ToString(), "test2.csv");

			var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
			var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

			var dataWriter = new TestModalDataWriter();

			var i = 0;
			var results = new[] {
				ModalResultField.n, ModalResultField.PaEng, ModalResultField.Tq_drag, ModalResultField.Pe_drag,
				ModalResultField.Pe_eng, ModalResultField.Tq_eng, ModalResultField.Tq_full, ModalResultField.Pe_full
			};
			//, ModalResultField.FC };
			//var siFactor = new[] { 1, 1000, 1, 1000, 1000, 1, 1, 1000, 1 };
			//var tolerances = new[] { 0.0001, 0.1, 0.0001, 0.1, 0.1, 0.001, 0.001, 0.1, 0.01 };
			foreach (var cycle in data.Entries) {
				port.Request(absTime, dt, cycle.EngineTorque, cycle.EngineSpeed);
				foreach (var sc in vehicle.SimulationComponents()) {
					sc.CommitSimulationStep(dataWriter);
				}

				// TODO: handle initial state of engine
				var row = expectedResults.Rows[i++];
				if (i > 2) {
					for (var j = 0; j < results.Length; j++) {
						var field = results[j];
						//						if (!Double.IsNaN(dataWriter.GetDouble(field)))
						Assert.AreEqual((double) row[field.GetName()], dataWriter.GetDouble(field),
							0.0001,
							String.Format("t: {0}  field: {1}", i, field));
					}
					if (row[ModalResultField.FC.GetName()] is double &&
						!Double.IsNaN(Double.Parse(row[ModalResultField.FC.GetName()].ToString()))) {
						Assert.AreEqual((double) row[ModalResultField.FC.GetName()],
							dataWriter.GetDouble(ModalResultField.FC), 0.01,
							"t: {0}  field: {1}", i, ModalResultField.FC);
					} else {
						Assert.IsTrue(Double.IsNaN(dataWriter.GetDouble(ModalResultField.FC)),
							String.Format("t: {0}", i));
					}
				}

				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
			}
			dataWriter.Data.WriteToFile(String.Format("result_{0}.csv", TestContext.DataRow["TestName"].ToString()));
		}

		[TestMethod]
		public void AssembleEngineOnlyPowerTrain()
		{
			var dataWriter = new TestModalDataWriter();

			var vehicleContainer = new VehicleContainer();

			var gearbox = new EngineOnlyGearbox(vehicleContainer);
			var engine = new CombustionEngine(vehicleContainer,
				EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile));

			gearbox.InShaft().Connect(engine.OutShaft());

			var absTime = new TimeSpan();
			var dt = TimeSpan.FromSeconds(1);

			var angularVelocity = 644.4445.RPMtoRad();
			var power = 2329.973.SI<Watt>();

			gearbox.OutShaft().Request(absTime, dt, Formulas.PowerToTorque(power, angularVelocity), angularVelocity);

			foreach (var sc in vehicleContainer.SimulationComponents()) {
				sc.CommitSimulationStep(dataWriter);
			}

			Assert.IsNotNull(dataWriter.CurrentRow);
		}
	}
}