using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
    [TestClass]
    public class DelauneyMapTest
    {
        public static void AssertException<T>(Action func, string message) where T : Exception
        {
            try
            {
                func();
                Assert.Fail();
            }
            catch (T ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        [TestMethod]
        public void Test_Simple_DelauneyMap()
        {
            var map = new DelauneyMap();
            map.AddPoint(0, 0, 0);
            map.AddPoint(1, 0, 0);
            map.AddPoint(0, 1, 0);

            map.Triangulate();

            var result = map.Interpolate(0.25, 0.25);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Test_DelauneyMapTriangle()
        {
            var map = new DelauneyMap();
            map.AddPoint(0, 0, 0);
            map.AddPoint(1, 0, 1);
            map.AddPoint(0, 1, 2);

            map.Triangulate();

            // fixed points
            Assert.AreEqual(0, map.Interpolate(0, 0));
            Assert.AreEqual(1, map.Interpolate(1, 0));
            Assert.AreEqual(2, map.Interpolate(0, 1));

            // interpolations
            Assert.AreEqual(0.5, map.Interpolate(0.5, 0));
            Assert.AreEqual(1, map.Interpolate(0, 0.5));
            Assert.AreEqual(1.5, map.Interpolate(0.5, 0.5));

            Assert.AreEqual(0.25, map.Interpolate(0.25, 0));
            Assert.AreEqual(0.5, map.Interpolate(0, 0.25));
            Assert.AreEqual(0.75, map.Interpolate(0.25, 0.25));

            Assert.AreEqual(0.75, map.Interpolate(0.75, 0));
            Assert.AreEqual(1.5, map.Interpolate(0, 0.75));

            // extrapolation (should fail)
            AssertException<VectoException>(() => map.Interpolate(1, 1), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(-1, -1), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(1, -1), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(-1, 1), "Interpolation failed.");
        }



        public void Test_DelauneyMapPlane()
        {
            var map = new DelauneyMap();
            map.AddPoint(0, 0, 0);
            map.AddPoint(1, 0, 1);
            map.AddPoint(0, 1, 2);
            map.AddPoint(1, 1, 3);

            map.Triangulate();

            // fixed points
            Assert.AreEqual(0, map.Interpolate(0, 0));
            Assert.AreEqual(1, map.Interpolate(1, 0));
            Assert.AreEqual(2, map.Interpolate(0, 1));
            Assert.AreEqual(3, map.Interpolate(1, 1));

            // interpolations
            Assert.AreEqual(0.5, map.Interpolate(0.5, 0));
            Assert.AreEqual(1, map.Interpolate(0, 0.5));
            Assert.AreEqual(2, map.Interpolate(1, 0.5));
            Assert.AreEqual(2.5, map.Interpolate(0.5, 1));

            Assert.AreEqual(1.5, map.Interpolate(0.5, 0.5));

            Assert.AreEqual(0.75, map.Interpolate(0.25, 0.25));
            Assert.AreEqual(2.25, map.Interpolate(0.75, 0.75));

            Assert.AreEqual(1.75, map.Interpolate(0.25, 0.75));
            Assert.AreEqual(1.25, map.Interpolate(0.75, 0.25));

            // extrapolation (should fail)
            AssertException<VectoException>(() => map.Interpolate(1.5, 0.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(1.5, 1.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(0.5, 1.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(-0.5, 1.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(-0.5, 0.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(-1.5, -1.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(0.5, -0.5), "Interpolation failed.");
            AssertException<VectoException>(() => map.Interpolate(-1.5, -0.5), "Interpolation failed.");
        }

        [TestMethod]
        public void Test_Delauney_LessThan3Points()
        {
            DelauneyMap map;
            try
            {
                map = new DelauneyMap();
                map.Triangulate();
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Triangulations needs at least 3 Points. Got 0 Points.", ex.Message);
            }
            try
            {
                map = new DelauneyMap();
                map.AddPoint(0, 0, 0);
                map.Triangulate();
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Triangulations needs at least 3 Points. Got 1 Points.", ex.Message);
            }
            try
            {
                map = new DelauneyMap();
                map.AddPoint(0, 0, 0);
                map.AddPoint(0, 0, 0);
                map.Triangulate();
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Triangulations needs at least 3 Points. Got 2 Points.", ex.Message);
            }

            map = new DelauneyMap();
            map.AddPoint(0, 0, 0);
            map.AddPoint(1, 0, 0);
            map.AddPoint(0, 1, 0);
            map.Triangulate();
        }


        [TestMethod]
        public void Test_DelauneyWithRealData()
        {
            Assert.Fail();
        }
    }
}
