using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
    [TestClass]
    public class FullLoadCurveTest
    {
        private static string CoachEngineFLD = @"TestData\Components\24t Coach.vfld";
        private static double tolerance = 0.0001;

        [TestMethod]
        public void TestFullLoadStaticTorque()
        {
            FullLoadCurve fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

            Assert.AreEqual(1180, fldCurve.FullLoadStationaryTorque(560.0), tolerance);
            Assert.AreEqual(1352, fldCurve.FullLoadStationaryTorque(2000), tolerance);

            Assert.AreEqual(1231, fldCurve.FullLoadStationaryTorque(580), tolerance);


        }

        [TestMethod]
        public void TestFullLoadStaticPower()
        {
            FullLoadCurve fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

            Assert.AreEqual(69198.814183, fldCurve.FullLoadStationaryPower(560.0), tolerance);
            Assert.AreEqual(283162.218372, fldCurve.FullLoadStationaryPower(2000), tolerance);

            Assert.AreEqual(74767.810760, fldCurve.FullLoadStationaryPower(580), tolerance);

        }

        [TestMethod]
        public void TestDragLoadStaticTorque()
        {
            FullLoadCurve fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

            Assert.AreEqual(-149, fldCurve.DragLoadStationaryTorque(560.0), tolerance);
            Assert.AreEqual(-301, fldCurve.DragLoadStationaryTorque(2000), tolerance);

            Assert.AreEqual(-148.5, fldCurve.DragLoadStationaryTorque(580), tolerance);

            Assert.AreEqual(-150, fldCurve.DragLoadStationaryTorque(520.0), tolerance);

            Assert.AreEqual(-339, fldCurve.DragLoadStationaryTorque(2200.0), tolerance);


        }

        [TestMethod]
        public void TestDragLoadStaticPower()
        {
            FullLoadCurve fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

            Assert.AreEqual(-8737.81636, fldCurve.DragLoadStationaryPower(560.0), tolerance);
            Assert.AreEqual(-63041.29254, fldCurve.DragLoadStationaryPower(2000), tolerance);

            Assert.AreEqual(-9019.51251, fldCurve.DragLoadStationaryPower(580), tolerance);
        }

        [TestMethod]
        public void TestPT1()
        {
            FullLoadCurve fldCurve = FullLoadCurve.ReadFromFile(CoachEngineFLD);

            Assert.AreEqual(0.6, fldCurve.PT1(560.0), tolerance);
            Assert.AreEqual(0.25, fldCurve.PT1(2000), tolerance);

            Assert.AreEqual(0.37, fldCurve.PT1(1700), tolerance);

        }

        /// <summary>
        /// [VECTO-78]
        /// </summary>
        [TestMethod]
        public void Test_FileRead_WrongFileFormat_InsufficientColumns()
        {
            try
            {
                var curve = FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve insufficient columns.vfld");
            }
            catch (VectoException ex)
            {
                Assert.AreEqual("FullLoadCurve Data File must consist of 4 columns.", ex.Message);
            }
        }

        /// <summary>
        /// [VECTO-78]
        /// </summary>
        [TestMethod]
        public void Test_FileRead_HeaderColumnsNotNamedCorrectly()
        {
            var curve = FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve wrong header.vfld");

            //todo: check log!
            Assert.Fail("todo: assert log");
        }

        /// <summary>
        /// [VECTO-78]
        /// </summary>
        [TestMethod]
        public void Test_FileRead_NoHeader()
        {
            try
            {
                var curve = FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve no header.vfld");
            }
            catch (VectoException ex)
            {
                Assert.AreEqual(
                    @"File TestData\Components\FullLoadCurve no header.vfld: Line 0: The data format is not correct: no columns found.", 
                    ex.Message);
            }
        }

        /// <summary>
        /// [VECTO-78]
        /// </summary>
        [TestMethod]
        public void Test_FileRead_InsufficientEntries()
        {
            try
            {
                var curve = FullLoadCurve.ReadFromFile(@"TestData\Components\FullLoadCurve insufficient entries.vfld");
            }
            catch (VectoException ex)
            {
                Assert.AreEqual(
                    "FullLoadCurve must consist of at least two lines with numeric values (below file header)",
                    ex.Message);
            }
        }
    }
}
