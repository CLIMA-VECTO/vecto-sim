using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace VectoCoreTest
{
	[TestClass]
	public class CombustionEngineTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var engineData = new CombustionEngineData();
			var engine = new CombustionEngine(engineData);

			// var port = engine.OutPort();
			Assert.IsNotNull(engine);
		}
	}
}
