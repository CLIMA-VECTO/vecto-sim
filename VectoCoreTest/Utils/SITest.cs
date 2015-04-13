using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	[TestClass]
	public class SITest
	{
		public static void AssertException<T>(Action func, string message) where T : Exception
		{
			try {
				func();
				Assert.Fail();
			} catch (T ex) {
				Assert.AreEqual(message, ex.Message);
			}
		}

		[TestMethod]
		public void TestSI()
		{
			var si = new SI();
			Assert.AreEqual(0.0, si.Double());
			Assert.AreEqual("0 [-]", si.ToString());
			Assert.IsTrue(si.HasEqualUnit(new SI()));

			var si2 = 5.0.SI().Watt;
			Assert.AreEqual("5 [W]", si2.ToString());

			var si3 = 2.SI().Radian.Per.Second;
			Assert.AreEqual("2 [1/s]", si3.ToString());

			var si4 = si2 * si3;
			Assert.AreEqual("10 [W/s]", si4.ToString());
			Assert.IsTrue(si4.HasEqualUnit(new SI().Watt.Per.Second));
			Assert.AreEqual("10 [kgmm/ssss]", si4.ToBasicUnits().ToString());


			var kg = 5.0.SI().Kilo.Gramm;
			Assert.AreEqual(5.0, kg.Double());
			Assert.AreEqual("5 [kg]", kg.ToString());

			kg = kg.ConvertTo().Kilo.Gramm.Value();
			Assert.AreEqual(5.0, kg.Double());
			Assert.AreEqual("5 [kg]", kg.ToString());

			kg = kg.ConvertTo().Gramm.Value();
			Assert.AreEqual(5000, kg.Double());
			Assert.AreEqual("5000 [g]", kg.ToString());

			var x = 5.SI();
			Assert.AreEqual((2.0 / 5.0).SI(), 2 / x);
			Assert.AreEqual((5.0 / 2.0).SI(), x / 2);
			Assert.AreEqual((2.0 * 5.0).SI(), 2 * x);
			Assert.AreEqual((5.0 * 2.0).SI(), x * 2);

			Assert.AreEqual((2.0 / 5.0).SI(), 2.0 / x);
			Assert.AreEqual((5.0 / 2.0).SI(), x / 2.0);
			Assert.AreEqual((2 * 5).SI(), 2.0 * x);
			Assert.AreEqual((5 * 2).SI(), x * 2.0);


			var y = 2.SI();
			Assert.AreEqual((2 * 5).SI(), y * x);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
		public void SITests_Addition_Subtraction()
		{
			var v1 = 600.SI<NewtonMeter>();
			var v2 = 455.SI<NewtonMeter>();
			NewtonMeter v3 = v1 + v2;

			NewtonMeter v4 = v1 - v2;

			var v5 = v1 * v2;
			Assert.IsTrue(v5.HasEqualUnit(0.SI().Square.Newton.Meter));
			Assert.AreEqual(v1.Double() * v2.Double(), v5.Double());

			var v6 = v1 / v2;
			Assert.IsTrue(v6.HasEqualUnit(0.SI()));
			Assert.AreEqual(v1.Double() / v2.Double(), v6.Double());

			var t = 10.SI<NewtonMeter>();
			var angVelo = 5.SI<PerSecond>();

			Watt w = t * angVelo;
			Watt w1 = angVelo * t;

			NewtonMeter t1 = w / angVelo;

			PerSecond angVelo1 = w / t;
			Second sec = t / w;
		}
	}
}