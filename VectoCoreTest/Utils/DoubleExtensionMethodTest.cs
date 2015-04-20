using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	[TestClass]
	public class DoubleExtensionMethodTest
	{
		[TestMethod]
		public void DoubleExtensions_SI_Test()
		{
			var val = 600.0.RPMtoRad();
			Assert.AreEqual(600 / 60 * 2 * Math.PI, val.Double());

			Assert.IsTrue(0.0.SI<PerSecond>().HasEqualUnit(val));

			var val2 = 1200.0.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second.Cast<PerSecond>();
			val = val * 2;
			Assert.AreEqual(val, val2);

			val2 = val2 / 2;
			val = val / 2;
			Assert.AreEqual(val, val2);
			Assert.AreEqual(600.SI().Rounds.Per.Minute.Cast<PerSecond>(), val2);
			Assert.AreEqual(600.SI().Rounds.Per.Minute.Cast<PerSecond>().Double(), val2.Double());
		}

		[TestMethod]
		public void DoubleExtension_CompareTests()
		{
			Assert.IsTrue(0.0.IsEqual(0.0));
			Assert.IsTrue(1.0.IsGreater(0.0));
			Assert.IsTrue(1.0.IsGreaterOrEqual(1.0));
			Assert.IsTrue(1.0.IsPositive());
			Assert.IsTrue(0.0.IsSmaller(1.0));
			Assert.IsTrue(1.0.IsSmallerOrEqual(1.0));


			Assert.IsTrue(0.0.IsEqual(0.001));

			Assert.IsTrue(1.002.IsGreater(1.0));
			Assert.IsTrue(1.001.IsGreater(1.0));
			Assert.IsTrue(1.0.IsGreater(1.0));
			Assert.IsFalse(0.999.IsGreater(1.0));

			Assert.IsTrue(1.001.IsGreaterOrEqual(1.0));
			Assert.IsFalse(0.999.IsGreaterOrEqual(1.0));
			Assert.IsFalse(0.998.IsGreaterOrEqual(1.0));

			Assert.IsTrue(0.001.IsPositive());
			Assert.IsTrue(0.0.IsPositive());
			Assert.IsTrue((-0.0009).IsPositive());
			Assert.IsFalse((-0.001).IsPositive());
			Assert.IsFalse((-0.002).IsPositive());

			Assert.IsTrue(0.998.IsSmaller(1.0));
			Assert.IsTrue(0.999.IsSmaller(1.0));
			Assert.IsTrue(1.0.IsSmaller(1.0));
			Assert.IsFalse(1.0011.IsSmaller(1.0));

			Assert.IsTrue(1.001.IsSmallerOrEqual(1.0));
			Assert.IsFalse(1.002.IsSmallerOrEqual(1.0));
			Assert.IsTrue(0.999.IsSmallerOrEqual(1.0));
			Assert.IsTrue(0.998.IsSmallerOrEqual(1.0));
		}
	}
}