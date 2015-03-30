﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
    [TestClass]
    public class SITest
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
        public void TestSI()
        {
            var si = new SI();
            Assert.AreEqual(0.0, si.ScalarValue());
            Assert.AreEqual("0 [-]", si.ToString());
            Assert.IsTrue(si.HasEqualUnit(new SI()));

            var si2 = 5.0.SI().Watt;
            Assert.AreEqual("5 [W]", si2.ToString());

            var si3 = 2.SI().Radian.Per.Second;
            Assert.AreEqual("2 [rad/s]", si3.ToString());

            var si4 = si2 * si3;
            Assert.AreEqual("10 [W/s]", si4.ToString());
            Assert.IsTrue(si4.HasEqualUnit(new SI().Watt.Per.Second));
            Assert.AreEqual("10 [kgmm/ssss]", si4.ToBasicUnits().ToString());


            var kg = 5.0.SI().Kilo.Gramm;
            Assert.AreEqual(5.0, kg.ScalarValue());
            Assert.AreEqual("5 [kg]", kg.ToString());

            kg = kg.To().Kilo.Gramm.Value();
            Assert.AreEqual(5.0, kg.ScalarValue());
            Assert.AreEqual("5 [kg]", kg.ToString());

            kg = kg.To().Gramm.Value();
            Assert.AreEqual(5000, kg.ScalarValue());
            Assert.AreEqual("5000 [g]", kg.ToString());

        }
    }
}
