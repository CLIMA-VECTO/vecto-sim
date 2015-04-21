using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class GearboxDataTest
	{
		public TestContext TestContext { get; set; }

		protected const string GearboxFile = @"Testdata\Components\24t Coach.vgbx";

		[TestMethod]
		public void TestGearboxDataReadTest()
		{
			var gbxData = GearboxData.ReadFromFile(GearboxFile);

			Assert.AreEqual(GearboxData.GearboxType.AutomatedManualTransmission, gbxData.Type());
			Assert.AreEqual(1.0, gbxData.TractionInterruption.Double(), 0.0001);
			Assert.AreEqual(9, gbxData.GearsCount());

			Assert.AreEqual(3.240355, gbxData.AxleGearData.Ratio, 0.0001);
			Assert.AreEqual(1.0, gbxData[7].Ratio, 0.0001);

			Assert.AreEqual(-400, gbxData[1].ShiftPolygon[0].Torque.Double(), 0.0001);
			Assert.AreEqual(560.RPMtoRad().Double(), gbxData[1].ShiftPolygon[0].AngularSpeedDown.Double(), 0.0001);
			Assert.AreEqual(1289.RPMtoRad().Double(), gbxData[1].ShiftPolygon[0].AngularSpeedUp.Double(), 0.0001);

			Assert.AreEqual(200.RPMtoRad().Double(), gbxData[1].LossMap[15].InputSpeed.Double(), 0.0001);
			Assert.AreEqual(-350, gbxData[1].LossMap[15].InputTorque.Double(), 0.0001);
			Assert.AreEqual(13.072, gbxData[1].LossMap[15].TorqueLoss.Double(), 0.0001);
		}

		[TestMethod]
		[DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV",
			"|DataDirectory|\\TestData\\AxleGearLossInterpolation.csv",
			"AxleGearLossInterpolation#csv", DataAccessMethod.Sequential)]
		public void TestInterpolation()
		{
			var rdyn = Double.Parse(TestContext.DataRow["rDyn"].ToString());
			var speed = double.Parse(TestContext.DataRow["v"].ToString());

			var gbxData = GearboxData.ReadFromFile(TestContext.DataRow["GearboxDataFile"].ToString());


			var angSpeed = SpeedToAngularSpeed(speed, rdyn) * gbxData.AxleGearData.Ratio;
			var PvD = Double.Parse(TestContext.DataRow["PowerGbxOut"].ToString()).SI<Watt>();

			var torqueToWheels = Formulas.PowerToTorque(PvD, angSpeed);
			var torqueFromEngine = 0.SI<NewtonMeter>();

			if (TestContext.DataRow["Gear"].ToString() == "A") {
				torqueFromEngine = gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
			}

			var powerEngine = Formulas.TorqueToPower(torqueFromEngine, angSpeed);
			var loss = powerEngine - PvD;

			Assert.AreEqual(Double.Parse(TestContext.DataRow["GbxPowerLoss"].ToString()), loss.Double(), 0.1,
				TestContext.DataRow["TestName"].ToString());
		}

		protected PerSecond SpeedToAngularSpeed(double v, double r)
		{
			return ((60 * v) / (2 * r * Math.PI / 1000)).RPMtoRad();
		}
	}
}