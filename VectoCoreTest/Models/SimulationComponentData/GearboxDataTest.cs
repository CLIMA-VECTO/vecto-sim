using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class GearboxDataTest
	{
		protected const string GearboxFile = @"Testdata\Components\24t Coach.vgbx";
		[TestMethod]
		public void TestGearboxDataReadTest()
		{
			var gbxData = GearboxData.ReadFromFile(GearboxFile);


		}
	}
}