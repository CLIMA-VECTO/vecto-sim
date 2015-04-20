using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

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

			Assert.AreEqual(GearboxData.GearboxType.AutomatedManualTransmission, gbxData.Type());
			Assert.AreEqual(1.0, gbxData.TractionInterruption.Double(), 0.0001);
			Assert.AreEqual(9, gbxData.GearsCount());

			Assert.AreEqual(3.240355, gbxData.AxleGear.Ratio, 0.0001);
			Assert.AreEqual(1.0, gbxData[7].Ratio, 0.0001);

			Assert.AreEqual(-400, gbxData[1].ShiftPolygon[0].Torque.Double(), 0.0001);
			Assert.AreEqual(560.RPMtoRad().Double(), gbxData[1].ShiftPolygon[0].AngularSpeedDown.Double(), 0.0001);
			Assert.AreEqual(1289.RPMtoRad().Double(), gbxData[1].ShiftPolygon[0].AngularSpeedUp.Double(), 0.0001);

			Assert.AreEqual(200.RPMtoRad().Double(), gbxData[1].LossMap[15].InputSpeed.Double(), 0.0001);
			Assert.AreEqual(-350, gbxData[1].LossMap[15].InputTorque.Double(), 0.0001);
			Assert.AreEqual(13.072, gbxData[1].LossMap[15].TorqueLoss.Double(), 0.0001);
		}
	}
}