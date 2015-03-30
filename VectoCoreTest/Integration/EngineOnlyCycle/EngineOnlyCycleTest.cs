using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		public TestContext TestContext { get; set; }
		
		private const string EngineFile = @"TestData\Components\24t Coach.veng";

		[DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "|DataDirectory|\\TestData\\EngineTests.csv", "EngineTests#csv", DataAccessMethod.Sequential)]
		[TestMethod]
		public void TestEngineOnlyDrivingCycle()
		{
			var vehicle = new VehicleContainer();
			var engineData = CombustionEngineData.ReadFromFile(TestContext.DataRow["EngineFile"].ToString());

			var gearbox = new EngineOnlyGearbox(vehicle);

			var data = DrivingCycleData.ReadFromFileEngineOnly(TestContext.DataRow["CycleFile"].ToString());
			var expectedResults = ModalResults.ReadFromFile(TestContext.DataRow["ModalResultFile"].ToString());

			var engine = new CombustionEngine(vehicle, engineData);
			var port = engine.OutShaft();


			var absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
			var dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

			var dataWriter = new TestModalDataWriter();

			var i = 0;
		    var results = new[] { ModalResultField.n, ModalResultField.PaEng, ModalResultField.Tq_drag, ModalResultField.Pe_drag, ModalResultField.Pe_eng, ModalResultField.Tq_eng, ModalResultField.Tq_full, ModalResultField.Pe_full, }; 
			//, ModalResultField.FC };
			var siFactor = new double[] {1, 1000, 1, 1000, 1000, 1, 1, 1000, 1 };
			var tolerances = new double[] {0.0001, 0.1, 0.0001, 0.1, 0.1, 0.001, 0.001, 0.1, 0.01 };
			foreach (var cycle in data.Entries)
			{
				port.Request(absTime, dt, cycle.EngineTorque, cycle.EngineSpeed);
				foreach (var sc in vehicle.SimulationComponents())
				{
					sc.CommitSimulationStep(dataWriter);
				}

				// TODO: handle initial state of engine
				var row = expectedResults.Rows[i++];
				if (i > 2) {
					for (var j = 0; j < results.Length; j++) {
						var field = results[j];
//						if (!Double.IsNaN(dataWriter.GetDouble(field)))
						Assert.AreEqual((double) row[field.GetName()] * siFactor[j], dataWriter.GetDouble(field), tolerances[j]);
					}
					if (row[ModalResultField.FC.GetName()] is double) {
						Assert.AreEqual((double)row[ModalResultField.FC.GetName()], dataWriter.GetDouble(ModalResultField.FC), 0.01);
					}
					else {
						Assert.IsTrue(Double.IsNaN(dataWriter.GetDouble(ModalResultField.FC)));
					}
				}

				dataWriter.CommitSimulationStep(absTime, dt);
				absTime += dt;
			}
			dataWriter.Data.WriteToFile("test2.csv");
		}


		[TestMethod]
		public void AssembleEngineOnlyPowerTrain()
		{
			var dataWriter = new TestModalDataWriter();

			var vehicleContainer = new VehicleContainer();

			var gearbox = new EngineOnlyGearbox(vehicleContainer);
			var engine = new CombustionEngine(vehicleContainer, CombustionEngineData.ReadFromFile(EngineFile));

			gearbox.InShaft().Connect(engine.OutShaft());

			var absTime = new TimeSpan();
			var dt = TimeSpan.FromSeconds(1);

		    var angularVelocity = 644.4445.RPMtoRad();
		    var power = 2329.973.SI<Watt>();

            gearbox.Request(absTime, dt, Formulas.PowerToTorque(power, angularVelocity), angularVelocity);

			foreach (var sc in vehicleContainer.SimulationComponents())
			{
				sc.CommitSimulationStep(dataWriter);
			}

			Assert.IsNotNull(dataWriter.CurrentRow);
		}
	}
}
