using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class AuxiliaryTypeHelperTest
	{
		[TestMethod]
		public void TestParseAuxiliaryType()
		{
			var aux1 = AuxiliaryTypeHelper.Parse("Fan");
			Assert.AreEqual(AuxiliaryType.Fan, aux1);

			var aux2 = AuxiliaryTypeHelper.Parse("Steering pump");
			Assert.AreEqual(AuxiliaryType.SteeringPump, aux2);

			var aux3 = AuxiliaryTypeHelper.Parse("Electric System");
			Assert.AreEqual(AuxiliaryType.ElectricSystem, aux3);

			var aux4 = AuxiliaryTypeHelper.Parse("HVAC");
			Assert.AreEqual(AuxiliaryType.HeatingVentilationAirCondition, aux4);

			var aux5 = AuxiliaryTypeHelper.Parse("Pneumatic System");
			Assert.AreEqual(AuxiliaryType.PneumaticSystem, aux5);

			try {
				var aux6 = AuxiliaryTypeHelper.Parse("Foo Bar Blupp");
				Assert.Fail();
			} catch (ArgumentOutOfRangeException e) {}
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