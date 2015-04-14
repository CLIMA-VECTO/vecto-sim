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

			Assert.AreEqual(1180, (double) fldCurve.FullLoadStationaryTorque(560.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(1352, (double) fldCurve.FullLoadStationaryTorque(2000.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(1231, (double) fldCurve.FullLoadStationaryTorque(580.0.RPMtoRad()), Tolerance);
		}

		[TestMethod]
		public void TestFullLoadStaticPower()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(69198.814183, (double) fldCurve.FullLoadStationaryPower(560.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(283162.218372, (double) fldCurve.FullLoadStationaryPower(2000.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(74767.810760, (double) fldCurve.FullLoadStationaryPower(580.0.RPMtoRad()), Tolerance);
		}

		[TestMethod]
		public void TestDragLoadStaticTorque()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(-149, (double) fldCurve.DragLoadStationaryTorque(560.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(-301, (double) fldCurve.DragLoadStationaryTorque(2000.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(-148.5, (double) fldCurve.DragLoadStationaryTorque(580.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(-150, (double) fldCurve.DragLoadStationaryTorque(520.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(-339, (double) fldCurve.DragLoadStationaryTorque(2200.0.RPMtoRad()), Tolerance);
		}

		[TestMethod]
		public void TestDragLoadStaticPower()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(-8737.81636, (double) fldCurve.DragLoadStationaryPower(560.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(-63041.29254, (double) fldCurve.DragLoadStationaryPower(2000.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(-9019.51251, (double) fldCurve.DragLoadStationaryPower(580.0.RPMtoRad()), Tolerance);
		}

		[TestMethod]
		public void TestPT1()
		{
			var fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

			Assert.AreEqual(0.6, (double) fldCurve.PT1(560.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(0.25, (double) fldCurve.PT1(2000.0.RPMtoRad()), Tolerance);
			Assert.AreEqual(0.37, (double) fldCurve.PT1(1700.0.RPMtoRad()), Tolerance);
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
			Assert.AreNotEqual((double) result, 0.0);
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