using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class FullLoadCurveTest
	{
		private const string CoachEngineFLD = @"TestData\Components\24t Coach.vfld";
		private const double Tolerance = 0.0001;

		[TestMethod]
		public void TestFullLoadStaticTorque()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(1180, fldCurve.FullLoadStationaryTorque(560.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(1352, fldCurve.FullLoadStationaryTorque(2000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(1231, fldCurve.FullLoadStationaryTorque(580.RPMtoRad()).Double(), Tolerance);
		}

		[TestMethod]
		public void TestFullLoadEngineSpeedRated()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);
			Assert.AreEqual(181.8444, (double)fldCurve.RatedSpeed(), Tolerance);
		}

		[TestMethod]
		public void TestFullLoadStaticPower()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(69198.814183, fldCurve.FullLoadStationaryPower(560.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(283162.218372, fldCurve.FullLoadStationaryPower(2000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(74767.810760, fldCurve.FullLoadStationaryPower(580.RPMtoRad()).Double(), Tolerance);
		}

		[TestMethod]
		public void TestDragLoadStaticTorque()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(-149, fldCurve.DragLoadStationaryTorque(560.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(-301, fldCurve.DragLoadStationaryTorque(2000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(-148.5, fldCurve.DragLoadStationaryTorque(580.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(-150, fldCurve.DragLoadStationaryTorque(520.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(-339, fldCurve.DragLoadStationaryTorque(2200.RPMtoRad()).Double(), Tolerance);
		}

		[TestMethod]
		public void TestDragLoadStaticPower()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(-8737.81636, fldCurve.DragLoadStationaryPower(560.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(-63041.29254, fldCurve.DragLoadStationaryPower(2000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(-9019.51251, fldCurve.DragLoadStationaryPower(580.RPMtoRad()).Double(), Tolerance);
		}

		[TestMethod]
		public void TestPT1()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(0.6, fldCurve.PT1(560.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.25, fldCurve.PT1(2000.RPMtoRad()).Double(), Tolerance);
			Assert.AreEqual(0.37, fldCurve.PT1(1700.RPMtoRad()).Double(), Tolerance);
		}

		/// <summary>
		///     [VECTO-78]
		/// </summary>
		[TestMethod]
		public void Test_FileRead_WrongFileFormat_InsufficientColumns()
		{
			try {
				var curve = FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve insufficient columns.vfld");
				Assert.Fail("this should not be reached.");
			} catch (VectoException ex) {
				Assert.AreEqual("FullLoadCurve Data File must consist of 4 columns.", ex.Message);
			}
		}

		/// <summary>
		///     [VECTO-78]
		/// </summary>
		[TestMethod]
		public void Test_FileRead_HeaderColumnsNotNamedCorrectly()
		{
			FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve wrong header.vfld");
			//todo: check log file: ensure header warning was written!
		}

		/// <summary>
		///     [VECTO-78]
		/// </summary>
		[TestMethod]
		public void Test_FileRead_NoHeader()
		{
			var curve = FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve no header.vfld");
			var result = curve.FullLoadStationaryTorque(1.SI<PerSecond>());
			Assert.AreNotEqual(result.Double(), 0.0);
		}

		/// <summary>
		///     [VECTO-78]
		/// </summary>
		[TestMethod]
		public void Test_FileRead_InsufficientEntries()
		{
			try {
				FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve insufficient entries.vfld");
				Assert.Fail("this should not be reached.");
			} catch (VectoException ex) {
				Assert.AreEqual(
					"FullLoadCurve must consist of at least two lines with numeric values (below file header)",
					ex.Message);
			}
		}
	}
}