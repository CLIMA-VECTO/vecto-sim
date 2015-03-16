using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Tests.Models
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var engineData = CombustionEngineData.ReadFromFile("TestData\\EngineOnly\\EngineMaps\\24t Coach.veng");
			var engine = new CombustionEngine(engineData);

			var simulationcontainer = new SimulationContainer();

			simulationcontainer.Addcomponent(engine);

			Assert.IsInstanceOfType(simulationcontainer.CombustionEngine(), typeof(ICombustionEngine));
		}
	}
}
