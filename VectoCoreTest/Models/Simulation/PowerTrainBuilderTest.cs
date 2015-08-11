using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	[TestClass]
	public class PowerTrainBuilderTest
	{
		public const string JobFile = @"TestData\Jobs\24t Coach.vecto";

		[TestMethod]
		public void BuildFullPowerTrainTest()
		{
			var reader = new EngineeringModeSimulationDataReader();
			reader.SetJobFile(JobFile);
			var runData = reader.NextRun().First();

			var writer = new MockModalDataWriter();
			var sumWriter = new TestSumWriter();
			var builder = new PowertrainBuilder(writer, sumWriter, false);

			var powerTrain = builder.Build(runData);

			Assert.IsInstanceOfType(powerTrain, typeof(IVehicleContainer));
			Assert.AreEqual(10, powerTrain.Components.Count);

			Assert.IsInstanceOfType(powerTrain.Engine, typeof(CombustionEngine));
			Assert.IsInstanceOfType(powerTrain.Gearbox, typeof(Gearbox));
			Assert.IsInstanceOfType(powerTrain.Cycle, typeof(ISimulationOutPort));
			Assert.IsInstanceOfType(powerTrain.Vehicle, typeof(Vehicle));
		}
	}
}