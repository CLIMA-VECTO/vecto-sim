using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl;
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
			var gbxData = EngineeringModeSimulationComponentFactory.CreateGearboxDataFromFile(GearboxFile);
				//GearboxData.ReadFromFile(GearboxFile);

			Assert.AreEqual(GearboxData.GearboxType.AMT, gbxData.Type);
			Assert.AreEqual(1.0, gbxData.TractionInterruption.Double(), 0.0001);
			Assert.AreEqual(8, gbxData.GearsCount());

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
			var rdyn = double.Parse(TestContext.DataRow["rDyn"].ToString(), CultureInfo.InvariantCulture);
			var speed = double.Parse(TestContext.DataRow["v"].ToString(), CultureInfo.InvariantCulture);

			var gbxData =
				EngineeringModeSimulationComponentFactory.CreateGearboxDataFromFile(TestContext.DataRow["GearboxDataFile"].ToString());


			var PvD = Double.Parse(TestContext.DataRow["PowerGbxOut"].ToString(), CultureInfo.InvariantCulture).SI<Watt>();

			var torqueToWheels = Formulas.PowerToTorque(PvD, SpeedToAngularSpeed(speed, rdyn));
			var torqueFromEngine = 0.SI<NewtonMeter>();

			var angSpeed = SpeedToAngularSpeed(speed, rdyn) * gbxData.AxleGearData.Ratio;
			if (TestContext.DataRow["Gear"].ToString() == "A") {
				torqueFromEngine = gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
			}

			var powerEngine = Formulas.TorqueToPower(torqueFromEngine, angSpeed);
			var loss = powerEngine - PvD;

			Assert.AreEqual(Double.Parse(TestContext.DataRow["GbxPowerLoss"].ToString(), CultureInfo.InvariantCulture),
				loss.Double(), 0.1,
				TestContext.DataRow["TestName"].ToString());
		}

		[TestMethod]
		public void TestInputOutOfRange()
		{
			var gbxData = EngineeringModeSimulationComponentFactory.CreateGearboxDataFromFile(GearboxFile);


			var angSpeed = 2700.RPMtoRad();
			var torqueToWheels = 100.SI<NewtonMeter>();

			try {
				gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
				Assert.Fail("angular Speed too high");
			} catch (Exception e) {
				Assert.IsInstanceOfType(e, typeof (VectoSimulationException), "angular speed too high");
			}

			angSpeed = 1000.RPMtoRad();
			torqueToWheels = 50000.SI<NewtonMeter>();
			try {
				gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
				Assert.Fail("torque too high");
			} catch (Exception e) {
				Assert.IsInstanceOfType(e, typeof (VectoSimulationException), "torque too high");
			}

			angSpeed = 1000.RPMtoRad();
			torqueToWheels = -10000.SI<NewtonMeter>();
			try {
				gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
				Assert.Fail("torque too low");
			} catch (Exception e) {
				Assert.IsInstanceOfType(e, typeof (VectoSimulationException), "torque too low");
			}

			angSpeed = -1000.RPMtoRad();
			torqueToWheels = 500.SI<NewtonMeter>();
			try {
				gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
				Assert.Fail("negative angular speed");
			} catch (Exception e) {
				Assert.IsInstanceOfType(e, typeof (VectoSimulationException), "negative angular speed");
			}
		}

		protected PerSecond SpeedToAngularSpeed(double v, double r)
		{
			return ((60 * v) / (2 * r * Math.PI / 1000)).RPMtoRad();
		}
	}
}