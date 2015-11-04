using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	[TestClass]
	public class SITest
	{
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

			var test = 2.0.SI<PerSecond>();
			var reziprok = 1.0 / test;
			Assert.AreEqual(0.5, reziprok.Value());
			Assert.IsTrue(1.SI<Second>().HasEqualUnit(reziprok));

			//add
			var angularVelocity2 = 400.SI<RoundsPerMinute>().Cast<PerSecond>();
			var angVeloSum = angularVelocity + angularVelocity2;
			Assert.IsInstanceOfType(angVeloSum, typeof(PerSecond));
			Assert.AreEqual((400.0 + 600) / 60 * 2 * Math.PI, angVeloSum.Value(), 0.0000001);
			AssertHelper.Exception<VectoException>(() => {
				var x = 500.SI().Watt + 300.SI().Newton;
			});

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
			AssertHelper.Exception<VectoException>(() => {
				var x = 40.SI<Newton>().ConvertTo().Watt;
			});
			var res1 = 40.SI<Newton>().ConvertTo().Newton;

			// Cast only allows the cast if the units are correct.
			AssertHelper.Exception<VectoException>(() => {
				var x = 40.SI().Newton.Cast<Watt>();
			});
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
		public void SI_Comparison_Operators()
		{
			var v1 = 600.SI<NewtonMeter>();
			var v2 = 455.SI<NewtonMeter>();
			var v3 = 600.SI<NewtonMeter>();
			var v4 = 100.SI<Watt>();
			var d = 700;

			Assert.IsTrue(v1 > v2);
			Assert.IsFalse(v1 < v2);
			AssertHelper.Exception<VectoException>(() => {
				var x = v1 < v4;
			},
				"Operator '<' can only operate on SI Objects with the same unit. Got: 600.0000 [Nm] < 100.0000 [W]");
			AssertHelper.Exception<VectoException>(() => {
				var x = v1 > v4;
			},
				"Operator '>' can only operate on SI Objects with the same unit. Got: 600.0000 [Nm] > 100.0000 [W]");
			AssertHelper.Exception<VectoException>(() => {
				var x = v1 <= v4;
			},
				"Operator '<=' can only operate on SI Objects with the same unit. Got: 600.0000 [Nm] <= 100.0000 [W]");
			AssertHelper.Exception<VectoException>(() => {
				var x = v1 >= v4;
			},
				"Operator '>=' can only operate on SI Objects with the same unit. Got: 600.0000 [Nm] >= 100.0000 [W]");

			SI si = null;
			Assert.IsFalse(si > 3);
			Assert.IsFalse(si < 3);
			Assert.IsFalse(si >= 3);
			Assert.IsFalse(si <= 3);

			Assert.IsFalse(3 > si);
			Assert.IsFalse(3 < si);
			Assert.IsFalse(si >= 3);
			Assert.IsFalse(si <= 3);


			Assert.IsTrue(v2 < v1);
			Assert.IsFalse(v2 > v1);

			Assert.IsTrue(v1 >= v2);
			Assert.IsFalse(v1 <= v2);

			Assert.IsTrue(v2 <= v1);
			Assert.IsFalse(v2 >= v1);

			Assert.IsTrue(v1 <= v3);
			Assert.IsTrue(v1 >= v3);

			Assert.IsTrue(v1 < d);
			Assert.IsFalse(v1 > d);
			Assert.IsFalse(v1 >= d);
			Assert.IsTrue(v1 <= d);


			Assert.AreEqual(1, new SI().CompareTo(null));
			Assert.AreEqual(1, new SI().CompareTo("not an SI"));
			Assert.AreEqual(-1, new SI().Meter.CompareTo(new SI().Kilo.Meter.Per.Hour));
			Assert.AreEqual(1, new SI().Newton.Meter.CompareTo(new SI().Meter));

			Assert.AreEqual(0, 1.SI().CompareTo(1.SI()));
			Assert.AreEqual(-1, 1.SI().CompareTo(2.SI()));
			Assert.AreEqual(1, 2.SI().CompareTo(1.SI()));
		}


		[TestMethod]
		public void SI_Test_Addition_Subtraction()
		{
			AssertHelper.AreRelativeEqual(3.SI(), 1.SI() + 2.SI());
			AssertHelper.AreRelativeEqual(-1.SI(), 1.SI() - 2.SI());

			AssertHelper.AreRelativeEqual(3.SI<Scalar>(), 1.SI<Scalar>() + 2.SI<Scalar>());
			AssertHelper.AreRelativeEqual(3.SI<Scalar>(), 1 + 2.SI<Scalar>());
			AssertHelper.AreRelativeEqual(3.SI<Scalar>(), 1.SI<Scalar>() + 2);
			AssertHelper.AreRelativeEqual(-1.SI<Scalar>(), 1.SI<Scalar>() - 2.SI<Scalar>());
			AssertHelper.AreRelativeEqual(-1.SI<Scalar>(), 1 - 2.SI<Scalar>());
			AssertHelper.AreRelativeEqual(-1.SI<Scalar>(), 1.SI<Scalar>() - 2);

			AssertHelper.AreRelativeEqual(3.SI<NewtonMeter>(), 1.SI<NewtonMeter>() + 2.SI<NewtonMeter>());
			AssertHelper.AreRelativeEqual(-1.SI<NewtonMeter>(), 1.SI<NewtonMeter>() - 2.SI<NewtonMeter>());

			AssertHelper.AreRelativeEqual(3.SI<NewtonMeter>(), 1.SI().Newton.Meter + 2.SI<NewtonMeter>());
			AssertHelper.AreRelativeEqual(-1.SI<NewtonMeter>(), 1.SI().Newton.Meter - 2.SI<NewtonMeter>());

			AssertHelper.AreRelativeEqual(3.SI<NewtonMeter>(), 1.SI<NewtonMeter>() + 2.SI().Newton.Meter);
			AssertHelper.AreRelativeEqual(-1.SI<NewtonMeter>(), 1.SI<NewtonMeter>() - 2.SI().Newton.Meter);

			AssertHelper.Exception<VectoException>(() => {
				var x = 1.SI().Second - 1.SI<Meter>();
			},
				"Operator '-' can only operate on SI Objects with the same unit. Got: 1.0000 [s] - 1.0000 [m]");
		}

		[TestMethod]
		public void SI_SpecialUnits()
		{
			Scalar scalar = 3.SI<Scalar>();
			AssertHelper.AreRelativeEqual(3.SI(), scalar);
			double scalarDouble = scalar;
			AssertHelper.AreRelativeEqual(3, scalarDouble);

			MeterPerSecond meterPerSecond = 2.SI<MeterPerSecond>();
			AssertHelper.AreRelativeEqual(2.SI().Meter.Per.Second, meterPerSecond);

			Second second = 1.SI<Second>();
			AssertHelper.AreRelativeEqual(1.SI().Second, second);

			Watt watt = 2.SI<Watt>();
			AssertHelper.AreRelativeEqual(2.SI().Watt, watt);

			PerSecond perSecond = 1.SI<PerSecond>();
			AssertHelper.AreRelativeEqual(1.SI().Per.Second, perSecond);

			RoundsPerMinute rpm = 20.SI<RoundsPerMinute>();
			AssertHelper.AreRelativeEqual(20.SI().Rounds.Per.Minute, rpm);
			AssertHelper.AreRelativeEqual(20.RPMtoRad(), rpm);
			AssertHelper.AreRelativeEqual(2.0943951023931953, rpm.Value());

			Radian radian = 30.SI<Radian>();
			AssertHelper.AreRelativeEqual(30.SI().Radian, radian);
			AssertHelper.AreRelativeEqual(30, radian.Value());

			Newton newton = 3.SI<Newton>();
			AssertHelper.AreRelativeEqual(3.SI().Newton, newton);

			NewtonMeter newtonMeter = 5.SI<NewtonMeter>();
			AssertHelper.AreRelativeEqual(5.SI().Newton.Meter, newtonMeter);
			AssertHelper.AreRelativeEqual(5.SI().Meter.Newton, newtonMeter);

			MeterPerSquareSecond meterPerSquareSecond = 3.SI<MeterPerSquareSecond>();
			AssertHelper.AreRelativeEqual(3.SI().Meter.Per.Square.Second, meterPerSquareSecond);

			Kilogram kilogram = 3.SI<Kilogram>();
			AssertHelper.AreRelativeEqual(3.SI().Kilo.Gramm, kilogram);
			AssertHelper.AreRelativeEqual(3, kilogram.Value());

			Ton ton = 5.SI<Ton>();
			AssertHelper.AreRelativeEqual(5.SI().Ton, ton);
			AssertHelper.AreRelativeEqual(5000.SI<Kilogram>(), ton);

			SquareMeter squareMeter = 3.SI<SquareMeter>();
			AssertHelper.AreRelativeEqual(3.SI().Square.Meter, squareMeter);

			CubicMeter cubicMeter = 3.SI<CubicMeter>();
			AssertHelper.AreRelativeEqual(3.SI().Cubic.Meter, cubicMeter);

			KilogramSquareMeter kilogramSquareMeter = 3.SI<KilogramSquareMeter>();
			AssertHelper.AreRelativeEqual(3.SI().Kilo.Gramm.Square.Meter, kilogramSquareMeter);

			KilogramPerWattSecond kilogramPerWattSecond = 3.SI<KilogramPerWattSecond>();
			AssertHelper.AreRelativeEqual(3.SI().Kilo.Gramm.Per.Watt.Second, kilogramPerWattSecond);
		}

		/// <summary>
		/// VECTO-111
		/// </summary>
		[TestMethod]
		public void SI_ReziprokDivision()
		{
			var test = 2.0.SI<Second>();

			var actual = 1.0 / test;
			var expected = 0.5.SI<PerSecond>();

			AssertHelper.AreRelativeEqual(expected, actual);
		}

		[TestMethod]
		public void SI_Multiplication_Division()
		{
			AssertHelper.AreRelativeEqual(12.SI(), 3.SI() * 4.SI());
			AssertHelper.AreRelativeEqual(12.SI(), 3 * 4.SI());
			AssertHelper.AreRelativeEqual(12.SI(), 3.SI() * 4);

			AssertHelper.AreRelativeEqual(12.SI<NewtonMeter>(), 3.SI<Newton>() * 4.SI<Meter>());
			AssertHelper.AreRelativeEqual(12.SI<NewtonMeter>(), 3 * 4.SI<NewtonMeter>());
			AssertHelper.AreRelativeEqual(12.SI<NewtonMeter>(), 3.SI<NewtonMeter>() * 4);
			AssertHelper.AreRelativeEqual(12.SI().Square.Newton.Meter, 3.SI<NewtonMeter>() * 4.SI<NewtonMeter>());

			AssertHelper.AreRelativeEqual(3.SI(), 12.SI() / 4);
			AssertHelper.AreRelativeEqual(3.SI(), 12.SI() / 4.SI());
			AssertHelper.AreRelativeEqual(3.SI(), 12.SI<NewtonMeter>() / 4.SI<NewtonMeter>());
			AssertHelper.AreRelativeEqual(3.SI<Scalar>(), 12.SI<NewtonMeter>() / 4.SI<NewtonMeter>());

			AssertHelper.AreRelativeEqual(3.SI<NewtonMeter>(), 12.SI<NewtonMeter>() / 4);
			AssertHelper.AreRelativeEqual(3.SI().Per.Newton.Meter, 12 / 4.SI<NewtonMeter>());


			var newtonMeter = 10.SI<NewtonMeter>();
			var perSecond = 5.SI<PerSecond>();
			var watt = (10 * 5).SI<Watt>();
			var second = (1.0 / 5.0).SI<Second>();

			AssertHelper.AreRelativeEqual(watt, newtonMeter * perSecond);
			AssertHelper.AreRelativeEqual(watt, perSecond * newtonMeter);

			AssertHelper.AreRelativeEqual(newtonMeter, watt / perSecond);
			AssertHelper.AreRelativeEqual(perSecond, watt / newtonMeter);

			AssertHelper.AreRelativeEqual(second, newtonMeter / watt);
		}

		[TestMethod]
		public void SI_MeterPerSecond_Div_Meter()
		{
			PerSecond actual = 6.SI<MeterPerSecond>() / 2.SI<Meter>();
			AssertHelper.AreRelativeEqual(3.SI().Per.Second, actual);
		}

		[TestMethod]
		public void SI_SimplifyUnits()
		{
			AssertHelper.AreRelativeEqual(3.SI(), 18.SI().Kilo.Gramm / 6.SI().Kilo.Gramm);
			AssertHelper.AreRelativeEqual(3.SI(), 18.SI<NewtonMeter>() / 6.SI<NewtonMeter>());

			AssertHelper.AreRelativeEqual(18.SI(), 3.SI().Kilo.Gramm * 6.SI().Per.Kilo.Gramm);
			AssertHelper.AreRelativeEqual(18.SI<Meter>(), 3.SI().Kilo.Gramm.Meter * 6.SI().Per.Kilo.Gramm);


			AssertHelper.AreRelativeEqual(3.SI().Kilo.Gramm.Square.Meter.Per.Cubic.Second, 3.SI<Watt>());
			AssertHelper.AreRelativeEqual(3.SI().Kilo.Gramm.Meter.Per.Square.Second, 3.SI<Newton>());
			AssertHelper.AreRelativeEqual(3000.SI().Kilo.Gramm, 3.SI<Ton>());
			AssertHelper.AreRelativeEqual(3.SI().Kilo.Kilo.Gramm.ConvertTo().Ton, 3.SI<Ton>().ConvertTo().Ton);

			AssertHelper.AreRelativeEqual(3.SI<Meter>(), 3000.SI().Milli.Meter);

			AssertHelper.AreRelativeEqual(36.SI().Square.Newton.Meter, 6.SI<NewtonMeter>() * 6.SI<NewtonMeter>());
			AssertHelper.AreRelativeEqual(36.SI().Newton.Newton.Meter.Meter, 6.SI<NewtonMeter>() * 6.SI<NewtonMeter>());

			AssertHelper.AreRelativeEqual(3.SI().Meter.Per.Second, 3.SI<Newton>().Second.Per.Kilo.Gramm);
		}

		[TestMethod]
		public void SI_Math()
		{
			AssertHelper.AreRelativeEqual(-3, -3.SI().Value());
			AssertHelper.AreRelativeEqual(3.SI(), (-3).SI().Abs());

			AssertHelper.AreRelativeEqual(6.SI(), 36.SI().Sqrt());
			AssertHelper.AreRelativeEqual(6.SI<NewtonMeter>(), (6.SI<NewtonMeter>() * 6.SI<NewtonMeter>()).Sqrt());
			AssertHelper.AreRelativeEqual(6.SI().Second, 36.SI().Square.Second.Sqrt());

			AssertHelper.Exception<VectoException>(() => 36.SI().Second.Sqrt(),
				"The squareroot cannot be calculated because the Unit-Exponents are not even: [s]");
		}

		[TestMethod]
		public void SI_Equality()
		{
			Assert.AreEqual(3.SI(), 3.SI());
			Assert.AreEqual(3.SI<NewtonMeter>(), 3.SI<NewtonMeter>());


			Assert.IsFalse(3.SI<NewtonMeter>().IsEqual(4.SI<NewtonMeter>()));
			Assert.IsFalse(3.SI<NewtonMeter>().IsEqual(3.SI<Meter>()));

			Assert.IsTrue(3.SI().IsEqual(4, 10));

			var x = 4.SI();
			var y = x;
			var z = 4.SI();
			Assert.IsTrue(x.Equals(y));

			Assert.IsFalse(3.SI().Equals(null));
			Assert.IsFalse(3.SI().IsEqual(4.SI()));
			Assert.IsTrue(z.Equals(x));
			Assert.IsFalse(3.SI().Equals(3.SI<Newton>()));

			var newton1 = 3.SI<Newton>();
			var newton2 = 3.SI<Newton>();
			Assert.IsTrue(newton1.Equals(newton2));


			Assert.IsTrue(3.SI().IsEqual(3.SI()));
			Assert.IsTrue(3.SI().IsEqual(3));

			Assert.IsFalse(3.SI().IsEqual(2.9.SI()));
			Assert.IsFalse(3.SI().IsEqual(2.9));

			// just calling to test wether the functions are not throwing an exception.
			3.SI().GetHashCode();
			3.0.SI().GetHashCode();
			4.SI<NewtonMeter>().GetHashCode();
		}

		[TestMethod]
		public void SI_Output()
		{
			Assert.AreEqual("3.0000", 3.SI().ToOutputFormat());
			Assert.AreEqual("3.0000 [-]", 3.SI().ToOutputFormat(showUnit: true));
			Assert.AreEqual("3.5000", 3.5.SI().ToOutputFormat());
			Assert.AreEqual("3.5000", 3.5.SI<Newton>().ToOutputFormat());
			Assert.AreEqual("3.50 [N]", 3.5.SI<Newton>().ToOutputFormat(2, showUnit: true));
			Assert.AreEqual("18.00 [m/s]", 5.SI<MeterPerSecond>().ToOutputFormat(2, 3.6, true));
			Assert.AreEqual("18.0000", 5.SI<MeterPerSecond>().ToOutputFormat(outputFactor: 3.6));
		}
	}
}