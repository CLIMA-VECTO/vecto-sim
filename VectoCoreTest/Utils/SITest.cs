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
				Assert.Fail("Expected Exception {0}, but no exception occured.", typeof(T));
			} catch (T ex) {
				if (message != null) {
					Assert.AreEqual(message, ex.Message);
				}
			}
		}

		[TestMethod]
		public void SI_TypicalUsageTest()
		{
			//mult
			var angularVelocity = 600.RPMtoRad();
			var torque = 1500.SI<NewtonMeter>();
			var power = angularVelocity * torque;
			Assert.IsInstanceOfType(power, typeof(Watt));
			Assert.AreEqual(600.0 / 60 * 2 * Math.PI * 1500, power.Value());

			var siStandardMult = power * torque;
			Assert.IsInstanceOfType(siStandardMult, typeof(SI));
			Assert.AreEqual(600.0 / 60 * 2 * Math.PI * 1500 * 1500, siStandardMult.Value());
			Assert.IsTrue(siStandardMult.HasEqualUnit(new SI().Watt.Newton.Meter));

			//div
			var torque2 = power / angularVelocity;
			Assert.IsInstanceOfType(torque2, typeof(NewtonMeter));
			Assert.AreEqual(1500, torque2.Value());

			var siStandardDiv = power / power;
			Assert.IsInstanceOfType(siStandardMult, typeof(SI));
			Assert.IsTrue(siStandardDiv.HasEqualUnit(new SI()));
			Assert.AreEqual(600.0 / 60 * 2 * Math.PI * 1500 * 1500, siStandardMult.Value());

			var force = torque / 100.SI<Meter>();
			Assert.IsInstanceOfType(force, typeof(Newton));
			Assert.AreEqual(15, force.Value());


			//add
			var angularVelocity2 = 400.SI<RoundsPerMinute>().Cast<PerSecond>();
			var angVeloSum = angularVelocity + angularVelocity2;
			Assert.IsInstanceOfType(angVeloSum, typeof(PerSecond));
			Assert.AreEqual((400.0 + 600) / 60 * 2 * Math.PI, angVeloSum.Value(), 0.0000001);
			AssertException<VectoException>(() => { var x = 500.SI().Watt + 300.SI().Newton; });

			//subtract
			var angVeloDiff = angularVelocity - angularVelocity2;
			Assert.IsInstanceOfType(angVeloDiff, typeof(PerSecond));
			Assert.AreEqual((600.0 - 400) / 60 * 2 * Math.PI, angVeloDiff.Value(), 0.0000001);

			//general si unit
			var generalSIUnit = 3600000000.0.SI().Gramm.Per.Kilo.Watt.Hour.ConvertTo().Kilo.Gramm.Per.Watt.Second;
			Assert.IsInstanceOfType(generalSIUnit, typeof(SI));
			Assert.AreEqual(1, generalSIUnit.Value());


			//type conversion
			var engineSpeed = 600.0;
			var angularVelocity3 = engineSpeed.RPMtoRad();

			// convert between units measures
			var angularVelocity4 = engineSpeed.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second;

			// cast SI to specialized unit classes.
			var angularVelocity5 = angularVelocity4.Cast<PerSecond>();
			Assert.AreEqual(angularVelocity3, angularVelocity5);
			Assert.AreEqual(angularVelocity3.Value(), angularVelocity4.Value());
			Assert.IsInstanceOfType(angularVelocity3, typeof(PerSecond));
			Assert.IsInstanceOfType(angularVelocity5, typeof(PerSecond));
			Assert.IsInstanceOfType(angularVelocity4, typeof(SI));


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
			Assert.AreEqual(0.0, si.Value());
			Assert.AreEqual("0.0000 [-]", si.ToString());
			Assert.IsTrue(si.HasEqualUnit(new SI()));

			var si2 = 5.SI().Watt;
			Assert.AreEqual("5.0000 [W]", si2.ToString());

			var si3 = 2.SI().Radian.Per.Second;
			Assert.AreEqual("2.0000 [1/s]", si3.ToString());

			var si4 = si2 * si3;
			Assert.AreEqual("10.0000 [W/s]", si4.ToString());
			Assert.IsTrue(si4.HasEqualUnit(new SI().Watt.Per.Second));
			Assert.AreEqual("10.0000 [kgmm/ssss]", si4.ToBasicUnits().ToString());


			var kg = 5.SI().Kilo.Gramm;
			Assert.AreEqual(5.0, kg.Value());
			Assert.AreEqual("5.0000 [kg]", kg.ToString());

			kg = kg.ConvertTo().Kilo.Gramm.Clone();
			Assert.AreEqual(5.0, kg.Value());
			Assert.AreEqual("5.0000 [kg]", kg.ToString());

			kg = kg.ConvertTo().Gramm.Clone();
			Assert.AreEqual(5000, kg.Value());
			Assert.AreEqual("5000.0000 [g]", kg.ToString());

			var x = 5.SI();
			Assert.AreEqual((2.0 / 5.0).SI(), 2 / x);
			Assert.AreEqual((5.0 / 2.0).SI(), x / 2);
			Assert.AreEqual((2.0 * 5.0).SI(), 2 * x);
			Assert.AreEqual((5.0 * 2.0).SI(), x * 2);

			Assert.AreEqual((2.0 / 5.0).SI(), 2.0 / x);
			Assert.AreEqual((5.0 / 2.0).SI(), x / 2.0);
			Assert.AreEqual((2 * 5).SI(), 2.0 * x);
			Assert.AreEqual((5 * 2).SI(), x * 2.0);


			//var y = 2.SI();
			//Assert.AreEqual((2 * 5).SI(), y * x);

			//var percent = 10.SI<Radian>().ConvertTo().GradientPercent;
			//Assert.AreEqual(67.975.ToString("F3") + " [Percent]", percent.ToString("F3"));
			//Assert.AreEqual(67.975, percent.Value(), 0.001);

			Assert.AreEqual(45.0 / 180.0 * Math.PI, VectoMath.InclinationToAngle(1).Value(), 0.000001);
		}

		[TestMethod]
		public void SI_Test_Addition_Subtraction()
		{
			var v1 = 600.SI<NewtonMeter>();
			var v2 = 455.SI<NewtonMeter>();

			Assert.IsTrue(v1 > v2);
			Assert.IsTrue(v2 < v1);
			Assert.IsTrue(v1 >= v2);
			Assert.IsTrue(v2 <= v1);

			Assert.IsFalse(v1 < v2);
			Assert.IsFalse(v2 > v1);
			Assert.IsFalse(v1 <= v2);
			Assert.IsFalse(v2 >= v1);

			Assert.AreEqual(1, new SI().CompareTo(null));
			Assert.AreEqual(1, new SI().CompareTo("not an SI"));
			Assert.AreEqual(-1, new SI().Meter.CompareTo(new SI().Kilo.Meter.Per.Hour));
			Assert.AreEqual(1, new SI().Newton.Meter.CompareTo(new SI().Meter));

			Assert.AreEqual(0, 1.SI().CompareTo(1.SI()));
			Assert.AreEqual(-1, 1.SI().CompareTo(2.SI()));
			Assert.AreEqual(1, 2.SI().CompareTo(1.SI()));


			NewtonMeter v3 = v1 + v2;

			NewtonMeter v4 = v1 - v2;

			var v5 = v1 * v2;
			Assert.IsTrue(v5.HasEqualUnit(0.SI().Square.Newton.Meter));
			Assert.AreEqual(v1.Value() * v2.Value(), v5.Value());

			var v6 = v1 / v2;
			Assert.IsTrue(v6.HasEqualUnit(0.SI()));
			Assert.AreEqual(v1.Value() / v2.Value(), v6.Value());

			var t = 10.SI<NewtonMeter>();
			var angVelo = 5.SI<PerSecond>();

			Watt w = t * angVelo;
			Watt w1 = angVelo * t;

			NewtonMeter t1 = w / angVelo;

			PerSecond angVelo1 = w / t;
			Second sec = t / w;
		}

		[TestMethod]
		public void SI_SpecialUnits()
		{
			2.SI<MeterPerSecond>();
			1.SI<Second>();
			2.SI<Watt>();
			1.SI<PerSecond>();
			2.SI<RoundsPerMinute>();
			3.SI<Newton>();
			4.SI<Radian>();
			5.SI<NewtonMeter>();
		}
	}
}