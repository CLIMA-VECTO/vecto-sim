using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	public class MeterPerSecond : SIBase<MeterPerSecond>
	{
		public MeterPerSecond(double val = 0) : base(val, new SI().Meter.Per.Second) {}
	}

	public class Radian : SIBase<Radian>
	{
		public Radian(double val = 0) : base(val, new SI().Radian) {}
	}

	public class Second : SIBase<Second>
	{
		public Second(double val = 0) : base(val, new SI().Second) {}
	}

	public class Watt : SIBase<Watt>
	{
		public Watt(double val = 0) : base(val, new SI().Watt) {}

		public static RadianPerSecond operator /(Watt watt, NewtonMeter newtonMeter)
		{
			return ((watt as SI) / newtonMeter).Cast<RadianPerSecond>();
		}

		public static NewtonMeter operator /(Watt watt, RadianPerSecond radianPerSecond)
		{
			return ((watt as SI) / radianPerSecond).Cast<NewtonMeter>();
		}
	}

	public class RadianPerSecond : SIBase<RadianPerSecond>
	{
		public RadianPerSecond(double val = 0) : base(val, new SI().Radian.Per.Second) {}

		public static Watt operator *(RadianPerSecond radianPerSecond, NewtonMeter newtonMeter)
		{
			return ((radianPerSecond as SI) * newtonMeter).Cast<Watt>();
		}
	}

	public class RoundsPerMinute : SIBase<RoundsPerMinute>
	{
		public RoundsPerMinute(double val = 0) : base(val, new SI().Rounds.Per.Minute) {}
	}

	public class NewtonMeter : SIBase<NewtonMeter>
	{
		public NewtonMeter(double val = 0) : base(val, new SI().Newton.Meter) {}

		public static Watt operator *(NewtonMeter newtonMeter, RadianPerSecond radianPerSecond)
		{
			return ((newtonMeter as SI) * radianPerSecond).Cast<Watt>();
		}

		public static Second operator /(NewtonMeter newtonMeter, Watt watt)
		{
			return ((newtonMeter as SI) / watt).Cast<Second>();
		}
	}

	public class Newton : SIBase<Newton>
	{
		public Newton(double val = 0) : base(val, new SI().Newton) {}
	}

	public abstract class SIBase<T> : SI where T : SIBase<T>
	{
		protected SIBase(double val = 0) : base(val) {}
		protected SIBase(double val, SI unit) : base(val, unit) {}

		#region Operators

		public static T operator +(SIBase<T> si1, SIBase<T> si2)
		{
			return (si1 as SI) + si2;
		}

		public static T operator +(SIBase<T> si1, SI si2)
		{
			return ((si1 as SI) + si2).Cast<T>();
		}

		public static T operator +(SI si1, SIBase<T> si2)
		{
			return si2 + si1;
		}

		public static T operator +(SIBase<T> si1, double d)
		{
			return ((si1 as SI) + d).Cast<T>();
		}

		public static T operator +(double d, SIBase<T> si)
		{
			return si + d;
		}

		public static T operator -(SIBase<T> si1, SIBase<T> si2)
		{
			return (si1 as SI) - si2;
		}

		public static T operator -(SIBase<T> si1, SI si2)
		{
			return -si2 + si1;
		}

		public static T operator -(SI si1, SIBase<T> si2)
		{
			return (si1 - (si2 as SI)).Cast<T>();
		}

		public static T operator -(SIBase<T> si, double d)
		{
			return ((si as SI) - d).Cast<T>();
		}

		public static T operator -(double d, SIBase<T> si)
		{
			return (d - (si as SI)).Cast<T>();
		}

		public static T operator *(double d, SIBase<T> si)
		{
			return si * d;
		}

		public static T operator *(SIBase<T> si, double d)
		{
			return ((si as SI) * d).Cast<T>();
		}

		public static T operator /(double d, SIBase<T> si)
		{
			return si / d;
		}

		public static T operator /(SIBase<T> si, double d)
		{
			return ((si as SI) / d).Cast<T>();
		}

		#endregion
	}


	[DataContract]
	public class SI
	{
		[DataMember] protected readonly string[] Denominator;
		[DataMember] protected readonly int Exponent;
		[DataMember] protected readonly string[] Numerator;
		[DataMember] protected readonly bool Reciproc;
		[DataMember] protected readonly bool Reverse;
		[DataMember] protected readonly double Val;

		public SI(double val = 0.0)
		{
			Val = val;
			Reciproc = false;
			Reverse = false;
			Numerator = new string[0];
			Denominator = new string[0];
			Exponent = 1;
		}

		protected SI(double val, IEnumerable<string> numerator, IEnumerable<string> denominator,
			bool reciproc = false,
			bool reverse = false, int exponent = 1)
		{
			Contract.Requires(numerator != null);
			Contract.Requires(denominator != null);

			Val = val;
			Reciproc = reciproc;
			Reverse = reverse;
			Exponent = exponent;

			var tmpNumerator = numerator.ToList();
			var tmpDenominator = denominator.ToList();

			foreach (var v in tmpDenominator.ToArray().Where(v => tmpNumerator.Contains(v))) {
				tmpNumerator.Remove(v);
				tmpDenominator.Remove(v);
			}

			Numerator = tmpNumerator.ToArray();
			Denominator = tmpDenominator.ToArray();
		}

		protected SI(double val, SI unit)
			: this(val, unit.Numerator, unit.Denominator) {}

		protected SI(SI si, double? factor = null, string fromUnit = null, string toUnit = null,
			bool? reciproc = null, bool? reverse = null, int? exponent = null)
		{
			Contract.Requires(si != null);
			Contract.Requires(si.Denominator != null);
			Contract.Requires(si.Numerator != null);

			var numerator = si.Denominator.ToList();
			var denominator = si.Numerator.ToList();

			Val = si.Val;
			Reciproc = reciproc ?? si.Reciproc;
			Reverse = reverse ?? si.Reverse;
			Exponent = exponent ?? si.Exponent;

			if (Reverse) {
				var tmp = fromUnit;
				fromUnit = toUnit;
				toUnit = tmp;
				factor = 1 / factor;
			}

			for (var i = 0; i < Exponent; i++) {
				if (!Reciproc) {
					UpdateUnit(fromUnit, toUnit, denominator);
					if (factor.HasValue) {
						Val *= factor.Value;
					}
				} else {
					UpdateUnit(fromUnit, toUnit, numerator);
					if (factor.HasValue) {
						Val /= factor.Value;
					}
				}
			}

			foreach (var v in numerator.ToArray().Where(v => denominator.Contains(v))) {
				denominator.Remove(v);
				numerator.Remove(v);
			}

			Numerator = denominator.ToArray();
			Denominator = numerator.ToArray();
		}

		private void UpdateUnit(string fromUnit, string toUnit, ICollection<string> units)
		{
			if (Reverse && !string.IsNullOrEmpty(fromUnit)) {
				if (units.Contains(fromUnit)) {
					units.Remove(fromUnit);
				} else {
					throw new VectoException("Unit missing. Conversion not possible.");
				}
			}

			if (!string.IsNullOrEmpty(toUnit)) {
				units.Add(toUnit);
			}
		}

		/// <summary>
		/// Converts the SI unit to another SI unit, defined by term(s) following after the ConvertTo().
		/// The Conversion Mode is active until an arithmetic operator is used (+,-,*,/), 
		/// or the .Value-Method, or the .Cast-Method were called.
		/// ATTENTION: Before returning an SI Unit, ensure to cancel Conversion Mode (with .Value or .Cast).
		/// </summary>
		/// <returns></returns>
		public SI ConvertTo()
		{
			return new SI(Linear, reciproc: false, reverse: true);
		}

		/// <summary>
		/// Casts the SI Unit to the concrete unit type if the units are correct.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Cast<T>() where T : SIBase<T>
		{
			var t = (T) Activator.CreateInstance(typeof (T), Val);
			Contract.Assert(HasEqualUnit(t), string.Format("SI Unit Conversion failed: From {0} to {1}", this, t));
			return t;
		}

		public SI ToBasicUnits()
		{
			var numerator = new List<string>();
			var denominator = new List<string>();
			Numerator.ToList().ForEach(unit => ConvertToBasicUnits(unit, numerator, denominator));
			Denominator.ToList().ForEach(unit => ConvertToBasicUnits(unit, denominator, numerator));
			return new SI(Val, numerator, denominator);
		}

		private static void ConvertToBasicUnits(string unit, ICollection<string> numerator,
			ICollection<string> denominator)
		{
			switch (unit) {
				case "W":
					numerator.Add("k");
					numerator.Add("g");
					numerator.Add("m");
					numerator.Add("m");
					denominator.Add("s");
					denominator.Add("s");
					denominator.Add("s");
					break;
				case "N":
					numerator.Add("k");
					numerator.Add("g");
					numerator.Add("m");
					denominator.Add("s");
					denominator.Add("s");
					break;
				default:
					numerator.Add(unit);
					break;
			}
		}

		/// <summary>
		///     Gets the basic double value.
		/// </summary>
		public double Double()
		{
			return Val;
		}

		public SI Value()
		{
			return new SI(Val, Numerator, Denominator);
		}

		public SI Abs()
		{
			return new SI(Math.Abs(Val), this);
		}

		#region Unit Definitions

		/// <summary>
		///     Defines the denominator by the terms following after the Per.
		/// </summary>
		[DebuggerHidden]
		public SI Per
		{
			get { return new SI(Linear, reciproc: !Reciproc); }
		}

		/// <summary>
		///     Takes all following terms as cubic terms (=to the power of 3).
		/// </summary>
		[DebuggerHidden]
		public SI Cubic
		{
			get { return new SI(this, exponent: 3); }
		}

		/// <summary>
		///     Takes all following terms as quadratic terms (=to the power of 2).
		/// </summary>
		[DebuggerHidden]
		public SI Square
		{
			get { return new SI(this, exponent: 2); }
		}

		/// <summary>
		///     Takes all following terms as linear terms (=to the power of 1).
		/// </summary>
		[DebuggerHidden]
		public SI Linear
		{
			get { return new SI(this, exponent: 1); }
		}

		/// <summary>
		///     [g] (to basic unit: [kg])
		/// </summary>
		[DebuggerHidden]
		public SI Gramm
		{
			get { return new SI(new SI(this, toUnit: "k"), 0.001, "g", "g"); }
		}

		/// <summary>
		///     [N]
		/// </summary>
		[DebuggerHidden]
		public SI Newton
		{
			get { return new SI(this, fromUnit: "N", toUnit: "N"); }
		}

		/// <summary>
		///     [W]
		/// </summary>
		[DebuggerHidden]
		public SI Watt
		{
			get { return new SI(this, fromUnit: "W", toUnit: "W"); }
		}

		/// <summary>
		///     [m]
		/// </summary>
		[DebuggerHidden]
		public SI Meter
		{
			get { return new SI(this, fromUnit: "m", toUnit: "m"); }
		}

		/// <summary>
		///     [s]
		/// </summary>
		[DebuggerHidden]
		public SI Second
		{
			get { return new SI(this, fromUnit: "s", toUnit: "s"); }
		}

		/// <summary>
		///     [rad]
		/// </summary>
		[DebuggerHidden]
		public SI Radian
		{
			get { return new SI(this, fromUnit: "rad", toUnit: "rad"); }
		}

		public SI GradientPercent
		{
			get { return new SI(this, factor: Math.Atan(Val) / Val, fromUnit: "%", toUnit: "rad"); }
		}

		/// <summary>
		///     Converts to/from Radiant
		/// </summary>
		[DebuggerHidden]
		public SI Rounds
		{
			get { return new SI(this, 2 * Math.PI, toUnit: "rad"); }
		}

		/// <summary>
		///     Converts to/from Second
		/// </summary>
		[DebuggerHidden]
		public SI Hour
		{
			get { return new SI(this, 3600.0, "h", "s"); }
		}

		/// <summary>
		///     Converts to/from Second
		/// </summary>
		[DebuggerHidden]
		public SI Minute
		{
			get { return new SI(this, 60.0, "min", "s"); }
		}

		/// <summary>
		///     Converts to/from 1000 * Basic Unit
		/// </summary>
		[DebuggerHidden]
		public SI Kilo
		{
			get { return new SI(this, 1000.0, "k"); }
		}

		/// <summary>
		///     Converts to/from Basic Unit / 100
		/// </summary>
		[DebuggerHidden]
		public SI Centi
		{
			get { return new SI(this, 1.0 / 100.0, "c"); }
		}

		#endregion

		#region Operators

		public static SI operator +(SI si1, SI si2)
		{
			Contract.Requires(si1.HasEqualUnit(si2));

			return new SI(si1.Val + si2.Val, si1.Numerator, si1.Denominator);
		}

		public static SI operator -(SI si1, SI si2)
		{
			Contract.Requires(si1.HasEqualUnit(si2));

			return new SI(si1.Val - si2.Val, si1.Numerator, si1.Denominator);
		}

		public static SI operator *(SI si1, SI si2)
		{
			var numerator = si1.Numerator.Concat(si2.Numerator).Where(d => d != "rad");
			var denominator = si1.Denominator.Concat(si2.Denominator).Where(d => d != "rad");
			return new SI(si1.Val * si2.Val, numerator, denominator);
		}

		public static SI operator /(SI si1, SI si2)
		{
			var numerator = si1.Numerator.Concat(si2.Denominator).Where(d => d != "rad");
			var denominator = si1.Denominator.Concat(si2.Numerator).Where(d => d != "rad");
			return new SI(si1.Val / si2.Val, numerator, denominator);
		}

		public static SI operator +(SI si1, double d)
		{
			return new SI(si1.Val + d, si1);
		}

		public static SI operator +(double d, SI si1)
		{
			return si1 + d;
		}

		public static SI operator -(SI si1, double d)
		{
			return new SI(si1.Val - d, si1);
		}

		public static SI operator -(double d, SI si1)
		{
			return new SI(d - si1.Val, si1);
		}

		public static SI operator -(SI si1)
		{
			return 0 - si1;
		}

		public static SI operator *(SI si1, double d)
		{
			return new SI(si1.Val * d, si1);
		}

		public static SI operator *(double d, SI si1)
		{
			return new SI(d * si1.Val, si1);
		}

		public static SI operator /(SI si1, double d)
		{
			return new SI(si1.Val / d, si1);
		}

		public static SI operator /(double d, SI si1)
		{
			return new SI(d / si1.Val, si1);
		}

		public static bool operator <(SI si1, SI si2)
		{
			Contract.Requires(si1.HasEqualUnit(si2));
			return si1.Val < si2.Val;
		}

		public static bool operator >(SI si1, SI si2)
		{
			Contract.Requires(si1.HasEqualUnit(si2));
			return si1.Val > si2.Val;
		}

		public static bool operator <=(SI si1, SI si2)
		{
			Contract.Requires(si1.HasEqualUnit(si2));
			return si1.Val <= si2.Val;
		}

		public static bool operator >=(SI si1, SI si2)
		{
			Contract.Requires(si1.HasEqualUnit(si2));
			return si1.Val >= si2.Val;
		}

		public static bool operator <(SI si1, double d)
		{
			return si1.Val < d;
		}

		public static bool operator >(SI si1, double d)
		{
			return si1.Val > d;
		}

		public static bool operator <=(SI si1, double d)
		{
			return si1.Val <= d;
		}

		public static bool operator >=(SI si1, double d)
		{
			return si1.Val >= d;
		}

		#endregion

		#region Double Conversion

		/// <summary>
		///     Casts an SI Unit to an double.
		/// </summary>
		/// <param name="si"></param>
		/// <returns></returns>
		public static explicit operator double(SI si)
		{
			return si.Val;
		}

		/// <summary>
		///     Casts a double to an SI Unit.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static explicit operator SI(double d)
		{
			return new SI(d);
		}

		#endregion

		#region ToString

		/// <summary>
		///     Returns the Unit Part of the SI Unit Expression.
		/// </summary>
		private string GetUnitString()
		{
			if (Denominator.Any()) {
				if (Numerator.Any()) {
					return string.Format("{0}/{1}", string.Join("", Numerator), string.Join("", Denominator));
				} else {
					return string.Format("1/{0}", string.Join("", Denominator));
				}
			}

			if (Numerator.Any()) {
				return string.Format("{0}", string.Join("", Numerator));
			}

			return "-";
		}

		/// <summary>
		///     Returns the String representation.
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0} [{1}]", Val, GetUnitString());
		}

		#endregion

		#region Equality members

		/// <summary>
		///     Compares the Unit-Parts of two SI Units.
		/// </summary>
		[Pure]
		public bool HasEqualUnit(SI si)
		{
			return ToBasicUnits()
				.Denominator.OrderBy(x => x)
				.SequenceEqual(si.ToBasicUnits().Denominator.OrderBy(x => x))
					&&
					ToBasicUnits().Numerator.OrderBy(x => x).SequenceEqual(si.ToBasicUnits().Numerator.OrderBy(x => x));
		}

		protected bool Equals(SI other)
		{
			return Val.Equals(other.Val) && HasEqualUnit(other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			var other = obj as SI;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = Val.GetHashCode();
				hashCode = (hashCode * 397) ^ (Numerator != null ? Numerator.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Denominator != null ? Denominator.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(SI left, SI right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SI left, SI right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}