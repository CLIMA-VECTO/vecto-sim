using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	public class Newton : SIBase<Newton>
	{
		static Newton()
		{
			Constructors.Add(typeof(Newton), val => new Newton(val));
		}

		[JsonConstructor]
		private Newton(double val) : base(new SI(val).Newton) {}

		public static NewtonMeter operator *(Newton newton, Meter meter)
		{
			return ((newton as SI) * meter).Cast<NewtonMeter>();
		}
	}

	public class Radian : SIBase<Radian>
	{
		static Radian()
		{
			Constructors.Add(typeof(Radian), val => new Radian(val));
		}

		[JsonConstructor]
		private Radian(double val) : base(new SI(val).Radian) {}
	}


	public class MeterPerSquareSecond : SIBase<MeterPerSquareSecond>
	{
		static MeterPerSquareSecond()
		{
			Constructors.Add(typeof(MeterPerSquareSecond), val => new MeterPerSquareSecond(val));
		}

		protected MeterPerSquareSecond(double val) : base(new SI(val).Meter.Per.Square.Second) {}
	}

	public class Second : SIBase<Second>
	{
		static Second()
		{
			Constructors.Add(typeof(Second), val => new Second(val));
		}

		[JsonConstructor]
		private Second(double val) : base(new SI(val).Second) {}
	}

	public class Meter : SIBase<Meter>
	{
		static Meter()
		{
			Constructors.Add(typeof(Meter), val => new Meter(val));
		}

		protected Meter(double val) : base(new SI(val).Meter) {}
	}

	public class Ton : SIBase<Ton>
	{
		static Ton()
		{
			Constructors.Add(typeof(Ton), val => new Ton(val));
		}

		[JsonConstructor]
		protected Ton(double val) : base(new SI(val).Kilo.Kilo.Gramm) {}
	}


	public class Kilogram : SIBase<Kilogram>
	{
		static Kilogram()
		{
			Constructors.Add(typeof(Kilogram), val => new Kilogram(val));
		}

		[JsonConstructor]
		protected Kilogram(double val) : base(new SI(val).Kilo.Gramm) {}
	}

	public class SquareMeter : SIBase<SquareMeter>
	{
		static SquareMeter()
		{
			Constructors.Add(typeof(SquareMeter), val => new SquareMeter(val));
		}

		[JsonConstructor]
		private SquareMeter(double val) : base(new SI(val).Square.Meter) {}
	}

	public class KilogramSquareMeter : SIBase<KilogramSquareMeter>
	{
		static KilogramSquareMeter()
		{
			Constructors.Add(typeof(KilogramSquareMeter), val => new KilogramSquareMeter(val));
		}

		[JsonConstructor]
		protected KilogramSquareMeter(double val) : base(new SI(val).Kilo.Gramm.Square.Meter) {}
	}

	public class Watt : SIBase<Watt>
	{
		static Watt()
		{
			Constructors.Add(typeof(Watt), val => new Watt(val));
		}

		[JsonConstructor]
		private Watt(double val) : base(new SI(val).Watt) {}

		public static PerSecond operator /(Watt watt, NewtonMeter newtonMeter)
		{
			return ((watt as SI) / newtonMeter).Cast<PerSecond>();
		}

		public static NewtonMeter operator /(Watt watt, PerSecond perSecond)
		{
			return ((watt as SI) / perSecond).Cast<NewtonMeter>();
		}
	}

	public class PerSecond : SIBase<PerSecond>
	{
		static PerSecond()
		{
			Constructors.Add(typeof(PerSecond), val => new PerSecond(val));
		}

		[JsonConstructor]
		private PerSecond(double val) : base(new SI(val).Per.Second) {}
	}

	public class MeterPerSecond : SIBase<MeterPerSecond>
	{
		static MeterPerSecond()
		{
			Constructors.Add(typeof(MeterPerSecond), val => new MeterPerSecond(val));
		}

		[JsonConstructor]
		private MeterPerSecond(double val) : base(new SI(val).Meter.Per.Second) {}


		public static PerSecond operator /(MeterPerSecond meterPerSecond, Meter meter)
		{
			return ((meterPerSecond as SI) / meter).Cast<PerSecond>();
		}
	}


	public class RoundsPerMinute : SIBase<RoundsPerMinute>
	{
		static RoundsPerMinute()
		{
			Constructors.Add(typeof(RoundsPerMinute), val => new RoundsPerMinute(val));
		}

		[JsonConstructor]
		private RoundsPerMinute(double val) : base(new SI(val).Rounds.Per.Minute) {}
	}


	public class NewtonMeter : SIBase<NewtonMeter>
	{
		static NewtonMeter()
		{
			Constructors.Add(typeof(NewtonMeter), val => new NewtonMeter(val));
		}

		[JsonConstructor]
		private NewtonMeter(double val) : base(new SI(val).Newton.Meter) {}

		public static Watt operator *(NewtonMeter newtonMeter, PerSecond perSecond)
		{
			return ((newtonMeter as SI) * perSecond).Cast<Watt>();
		}

		public static Watt operator *(PerSecond perSecond, NewtonMeter newtonMeter)
		{
			return ((perSecond as SI) * newtonMeter).Cast<Watt>();
		}

		public static Second operator /(NewtonMeter newtonMeter, Watt watt)
		{
			return ((newtonMeter as SI) / watt).Cast<Second>();
		}

		public static Newton operator /(NewtonMeter newtonMeter, Meter meter)
		{
			return ((newtonMeter as SI) / meter).Cast<Newton>();
		}
	}

	public abstract class SIBase<T> : SI where T : SIBase<T>
	{
		protected static Dictionary<Type, Func<double, T>> Constructors =
			new Dictionary<Type, Func<double, T>>();

		public static T Create(double val)
		{
			RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
			return Constructors[typeof(T)](val);
		}

		protected SIBase(Type type, Func<double, T> constructor)
		{
			Constructors[type] = constructor;
		}

		protected SIBase(SI si) : base(si) {}

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


		public static T operator -(SIBase<T> si1)
		{
			return 0 - si1;
		}

		public static T operator -(SIBase<T> si1, SIBase<T> si2)
		{
			return (si1 as SI) - si2;
		}

		public static T operator -(SIBase<T> si1, SI si2)
		{
			return (-1 * si2) + si1;
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

	/// <summary>
	/// Class for Representing SI Units.
	/// </summary>
	[DataContract]
	public class SI : IComparable
	{
		[DataMember] protected readonly Unit[] Denominator;
		[DataMember] protected readonly int Exponent;
		[DataMember] protected readonly Unit[] Numerator;
		[DataMember] protected readonly bool Reciproc;
		[DataMember] protected readonly bool Reverse;
		[DataMember] protected readonly double Val;

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		protected enum Unit
		{
			k,
			s,
			m,
			g,
			W,
			N,
			Percent,
			min,
			c,
			h,

			/// <summary>
			/// Milli
			/// </summary>
			milli
		}

		/// <summary>
		/// Creates a new dimensionless SI Unit.
		/// </summary>
		/// <param name="val"></param>
		public SI(double val = 0.0)
		{
			Val = val;
			Reciproc = false;
			Reverse = false;
			Numerator = new Unit[0];
			Denominator = new Unit[0];
			Exponent = 1;
		}

		protected SI(double val, IEnumerable<Unit> numerator, IEnumerable<Unit> denominator,
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

		protected SI(SI si, double? factor = null, Unit? fromUnit = null, Unit? toUnit = null,
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

		private void UpdateUnit(Unit? fromUnit, Unit? toUnit, ICollection<Unit> units)
		{
			if (Reverse && fromUnit.HasValue) {
				if (units.Contains(fromUnit.Value)) {
					units.Remove(fromUnit.Value);
				} else {
					throw new VectoException(string.Format("Unit missing. Conversion not possible. [{0}] does not contain a [{1}].",
						string.Join(", ", units), fromUnit));
				}
			}

			if (toUnit.HasValue) {
				units.Add(toUnit.Value);
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
		/// Casts the SI Unit to the concrete unit type (if the units allow such an cast).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Cast<T>() where T : SIBase<T>
		{
			var t = SIBase<T>.Create(Val);
			if (!HasEqualUnit(t)) {
				throw new VectoException(string.Format("SI Unit Conversion failed: From {0} to {1}", this, t));
			}
			return t;
		}

		/// <summary>
		/// Converts the derived SI units to the basic units and returns this as a new SI object.
		/// </summary>
		/// <returns></returns>
		public SI ToBasicUnits()
		{
			var numerator = new List<Unit>();
			var denominator = new List<Unit>();
			Numerator.ToList().ForEach(unit => ConvertToBasicUnits(unit, numerator, denominator));
			Denominator.ToList().ForEach(unit => ConvertToBasicUnits(unit, denominator, numerator));
			return new SI(Val, numerator, denominator);
		}


		private static void ConvertToBasicUnits(Unit unit, ICollection<Unit> numerator,
			ICollection<Unit> denominator)
		{
			switch (unit) {
				case Unit.W:
					numerator.Add(Unit.k);
					numerator.Add(Unit.g);
					numerator.Add(Unit.m);
					numerator.Add(Unit.m);
					denominator.Add(Unit.s);
					denominator.Add(Unit.s);
					denominator.Add(Unit.s);
					break;
				case Unit.N:
					numerator.Add(Unit.k);
					numerator.Add(Unit.g);
					numerator.Add(Unit.m);
					denominator.Add(Unit.s);
					denominator.Add(Unit.s);
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

		/// <summary>
		/// Returns the absolute value.
		/// </summary>
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
			get { return new SI(new SI(this, toUnit: Unit.k), 0.001, Unit.g, Unit.g); }
		}


		/// <summary>
		///     [N]
		/// </summary>
		[DebuggerHidden]
		public SI Newton
		{
			get { return new SI(this, fromUnit: Unit.N, toUnit: Unit.N); }
		}

		/// <summary>
		///     [W]
		/// </summary>
		[DebuggerHidden]
		public SI Watt
		{
			get { return new SI(this, fromUnit: Unit.W, toUnit: Unit.W); }
		}

		/// <summary>
		///     [m]
		/// </summary>
		[DebuggerHidden]
		public SI Meter
		{
			get { return new SI(this, fromUnit: Unit.m, toUnit: Unit.m); }
		}

		/// <summary>
		///     [s]
		/// </summary>
		[DebuggerHidden]
		public SI Second
		{
			get { return new SI(this, fromUnit: Unit.s, toUnit: Unit.s); }
		}

		/// <summary>
		///     [rad]
		/// </summary>
		[DebuggerHidden]
		public SI Radian
		{
			get { return new SI(this); }
		}

		public SI GradientPercent
		{
			get { return new SI(this, factor: Math.Atan(Val) / Val, fromUnit: Unit.Percent); }
		}

		/// <summary>
		///     Converts to/from Radiant
		/// </summary>
		[DebuggerHidden]
		public SI Rounds
		{
			get { return new SI(this, 2 * Math.PI); }
		}

		/// <summary>
		///     Converts to/from Second
		/// </summary>
		[DebuggerHidden]
		public SI Hour
		{
			get { return new SI(this, 3600.0, Unit.h, Unit.s); }
		}

		/// <summary>
		///     Converts to/from Second
		/// </summary>
		[DebuggerHidden]
		public SI Minute
		{
			get { return new SI(this, 60.0, Unit.min, Unit.s); }
		}

		public SI Milli
		{
			get { return new SI(this, 0.001, Unit.milli); }
		}

		/// <summary>
		///     Converts to/from 1000 * Basic Unit
		/// </summary>
		[DebuggerHidden]
		public SI Kilo
		{
			get { return new SI(this, 1000.0, Unit.k); }
		}

		/// <summary>
		///     Converts to/from Basic Unit / 100
		/// </summary>
		[DebuggerHidden]
		public SI Centi
		{
			get { return new SI(this, 1.0 / 100.0, Unit.c); }
		}

		#endregion

		#region Operators

		public static SI operator +(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '+' can only operate on SI Objects with the same unit. Got: {0} + {1}", si1, si2));
			}

			return new SI(si1.Val + si2.Val, si1.Numerator, si1.Denominator);
		}

		public static SI operator -(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '-' can only operate on SI Objects with the same unit. Got: {0} + {1}", si1, si2));
			}
			return new SI(si1.Val - si2.Val, si1.Numerator, si1.Denominator);
		}

		public static SI operator *(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			var numerator = si1.Numerator.Concat(si2.Numerator);
			var denominator = si1.Denominator.Concat(si2.Denominator);
			return new SI(si1.Val * si2.Val, numerator, denominator);
		}

		public static SI operator /(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			var numerator = si1.Numerator.Concat(si2.Denominator);
			var denominator = si1.Denominator.Concat(si2.Numerator);
			return new SI(si1.Val / si2.Val, numerator, denominator);
		}

		public static SI operator +(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return new SI(si1.Val + d, si1);
		}

		public static SI operator +(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return si1 + d;
		}

		public static SI operator -(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return new SI(si1.Val - d, si1);
		}

		public static SI operator -(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return new SI(d - si1.Val, si1);
		}

		public static SI operator -(SI si1)
		{
			Contract.Requires(si1 != null);
			return 0 - si1;
		}

		public static SI operator *(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return new SI(si1.Val * d, si1);
		}

		public static SI operator *(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return new SI(d * si1.Val, si1);
		}

		public static SI operator /(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return new SI(si1.Val / d, si1);
		}

		public static SI operator /(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return new SI(d / si1.Val, si1);
		}

		public static bool operator <(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '<' can only operate on SI Objects with the same unit. Got: {0} + {1}", si1, si2));
			}
			return si1.Val < si2.Val;
		}

		public static bool operator >(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '>' can only operate on SI Objects with the same unit. Got: {0} + {1}", si1, si2));
			}
			return si1.Val > si2.Val;
		}

		public static bool operator <=(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '<=' can only operate on SI Objects with the same unit. Got: {0} + {1}", si1, si2));
			}
			return si1.Val <= si2.Val;
		}

		public static bool operator >=(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '>=' can only operate on SI Objects with the same unit. Got: {0} + {1}", si1, si2));
			}
			return si1.Val >= si2.Val;
		}

		public static bool operator <(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1.Val < d;
		}

		public static bool operator >(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1.Val > d;
		}

		public static bool operator <=(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1.Val <= d;
		}

		public static bool operator >=(SI si1, double d)
		{
			Contract.Requires(si1 != null);
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
			return ToString(null);
		}

		public virtual string ToString(string format)
		{
			if (string.IsNullOrEmpty(format)) {
				format = "";
			}

			return string.Format("{0:" + format + "} [{2}]", Val, format, GetUnitString());
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Compares the Unit-Parts of two SI Units.
		/// </summary>
		public bool HasEqualUnit(SI si)
		{
			Contract.Requires(si != null);
			return ToBasicUnits()
				.Denominator.OrderBy(x => x)
				.SequenceEqual(si.ToBasicUnits().Denominator.OrderBy(x => x))
					&&
					ToBasicUnits().Numerator.OrderBy(x => x).SequenceEqual(si.ToBasicUnits().Numerator.OrderBy(x => x));
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
			return other != null && Val.Equals(other.Val) && HasEqualUnit(other);
		}

		public override int GetHashCode()
		{
			unchecked {
				// ReSharper disable once NonReadonlyMemberInGetHashCode
				var hashCode = Val.GetHashCode();
				hashCode = (hashCode * 397) ^ (Numerator != null ? Numerator.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Denominator != null ? Denominator.GetHashCode() : 0);
				return hashCode;
			}
		}

		public int CompareTo(object obj)
		{
			var si = (obj as SI);
			if (si == null) {
				return 1;
			}

			if (!HasEqualUnit(si)) {
				if (si.Numerator.Length + si.Denominator.Length >= Numerator.Length + Denominator.Length) {
					return -1;
				}
				return 1;
			}

			if (this > si) {
				return 1;
			}
			return this < si ? -1 : 0;
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