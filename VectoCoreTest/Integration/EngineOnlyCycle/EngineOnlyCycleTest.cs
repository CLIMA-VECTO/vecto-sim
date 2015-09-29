using System.Data;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
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
			var container = new VehicleContainer();
			var cycle = new MockDrivingCycle(container, data);
			var vehicle = new VehicleContainer();
			var engineData =
				EngineeringModeSimulationDataReader.CreateEngineDataFromFile(TestContext.DataRow["EngineFile"].ToString());

			var aux = new Auxiliary(vehicle);
			aux.AddDirect(cycle);
			var gearbox = new EngineOnlyGearbox(vehicle);

			var engine = new EngineOnlyCombustionEngine(vehicle, engineData);

			aux.InPort().Connect(engine.OutPort());
			gearbox.InPort().Connect(aux.OutPort());
			var port = aux.OutPort();

			var absTime = 0.SI<Second>();
			var dt = 1.SI<Second>();

			var modFile = Path.GetRandomFileName() + ".vmod";
			var dataWriter = new ModalDataWriter(modFile, SimulatorFactory.FactoryMode.EngineOnlyMode);

			foreach (var cycleEntry in data.Entries) {
				var response = port.Request(absTime, dt, cycleEntry.EngineTorque, cycleEntry.EngineSpeed);
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));
				foreach (var sc in vehicle.SimulationComponents()) {
					dataWriter[ModalResultField.time] = absTime + dt / 2;
					sc.CommitSimulationStep(dataWriter);
				}

				dataWriter.CommitSimulationStep();
				absTime += dt;
			}
			dataWriter.Finish();

			ResultFileHelper.TestModFile(TestContext.DataRow["ModalResultFile"].ToString(), modFile);
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

			gearbox.OutPort().Request(absTime, dt, power / angularVelocity, angularVelocity);

			foreach (var sc in vehicleContainer.SimulationComponents()) {
				sc.CommitSimulationStep(dataWriter);
			}

			Assert.IsNotNull(dataWriter.CurrentRow);
		}
	}
}