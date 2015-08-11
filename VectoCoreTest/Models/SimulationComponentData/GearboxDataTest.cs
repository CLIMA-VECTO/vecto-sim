using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO.Reader.Impl;
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
			var gbxData = EngineeringModeSimulationDataReader.CreateGearboxDataFromFile(GearboxFile);

			Assert.AreEqual(GearboxData.GearboxType.AMT, gbxData.Type);
			Assert.AreEqual(1.0, gbxData.TractionInterruption.Value(), 0.0001);
			Assert.AreEqual(8, gbxData.Gears.Count);

			Assert.AreEqual(3.240355, gbxData.AxleGearData.Ratio, 0.0001);
			Assert.AreEqual(1.0, gbxData.Gears[7].Ratio, 0.0001);

			Assert.AreEqual(-400, gbxData.Gears[1].ShiftPolygon.Downshift[0].Torque.Value(), 0.0001);
			Assert.AreEqual(560.RPMtoRad().Value(), gbxData.Gears[1].ShiftPolygon.Downshift[0].AngularSpeed.Value(), 0.0001);
			Assert.AreEqual(1289.RPMtoRad().Value(), gbxData.Gears[1].ShiftPolygon.Upshift[0].AngularSpeed.Value(), 0.0001);

			Assert.AreEqual(200.RPMtoRad().Value(), gbxData.Gears[1].LossMap[15].InputSpeed.Value(), 0.0001);
			Assert.AreEqual(-350, gbxData.Gears[1].LossMap[15].InputTorque.Value(), 0.0001);
			Assert.AreEqual(13.072, gbxData.Gears[1].LossMap[15].TorqueLoss.Value(), 0.0001);
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
				EngineeringModeSimulationDataReader.CreateGearboxDataFromFile(TestContext.DataRow["GearboxDataFile"].ToString());


			var PvD = double.Parse(TestContext.DataRow["PowerGbxOut"].ToString(), CultureInfo.InvariantCulture).SI<Watt>();

			var torqueToWheels = Formulas.PowerToTorque(PvD, SpeedToAngularSpeed(speed, rdyn));
			var torqueFromEngine = 0.SI<NewtonMeter>();

			var angSpeed = SpeedToAngularSpeed(speed, rdyn) * gbxData.AxleGearData.Ratio;
			if (TestContext.DataRow["Gear"].ToString() == "A") {
				torqueFromEngine = gbxData.AxleGearData.LossMap.GearboxInTorque(angSpeed, torqueToWheels);
			}

			var powerEngine = Formulas.TorqueToPower(torqueFromEngine, angSpeed);
			var loss = powerEngine - PvD;

			Assert.AreEqual(double.Parse(TestContext.DataRow["GbxPowerLoss"].ToString(), CultureInfo.InvariantCulture),
				loss.Value(), 0.1,
				TestContext.DataRow["TestName"].ToString());
		}

		[TestMethod]
		public void TestInputOutOfRange()
		{
			var gbxData = EngineeringModeSimulationDataReader.CreateGearboxDataFromFile(GearboxFile);
		}

		protected PerSecond SpeedToAngularSpeed(double v, double r)
		{
			return ((60 * v) / (2 * r * Math.PI / 1000)).RPMtoRad();
		}
	}
}