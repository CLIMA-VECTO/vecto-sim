using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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


			gearbox.Request(absTime, dt, VectoMath.ConvertPowerRpmToTorque(2329.973, 644.4445), 644.4445);

			foreach (var sc in vehicleContainer.SimulationComponents())
			{
				sc.CommitSimulationStep(dataWriter);
			}

			Assert.IsNotNull(dataWriter.CurrentRow);
		}
	}
}
