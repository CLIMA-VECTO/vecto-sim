using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	/// <summary>
	/// SI Class for Scalar Values. Converts implicitely to double and is only castable if the SI value has no units.
	/// </summary>
	public class Scalar : SIBase<Scalar>
	{
		static Scalar()
		{
			Register(val => new Scalar(val));
		}

		private Scalar(double val) : base(new SI(val)) {}

		public static implicit operator double(Scalar self)
		{
			return self.Val;
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static Scalar operator +(Scalar si1, Scalar si2)
		{
			return new Scalar(si1.Val + si2.Val);
		}

		[DebuggerHidden]
		public static Scalar operator +(Scalar si1, double si2)
		{
			return new Scalar(si1.Val + si2);
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static Scalar operator +(double si1, Scalar si2)
		{
			return new Scalar(si1 + si2.Val);
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static Scalar operator -(Scalar si1, Scalar si2)
		{
			return new Scalar(si1.Val - si2.Val);
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static Scalar operator -(Scalar si1, double si2)
		{
			return new Scalar(si1.Val - si2);
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static Scalar operator -(double si1, Scalar si2)
		{
			return new Scalar(si1 - si2.Val);
		}
	}

	/// <summary>
	/// SI Class for Newton [N].
	/// </summary>
	public class Newton : SIBase<Newton>
	{
		static Newton()
		{
			Register(val => new Newton(val));
		}

		[JsonConstructor]
		private Newton(double val) : base(new SI(val).Newton) {}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="newton">The newton.</param>
		/// <param name="meter">The meter.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static NewtonMeter operator *(Newton newton, Meter meter)
		{
			return ((newton as SI) * meter).Cast<NewtonMeter>();
		}
	}

	/// <summary>
	/// SI Class for Radian [] (rad).
	/// </summary>
	public class Radian : SIBase<Radian>
	{
		static Radian()
		{
			Register(val => new Radian(val));
		}

		[JsonConstructor]
		private Radian(double val) : base(new SI(val).Radian) {}
	}

	/// <summary>
	/// SI Class for Meter per square second [m/s²].
	/// </summary>
	public class MeterPerSquareSecond : SIBase<MeterPerSquareSecond>
	{
		static MeterPerSquareSecond()
		{
			Register(val => new MeterPerSquareSecond(val));
		}

		protected MeterPerSquareSecond(double val) : base(new SI(val).Meter.Per.Square.Second) {}
	}

	/// <summary>
	/// SI Class for Second [s].
	/// </summary>
	public class Second : SIBase<Second>
	{
		static Second()
		{
			Register(val => new Second(val));
		}

		[JsonConstructor]
		private Second(double val) : base(new SI(val).Second) {}
	}

	/// <summary>
	/// SI Class for Meter [m].
	/// </summary>
	public class Meter : SIBase<Meter>
	{
		static Meter()
		{
			Register(val => new Meter(val));
		}

		protected Meter(double val) : base(new SI(val).Meter) {}
	}

	/// <summary>
	/// SI Class for Kilogram [kg].
	/// </summary>
	public class Kilogram : SIBase<Kilogram>
	{
		static Kilogram()
		{
			Register(val => new Kilogram(val));
		}

		[JsonConstructor]
		protected Kilogram(double val) : base(new SI(val).Kilo.Gramm) {}
	}

	/// <summary>
	/// SI Class for Ton [t] (automatically converts to [kg])
	/// </summary>
	public class Ton : SIBase<Ton>
	{
		static Ton()
		{
			Register(val => new Ton(val));
		}

		[JsonConstructor]
		protected Ton(double val) : base(new SI(val).Ton) {}
	}

	/// <summary>
	/// SI Class for Square meter [m²].
	/// </summary>
	public class SquareMeter : SIBase<SquareMeter>
	{
		static SquareMeter()
		{
			Register(val => new SquareMeter(val));
		}

		[JsonConstructor]
		private SquareMeter(double val) : base(new SI(val).Square.Meter) {}
	}

	/// <summary>
	/// SI Class for cubic meter [m³].
	/// </summary>
	public class CubicMeter : SIBase<CubicMeter>
	{
		static CubicMeter()
		{
			Register(val => new CubicMeter(val));
		}

		[JsonConstructor]
		private CubicMeter(double val) : base(new SI(val).Cubic.Meter) {}
	}

	/// <summary>
	/// SI Class for Kilogram Square Meter [kgm²].
	/// </summary>
	public class KilogramSquareMeter : SIBase<KilogramSquareMeter>
	{
		static KilogramSquareMeter()
		{
			Register(val => new KilogramSquareMeter(val));
		}

		[JsonConstructor]
		protected KilogramSquareMeter(double val) : base(new SI(val).Kilo.Gramm.Square.Meter) {}
	}

	/// <summary>
	/// SI Class for Kilogramm per watt second [kg/ws].
	/// </summary>
	public class KilogramPerWattSecond : SIBase<KilogramPerWattSecond>
	{
		static KilogramPerWattSecond()
		{
			Register(val => new KilogramPerWattSecond(val));
		}

		[JsonConstructor]
		protected KilogramPerWattSecond(double val) : base(new SI(val).Kilo.Gramm.Per.Watt.Second) {}
	}

	/// <summary>
	/// SI Class for Watt [W].
	/// </summary>
	public class Watt : SIBase<Watt>
	{
		static Watt()
		{
			Register(val => new Watt(val));
		}

		[JsonConstructor]
		private Watt(double val) : base(new SI(val).Watt) {}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="watt">The watt.</param>
		/// <param name="newtonMeter">The newton meter.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static PerSecond operator /(Watt watt, NewtonMeter newtonMeter)
		{
			return ((watt as SI) / newtonMeter).Cast<PerSecond>();
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="watt">The watt.</param>
		/// <param name="perSecond">The per second.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static NewtonMeter operator /(Watt watt, PerSecond perSecond)
		{
			return ((watt as SI) / perSecond).Cast<NewtonMeter>();
		}
	}

	/// <summary>
	/// SI Class for one per second [1/s].
	/// </summary>
	public class PerSecond : SIBase<PerSecond>
	{
		static PerSecond()
		{
			Register(val => new PerSecond(val));
		}

		[JsonConstructor]
		private PerSecond(double val) : base(new SI(val).Per.Second) {}

		public SI RoundsPerMinute
		{
			get { return ConvertTo().Rounds.Per.Minute; }
		}
	}

	/// <summary>
	/// SI Class for Meter per second [m/s].
	/// </summary>
	public class MeterPerSecond : SIBase<MeterPerSecond>
	{
		static MeterPerSecond()
		{
			Register(val => new MeterPerSecond(val));
		}

		[JsonConstructor]
		private MeterPerSecond(double val) : base(new SI(val).Meter.Per.Second) {}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="meterPerSecond">The meter per second.</param>
		/// <param name="meter">The meter.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static PerSecond operator /(MeterPerSecond meterPerSecond, Meter meter)
		{
			return ((meterPerSecond as SI) / meter).Cast<PerSecond>();
		}
	}

	/// <summary>
	/// SI Class for Rounds per minute [rpm] (automatically converts internally to radian per second)
	/// </summary>
	public class RoundsPerMinute : SIBase<RoundsPerMinute>
	{
		static RoundsPerMinute()
		{
			Register(val => new RoundsPerMinute(val));
		}

		[JsonConstructor]
		private RoundsPerMinute(double val) : base(new SI(val).Rounds.Per.Minute) {}
	}

	/// <summary>
	/// SI Class for NewtonMeter [Nm].
	/// </summary>
	public class NewtonMeter : SIBase<NewtonMeter>
	{
		static NewtonMeter()
		{
			Register(val => new NewtonMeter(val));
		}

		[JsonConstructor]
		private NewtonMeter(double val) : base(new SI(val).Newton.Meter) {}

		[DebuggerHidden]
		public static Watt operator *(NewtonMeter newtonMeter, PerSecond perSecond)
		{
			return ((newtonMeter as SI) * perSecond).Cast<Watt>();
		}

		[DebuggerHidden]
		public static Watt operator *(PerSecond perSecond, NewtonMeter newtonMeter)
		{
			return ((perSecond as SI) * newtonMeter).Cast<Watt>();
		}

		[DebuggerHidden]
		public static Second operator /(NewtonMeter newtonMeter, Watt watt)
		{
			return ((newtonMeter as SI) / watt).Cast<Second>();
		}

		[DebuggerHidden]
		public static Newton operator /(NewtonMeter newtonMeter, Meter meter)
		{
			return ((newtonMeter as SI) / meter).Cast<Newton>();
		}
	}


	/// <summary>
	/// Base Class for all special SI Classes. Not intended to be used directly.
	/// Implements templated operators for type safety and convenience.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SIBase<T> : SI where T : SIBase<T>
	{
		/// <summary>
		/// Static dictionary with constructors for the specialized types.
		/// Every specialized SI type needs to Register itself in a static constructor (with the method <see cref="Register"/>).
		/// </summary>
		private static readonly Dictionary<Type, Func<double, T>> Constructors = new Dictionary<Type, Func<double, T>>();


		/// <summary>
		/// Creates the specified special SI object.
		/// </summary>
		/// <param name="val">The value of the SI object.</param>
		public static T Create(double val)
		{
			RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
			return Constructors[typeof(T)](val);
		}

		/// <summary>
		/// Registers the specified constructor in the constructor list (which is used for the <see cref="Create"/> Method).
		/// </summary>
		/// <param name="func">The constructor of the specified type T.</param>
		protected static void Register(Func<double, T> func)
		{
			Constructors[typeof(T)] = func;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SIBase{T}"/> class. Is used by specialized sub classes.
		/// </summary>
		protected SIBase(SI si) : base(si) {}

		#region Operators

		/// <summary>
		/// Implements the operator + for two specialized SI Classes.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator +(SIBase<T> si1, SIBase<T> si2)
		{
			return (si1 as SI) + si2;
		}

		/// <summary>
		/// Implements the operator + for a specialized SI Class and a generic SI Class.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator +(SIBase<T> si1, SI si2)
		{
			return ((si1 as SI) + si2).Cast<T>();
		}

		/// <summary>
		/// Implements the operator + for a generic SI Class and a specialized SI Class.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator +(SI si1, SIBase<T> si2)
		{
			return (si1 + (si2 as SI)).Cast<T>();
		}

		/// <summary>
		/// Implements the unary operator -.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator -(SIBase<T> si1)
		{
			return (-(si1 as SI)).Cast<T>();
		}

		/// <summary>
		/// Implements the operator - for two specialized SI classes.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator -(SIBase<T> si1, SIBase<T> si2)
		{
			return ((si1 as SI) - (si2 as SI)).Cast<T>();
		}

		/// <summary>
		/// Implements the operator - for a specialized SI class and a generic SI class.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator -(SIBase<T> si1, SI si2)
		{
			return ((si1 as SI) - si2).Cast<T>();
		}

		/// <summary>
		/// Implements the operator - for a generic SI class and a specialized SI class.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator -(SI si1, SIBase<T> si2)
		{
			return (si1 - (si2 as SI)).Cast<T>();
		}

		/// <summary>
		/// Implements the operator * for a double and a specialized SI class.
		/// </summary>
		/// <param name="d">The double value.</param>
		/// <param name="si">The si.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator *(double d, SIBase<T> si)
		{
			return (d * (si as SI)).Cast<T>();
		}

		/// <summary>
		/// Implements the operator * for a specialized SI class and a double.
		/// </summary>
		/// <param name="si">The si.</param>
		/// <param name="d">The double.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator *(SIBase<T> si, double d)
		{
			return ((si as SI) * d).Cast<T>();
		}

		/// <summary>
		/// Implements the operator / for a specialized SI class and a double.
		/// </summary>
		/// <param name="si">The si.</param>
		/// <param name="d">The double.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static T operator /(SIBase<T> si, double d)
		{
			return ((si as SI) / d).Cast<T>();
		}

		#endregion
	}

	/// <summary>
	/// Class for representing generic SI Units.
	/// </summary>
	/// <remarks>
	/// Usage: new SI(1.0).Newton.Meter, new SI(2.3).Rounds.Per.Minute
	/// </remarks>
	[DataContract]
	public class SI : IComparable
	{
		/// <summary>
		/// The basic scalar value of the SI.
		/// </summary>
		[DataMember] protected readonly double Val;

		/// <summary>
		/// The denominator of the SI.
		/// </summary>
		[DataMember] protected readonly Unit[] Denominator;

		/// <summary>
		/// The numerator of the SI.
		/// </summary>
		[DataMember] protected readonly Unit[] Numerator;

		/// <summary>
		/// The current exponent for conversion operations (Square, Cubic, Linear, e.g. new SI(3).Square.Meter).
		/// Can be reseted with Reset, Per, Cast.
		/// </summary>
		[DataMember] protected readonly int Exponent;

		/// <summary>
		/// A flag indicating if the current SI is in reciprocal mode (used in the <see cref="Per"/> method for reciprocal units: e.g. new SI(2).Meter.Per.Second) ==> [m/s]
		/// Can be reseted with Reset, Per, Cast.
		/// </summary>
		[DataMember] protected readonly bool Reciproc;

		/// <summary>
		/// A flag indicating if the current SI is in reverse mode (used for conversions: e.g. new SI(2).Rounds.Per.Minute.ConverTo.Radian.Per.Second ==> [rpm/min] => [rad/s]).
		/// </summary>
		[DataMember] protected readonly bool Reverse;


		/// <summary>
		/// Enum for defining the Units.
		/// </summary>
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
			milli,
			t
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SI"/> class without any units (dimensionless, scalar) [-].
		/// </summary>
		/// <param name="val">The value.</param>
		public SI(double val = 0.0)
		{
			Val = val;
			Reciproc = false;
			Reverse = false;
			Numerator = new Unit[0];
			Denominator = new Unit[0];
			Exponent = 1;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SI"/> class which allows to construct a new SI with all parameters.
		/// </summary>
		/// <param name="val">The value.</param>
		/// <param name="numerator">The numerator.</param>
		/// <param name="denominator">The denominator.</param>
		/// <param name="reciproc">if set to <c>true</c> then the object is in reciproc mode (1/...)</param>
		/// <param name="reverse">if set to <c>true</c> then the object is in reverse convertion mode (e.g. rpm/min => rad/s).</param>
		/// <param name="exponent">The exponent for further conversions (e.g. Square.Meter).</param>
		protected SI(double val, IEnumerable<Unit> numerator, IEnumerable<Unit> denominator, bool reciproc = false,
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

		/// <summary>
		/// Initializes a new instance of the <see cref="SI"/> class which copies the units from an already existing SI.
		/// </summary>
		/// <param name="val">The value.</param>
		/// <param name="unit">The unit.</param>
		protected SI(double val, SI unit) : this(val, unit.Numerator, unit.Denominator) {}

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

			// if reverse mode then swap fromUnit and toUnit and invert factor.
			if (Reverse) {
				var tmp = fromUnit;
				fromUnit = toUnit;
				toUnit = tmp;
				factor = 1 / factor;
			}

			// add the unit as often as is defined by the exponent.
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

			Numerator = denominator.ToArray();
			Denominator = numerator.ToArray();
		}

		/// <summary>
		/// Adds the new toUnit to the units collection and removes the fromUnit.
		/// </summary>
		/// <param name="fromUnit">From unit.</param>
		/// <param name="toUnit">To unit.</param>
		/// <param name="units">The units.</param>
		/// <exception cref="VectoException"></exception>
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
		/// ATTENTION: Before returning an SI Unit, ensure to cancel Conversion Mode (with or Cast).
		/// </summary>
		/// <returns></returns>
		public SI ConvertTo()
		{
			return new SI(Linear, reciproc: false, reverse: true);
		}

		/// <summary>
		/// Casts the SI Unit to the concrete unit type (if the units allow such an cast).
		/// </summary>
		/// <typeparam name="T">the specialized SI unit. e.g. Watt, NewtonMeter, Second</typeparam>
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
		public SI ToBasicUnits()
		{
			var numerator = new List<Unit>();
			var denominator = new List<Unit>();
			var numeratorFactor = 1.0;
			Numerator.ToList().ForEach(unit => ConvertToBasicUnits(unit, numerator, denominator, ref numeratorFactor));
			var denominatorFactor = 1.0;
			Denominator.ToList().ForEach(unit => ConvertToBasicUnits(unit, denominator, numerator, ref denominatorFactor));
			return new SI(Val * numeratorFactor / denominatorFactor, numerator, denominator);
		}


		/// <summary>
		/// Converts to basic units. e.g [W] => [kgm²/s³]
		/// </summary>
		/// <param name="unit">The unit.</param>
		/// <param name="numerator">The numerator.</param>
		/// <param name="denominator">The denominator.</param>
		/// <param name="factor">The factor.</param>
		private static void ConvertToBasicUnits(Unit unit, ICollection<Unit> numerator,
			ICollection<Unit> denominator, ref double factor)
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
				case Unit.t:
					factor *= 1000;
					numerator.Add(Unit.k);
					numerator.Add(Unit.g);
					break;
				case Unit.min:
					factor *= 60;
					numerator.Add(Unit.s);
					break;
				default:
					numerator.Add(unit);
					break;
			}
		}

		/// <summary>
		/// Gets the underlying scalar double value.
		/// </summary>
		public double Value()
		{
			return Val;
		}


		/// <summary>
		/// Clones this instance.
		/// </summary>
		public SI Clone()
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

		/// <summary>
		/// Returns the Square root of value and units of the SI.
		/// </summary>
		public SI Sqrt()
		{
			var si = ToBasicUnits();
			if (si.Numerator.Length % 2 != 0 || si.Denominator.Length % 2 != 0) {
				throw new VectoException(
					string.Format("The squareroot cannot be calculated because the Unit-Exponents are not even: [{0}]",
						si.GetUnitString()));
			}

			var numerator = new List<Unit>();
			var currentNumerator = si.Numerator.ToList();
			while (currentNumerator.Count != 0) {
				var unit = currentNumerator.First();
				currentNumerator.Remove(unit);
				currentNumerator.Remove(unit);
				numerator.Add(unit);
			}

			var denominator = new List<Unit>();
			var currentDenominator = si.Denominator.ToList();
			while (currentDenominator.Count != 0) {
				var unit = currentDenominator.First();
				currentDenominator.Remove(unit);
				currentDenominator.Remove(unit);
				denominator.Add(unit);
			}

			return new SI(Math.Sqrt(si.Val), numerator, denominator);
		}

		#region Unit Definitions

		/// <summary>
		/// Defines the denominator by the terms following after the Per.
		/// </summary>
		[DebuggerHidden]
		public SI Per
		{
			get { return new SI(Linear, reciproc: !Reciproc); }
		}

		/// <summary>
		/// Takes all following terms as cubic terms (=to the power of 3).
		/// </summary>
		[DebuggerHidden]
		public SI Cubic
		{
			get { return new SI(this, exponent: 3); }
		}

		/// <summary>
		/// Takes all following terms as quadratic terms (=to the power of 2).
		/// </summary>
		[DebuggerHidden]
		public SI Square
		{
			get { return new SI(this, exponent: 2); }
		}

		/// <summary>
		/// Takes all following terms as linear terms (=to the power of 1).
		/// </summary>
		[DebuggerHidden]
		public SI Linear
		{
			get { return new SI(this, exponent: 1); }
		}

		/// <summary>
		/// [g] (to basic unit: [kg])
		/// </summary>
		[DebuggerHidden]
		public SI Gramm
		{
			get { return new SI(new SI(this, toUnit: Unit.k), 0.001, Unit.g, Unit.g); }
		}

		/// <summary>
		/// [t] (to basic unit: [kg])
		/// </summary>
		[DebuggerHidden]
		public SI Ton
		{
			get { return new SI(new SI(this, toUnit: Unit.k), 1000, Unit.t, Unit.g); }
		}


		/// <summary>
		/// [N]
		/// </summary>
		[DebuggerHidden]
		public SI Newton
		{
			get { return new SI(this, fromUnit: Unit.N, toUnit: Unit.N); }
		}

		/// <summary>
		/// [W]
		/// </summary>
		[DebuggerHidden]
		public SI Watt
		{
			get { return new SI(this, fromUnit: Unit.W, toUnit: Unit.W); }
		}

		/// <summary>
		/// [m]
		/// </summary>
		[DebuggerHidden]
		public SI Meter
		{
			get { return new SI(this, fromUnit: Unit.m, toUnit: Unit.m); }
		}

		/// <summary>
		/// [s]
		/// </summary>
		[DebuggerHidden]
		public SI Second
		{
			get { return new SI(this, fromUnit: Unit.s, toUnit: Unit.s); }
		}

		/// <summary>
		/// [-]. Defines radian. Only virtual. Has no real SI unit.
		/// </summary>
		[DebuggerHidden]
		public SI Radian
		{
			get { return new SI(this); }
		}


		/// <summary>
		/// [-]. Converts to/from Radiant. Internally everything is stored in radian.
		/// </summary>
		[DebuggerHidden]
		public SI Rounds
		{
			get { return new SI(this, 2 * Math.PI); }
		}

		/// <summary>
		/// [s] Converts to/from Second. Internally everything is stored in seconds.
		/// </summary>
		[DebuggerHidden]
		public SI Hour
		{
			get { return new SI(this, 3600.0, Unit.h, Unit.s); }
		}

		/// <summary>
		/// [s] Converts to/from Second. Internally everything is stored in seconds.
		/// </summary>
		[DebuggerHidden]
		public SI Minute
		{
			get { return new SI(this, 60.0, Unit.min, Unit.s); }
		}

		/// <summary>
		/// Quantifier for milli (1/1000).
		/// </summary>
		[DebuggerHidden]
		public SI Milli
		{
			get { return new SI(this, 0.001, Unit.milli); }
		}

		/// <summary>
		/// Quantifier for Kilo (1000).
		/// </summary>
		[DebuggerHidden]
		public SI Kilo
		{
			get { return new SI(this, 1000.0, Unit.k); }
		}

		/// <summary>
		/// Quantifier for Centi (1/100)
		/// </summary>
		[DebuggerHidden]
		public SI Centi
		{
			get { return new SI(this, 0.01, Unit.c); }
		}

		#endregion

		#region Operators

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		/// <exception cref="VectoException"></exception>
		[DebuggerHidden]
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

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		/// <exception cref="VectoException"></exception>
		[DebuggerHidden]
		public static SI operator -(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '-' can only operate on SI Objects with the same unit. Got: {0} - {1}", si1, si2));
			}
			return new SI(si1.Val - si2.Val, si1.Numerator, si1.Denominator);
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator -(SI si1)
		{
			Contract.Requires(si1 != null);
			return new SI(-si1.Val, si1);
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator *(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			var numerator = si1.Numerator.Concat(si2.Numerator);
			var denominator = si1.Denominator.Concat(si2.Denominator);
			return new SI(si1.Val * si2.Val, numerator, denominator);
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="d">The d.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator *(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return new SI(si1.Val * d, si1);
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="si1">The si1.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator *(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return new SI(d * si1.Val, si1);
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator /(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			var numerator = si1.Numerator.Concat(si2.Denominator);
			var denominator = si1.Denominator.Concat(si2.Numerator);
			return new SI(si1.Val / si2.Val, numerator, denominator);
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="d">The d.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator /(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return new SI(si1.Val / d, si1);
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="si1">The si1.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static SI operator /(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return new SI(d / si1.Val, si1.Denominator, si1.Numerator);
		}

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		/// <exception cref="VectoException"></exception>
		[DebuggerHidden]
		public static bool operator <(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '<' can only operate on SI Objects with the same unit. Got: {0} < {1}", si1, si2));
			}
			return si1.Val < si2.Val;
		}

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="d">The d.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator <(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1 != null && si1.Val < d;
		}

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		/// <exception cref="VectoException"></exception>
		[DebuggerHidden]
		public static bool operator >(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '>' can only operate on SI Objects with the same unit. Got: {0} > {1}", si1, si2));
			}
			return si1.Val > si2.Val;
		}

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="d">The d.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator >(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1 != null && si1.Val > d;
		}

		/// <summary>
		/// Implements the operator &gt;.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="si1">The si1.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator >(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return si1 != null && d > si1.Val;
		}

		/// <summary>
		/// Implements the operator &lt;.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="si1">The si1.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator <(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return si1 != null && d < si1.Val;
		}

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		/// <exception cref="VectoException"></exception>
		[DebuggerHidden]
		public static bool operator <=(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '<=' can only operate on SI Objects with the same unit. Got: {0} <= {1}", si1, si2));
			}
			return si1.Val <= si2.Val;
		}

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="d">The d.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator <=(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1 != null && si1.Val <= d;
		}

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="si2">The si2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		/// <exception cref="VectoException"></exception>
		[DebuggerHidden]
		public static bool operator >=(SI si1, SI si2)
		{
			Contract.Requires(si1 != null);
			Contract.Requires(si2 != null);
			if (!si1.HasEqualUnit(si2)) {
				throw new VectoException(
					string.Format("Operator '>=' can only operate on SI Objects with the same unit. Got: {0} >= {1}", si1, si2));
			}
			return si1.Val >= si2.Val;
		}

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="si1">The si1.</param>
		/// <param name="d">The d.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator >=(SI si1, double d)
		{
			Contract.Requires(si1 != null);
			return si1 != null && si1.Val >= d;
		}

		/// <summary>
		/// Implements the operator &gt;=.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="si1">The lower.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[DebuggerHidden]
		public static bool operator >=(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return si1 != null && d >= si1.Val;
		}

		/// <summary>
		/// Implements the operator &lt;=.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="si1">The lower.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator <=(double d, SI si1)
		{
			Contract.Requires(si1 != null);
			return si1 != null && d <= si1.Val;
		}

		/// <summary>
		/// Determines whether the SI is between lower and uppper bound.
		/// </summary>
		/// <param name="lower">The lower bound.</param>
		/// <param name="upper">The upper bound.</param>
		/// <returns></returns>
		public bool IsBetween(SI lower, SI upper)
		{
			return lower <= Val && Val <= upper;
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

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public virtual string ToString(string format)
		{
			if (string.IsNullOrEmpty(format)) {
				format = "F4";
			}

			return string.Format(CultureInfo.InvariantCulture, "{0:" + format + "} [{2}]", Val, format, GetUnitString());
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Compares the Unit-Parts of two SI Units.
		/// </summary>
		/// <param name="si">The si.</param>
		/// <returns></returns>
		public bool HasEqualUnit(SI si)
		{
			Contract.Requires(si != null);
			return ToBasicUnits()
				.Denominator.OrderBy(x => x)
				.SequenceEqual(si.ToBasicUnits().Denominator.OrderBy(x => x))
					&&
					ToBasicUnits().Numerator.OrderBy(x => x).SequenceEqual(si.ToBasicUnits().Numerator.OrderBy(x => x));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
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

		/// <summary>
		/// Determines whether the specified si is equal.
		/// </summary>
		/// <param name="si">The si.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public bool IsEqual(SI si, double tolerance = DoubleExtensionMethods.Tolerance)
		{
			return HasEqualUnit(si) && Val.IsEqual(si.Val, tolerance);
		}

		/// <summary>
		/// Determines whether the specified value is equal.
		/// </summary>
		/// <param name="val">The value.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public bool IsEqual(double val, double tolerance = DoubleExtensionMethods.Tolerance)
		{
			return Val.IsEqual(val, tolerance);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
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

		/// <summary>
		/// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
		/// </returns>
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

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(SI left, SI right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(SI left, SI right)
		{
			return !Equals(left, right);
		}

		#endregion

		/// <summary>
		/// Convert the SI to a string in the wished output format.
		/// </summary>
		/// <param name="decimals">The decimals.</param>
		/// <param name="outputFactor">The output factor.</param>
		/// <param name="showUnit">The show unit.</param>
		/// <returns></returns>
		public virtual string ToOutputFormat(uint? decimals = null, double? outputFactor = null, bool? showUnit = null)
		{
			decimals = decimals ?? 4;
			outputFactor = outputFactor ?? 1.0;
			showUnit = showUnit ?? false;

			var format = string.Format("{{0:F{0}}}" + (showUnit.Value ? " [{{1}}]" : ""), decimals);
			return string.Format(CultureInfo.InvariantCulture, format, Val * outputFactor, GetUnitString());
		}
	}
}