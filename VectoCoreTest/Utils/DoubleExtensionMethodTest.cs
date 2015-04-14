using System;
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
	}
}