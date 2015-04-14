using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	[TestClass]
	public class SITest
	{
		/// <summary>
		/// Assert an expected Exception.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		/// <param name="message"></param>
		public static void AssertException<T>(Action func, string message = null) where T : Exception
		{
			try {
				func();
				Assert.Fail("Expected Exception {0}, but no exception occured.", typeof (T));
			} catch (T ex) {
				if (message != null) {
					Assert.AreEqual(message, ex.Message);
				}
			}
		}

		public void SI_TypicalUsageTest()
		{
			//mult
			var angularVelocity = 600.RPMtoRad();
			var torque = 1500.SI<NewtonMeter>();
			var power = angularVelocity * torque;
			Assert.IsInstanceOfType(power, typeof (Watt));
			Assert.AreEqual(600 * 1500, power.Double());

			var siStandardMult = power * torque;
			Assert.IsInstanceOfType(siStandardMult, typeof (SI));
			Assert.AreEqual(600 * 1500 * 1500, siStandardMult.Double());
			Assert.IsTrue(siStandardMult.HasEqualUnit(new SI().Watt.Newton.Meter));

			//div
			var torque2 = power / angularVelocity;
			Assert.IsInstanceOfType(torque2, typeof (NewtonMeter));
			Assert.AreEqual(1500, torque2.Double());

			var siStandardDiv = power / power;
			Assert.IsInstanceOfType(siStandardMult, typeof (SI));
			Assert.IsTrue(siStandardDiv.HasEqualUnit(new SI()));
			Assert.AreEqual(600 * 1500 * 1500, siStandardMult.Double());


			//add
			var angularVelocity2 = 400.SI<RoundsPerMinute>().Cast<PerSecond>();
			var angVeloSum = angularVelocity + angularVelocity2;
			Assert.IsInstanceOfType(angVeloSum, typeof (PerSecond));
			Assert.AreEqual(400 + 600, angVeloSum.Double());
			AssertException<VectoException>(() => { var x = 500.SI().Watt + 300.SI().Newton; });

			//subtract
			var angVeloDiff = angularVelocity - angularVelocity2;
			Assert.IsInstanceOfType(angVeloDiff, typeof (PerSecond));
			Assert.AreEqual(600 - 400, angVeloDiff.Double());

			//general si unit
			var generalSIUnit = 60000.SI().Gramm.Per.Kilo.Watt.Hour.ConvertTo().Kilo.Gramm.Per.Watt.Second;
			Assert.IsInstanceOfType(generalSIUnit, typeof (SI));
			Assert.AreEqual(1, generalSIUnit.Double());


			//type conversion
			var engineSpeed = 600;
			var angularVelocity3 = engineSpeed.RPMtoRad();

			// convert between units measures
			var angularVelocity4 = engineSpeed.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second;

			// cast SI to specialized unit classes.
			var angularVelocity5 = angularVelocity2.Cast<PerSecond>();
			Assert.AreEqual(angularVelocity3, angularVelocity5);
			Assert.AreEqual(angularVelocity3.Double(), angularVelocity4.Double());
			Assert.IsInstanceOfType(angularVelocity3, typeof (PerSecond));
			Assert.IsInstanceOfType(angularVelocity5, typeof (PerSecond));
			Assert.IsInstanceOfType(angularVelocity4, typeof (SI));


			// ConvertTo only allows conversion if the units are correct.
			AssertException<VectoException>(() => { var x = 40.SI<Newton>().ConvertTo().Watt; });
			var res1 = 40.SI<Newton>().ConvertTo().Newton;

			// Cast only allows the cast if the units are correct.
			AssertException<VectoException>(() => { var x = 40.SI().Newton.Cast<Watt>(); });
			var res2 = 40.SI().Newton.Cast<Newton>();
		}


		[TestMethod]
		public void SI_Test()
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

			var percent = 10.SI<Radian>().ConvertTo().GradientPercent;
			Assert.AreEqual(67.975.ToString("F3") + " [Percent]", percent.ToString("F3"));
			Assert.AreEqual(67.975, percent.Double(), 0.001);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
		public void SI_Test_Addition_Subtraction()
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