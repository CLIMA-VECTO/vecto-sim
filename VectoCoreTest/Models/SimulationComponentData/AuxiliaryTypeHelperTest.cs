using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class AuxiliaryTypeHelperTest
	{
		[TestMethod]
		public void TestParseAuxiliaryType()
		{
			Assert.AreEqual(AuxiliaryType.Fan, AuxiliaryTypeHelper.Parse("Fan"));
			Assert.AreEqual(AuxiliaryType.SteeringPump, AuxiliaryTypeHelper.Parse("Steering pump"));
			Assert.AreEqual(AuxiliaryType.ElectricSystem, AuxiliaryTypeHelper.Parse("Electric System"));
			Assert.AreEqual(AuxiliaryType.HeatingVentilationAirCondition, AuxiliaryTypeHelper.Parse("HVAC"));
			Assert.AreEqual(AuxiliaryType.PneumaticSystem, AuxiliaryTypeHelper.Parse("Pneumatic System"));
			AssertHelper.Exception<ArgumentOutOfRangeException>(() => { AuxiliaryTypeHelper.Parse("Foo Bar Blupp"); });
		}

		[TestMethod]
		public void TestToString()
		{
			Assert.AreEqual(Constants.Auxiliaries.Names.Fan, AuxiliaryTypeHelper.ToString(AuxiliaryType.Fan));
			Assert.AreEqual(Constants.Auxiliaries.Names.SteeringPump, AuxiliaryTypeHelper.ToString(AuxiliaryType.SteeringPump));
			Assert.AreEqual(Constants.Auxiliaries.Names.HeatingVentilationAirCondition,
				AuxiliaryTypeHelper.ToString(AuxiliaryType.HeatingVentilationAirCondition));
			Assert.AreEqual(Constants.Auxiliaries.Names.PneumaticSystem,
				AuxiliaryTypeHelper.ToString(AuxiliaryType.PneumaticSystem));
			Assert.AreEqual(Constants.Auxiliaries.Names.ElectricSystem,
				AuxiliaryTypeHelper.ToString(AuxiliaryType.ElectricSystem));
		}
	}
}