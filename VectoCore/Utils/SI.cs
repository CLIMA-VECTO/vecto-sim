using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    public class MeterPerSecond : SI
    {
        public MeterPerSecond(double value = 0) : base(value, new SI().Meter.Per.Second) { }
    }

    public class Second : SI
    {
        public Second(double value = 0) : base(value, new SI().Second) { }
    }

    public class Watt : SI
    {
        public Watt(double value = 0) : base(value, new SI().Watt) { }
    }

    public class RadianPerSecond : SI
    {
        public RadianPerSecond(double value = 0) : base(value, new SI().Radian.Per.Second) { }
    }

    public class NewtonMeter : SI
    {
        public NewtonMeter(double value = 0) : base(value, new SI().Newton.Meter) { }
    }

    [DataContract]
    public class SI
    {
        [DataMember]
        protected readonly double _value;

        [DataMember]
        protected readonly string[] _numerator;

        [DataMember]
        protected readonly string[] _denominator;

        [DataMember]
        protected readonly bool _reciproc;

        [DataMember]
        protected readonly bool _reverse;

        [DataMember]
        protected readonly int _exponent;

        public SI(double value = 0.0)
        {
            _value = value;
            _reciproc = false;
            _reverse = false;
            _numerator = new string[0];
            _denominator = new string[0];
            _exponent = 1;
        }

        protected SI(double value, IEnumerable<string> numerator, IEnumerable<string> denominator, bool reciproc = false, bool reverse = false, int exponent = 1)
        {
            Contract.Requires(numerator != null);
            Contract.Requires(denominator != null);

            _value = value;
            _reciproc = reciproc;
            _reverse = reverse;
            _exponent = exponent;

            var tmpNumerator = numerator.ToList();
            var tmpDenominator = denominator.ToList();

            foreach (var v in tmpDenominator.ToArray().Where(v => tmpNumerator.Contains(v)))
            {
                tmpNumerator.Remove(v);
                tmpDenominator.Remove(v);
            }

            _numerator = tmpNumerator.ToArray();
            _denominator = tmpDenominator.ToArray();
        }

        protected SI(double value, SI unit)
            : this(value, unit._numerator, unit._denominator)
        {

        }

        protected SI(SI si, double? factor = null, string fromUnit = null, string toUnit = null,
            bool? reciproc = null, bool? reverse = null, int? exponent = null)
        {
            Contract.Requires(si != null);
            Contract.Requires(si._denominator != null);
            Contract.Requires(si._numerator != null);

            var numerator = si._denominator.ToList();
            var denominator = si._numerator.ToList();

            _value = si._value;
            _reciproc = reciproc ?? si._reciproc;
            _reverse = reverse ?? si._reverse;
            _exponent = exponent ?? si._exponent;

            if (_reverse)
            {
                var tmp = fromUnit;
                fromUnit = toUnit;
                toUnit = tmp;
                factor = 1 / factor;
            }

            for (var i = 0; i < _exponent; i++)
            {
                if (!_reciproc)
                {
                    if (_reverse && !string.IsNullOrEmpty(fromUnit))
                        if (denominator.Contains(fromUnit))
                            denominator.Remove(fromUnit);
                        else
                            throw new VectoException("Unit missing. Conversion not possible.");

                    if (!string.IsNullOrEmpty(toUnit))
                        denominator.Add(toUnit);

                    if (factor.HasValue)
                        _value *= factor.Value;
                }
                else
                {
                    if (_reverse && !string.IsNullOrEmpty(fromUnit))
                        if (numerator.Contains(fromUnit))
                            numerator.Remove(fromUnit);
                        else
                            throw new VectoException("Unit missing. Conversion not possible.");

                    if (!string.IsNullOrEmpty(toUnit))
                        numerator.Add(toUnit);

                    if (factor.HasValue)
                        _value /= factor.Value;
                }
            }

            foreach (var v in numerator.ToArray().Where(v => denominator.Contains(v)))
            {
                denominator.Remove(v);
                numerator.Remove(v);
            }

            _numerator = denominator.ToArray();
            _denominator = numerator.ToArray();
        }

        #region Unit Definitions
        /// <summary>
        /// Defines the denominator by the terms following after the Per.
        /// </summary>
        [DebuggerHidden]
        public SI Per { get { return new SI(Linear, reciproc: !_reciproc); } }

        /// <summary>
        /// Takes all following terms as cubic terms (=to the power of 3).
        /// </summary>
        [DebuggerHidden]
        public SI Cubic { get { return new SI(this, exponent: 3); } }

        /// <summary>
        /// Takes all following terms as quadratic terms (=to the power of 2).
        /// </summary>
        [DebuggerHidden]
        public SI Square { get { return new SI(this, exponent: 2); } }

        /// <summary>
        /// Takes all following terms as linear terms (=to the power of 1).
        /// </summary>
        [DebuggerHidden]
        public SI Linear { get { return new SI(this, exponent: 1); } }

        /// <summary>
        /// [g] (to basic unit: [kg])
        /// </summary>
        [DebuggerHidden]
        public SI Gramm { get { return new SI(new SI(this, toUnit: "k"), factor: 0.001, fromUnit: "g", toUnit: "g"); } }

        /// <summary>
        /// [N]
        /// </summary>
        [DebuggerHidden]
        public SI Newton { get { return new SI(this, fromUnit: "N", toUnit: "N"); } }

        /// <summary>
        /// [W]
        /// </summary>
        [DebuggerHidden]
        public SI Watt { get { return new SI(this, fromUnit: "W", toUnit: "W"); } }

        /// <summary>
        /// [m]
        /// </summary>
        [DebuggerHidden]
        public SI Meter { get { return new SI(this, fromUnit: "m", toUnit: "m"); } }

        /// <summary>
        /// [s]
        /// </summary>
        [DebuggerHidden]
        public SI Second { get { return new SI(this, fromUnit: "s", toUnit: "s"); } }

        /// <summary>
        /// [rad]
        /// </summary>
        [DebuggerHidden]
        public SI Radian { get { return new SI(this, fromUnit: "rad", toUnit: "rad"); } }

        /// <summary>
        /// Converts to/from Radiant
        /// </summary>
        [DebuggerHidden]
        public SI Rounds { get { return new SI(this, 2 * Math.PI, toUnit: "rad"); } }

        /// <summary>
        /// Converts to/from Second
        /// </summary>
        [DebuggerHidden]
        public SI Hour { get { return new SI(this, factor: 3600.0, fromUnit: "h", toUnit: "s"); } }

        /// <summary>
        /// Converts to/from Second
        /// </summary>
        [DebuggerHidden]
        public SI Minute { get { return new SI(this, factor: 60.0, fromUnit: "min", toUnit: "s"); } }

        /// <summary>
        /// Converts to/from 1000 * Basic Unit 
        /// </summary>
        [DebuggerHidden]
        public SI Kilo { get { return new SI(this, factor: 1000.0, fromUnit: "k"); } }

        /// <summary>
        /// Converts to/from Basic Unit / 100 
        /// </summary>
        [DebuggerHidden]
        public SI Centi { get { return new SI(this, factor: 1.0 / 100.0, fromUnit: "c"); } }
        #endregion

        /// <summary>
        /// Convert an SI unit into another SI unit, defined by term following after the To().
        /// </summary>
        /// <returns></returns>
        public SI To() { return new SI(Linear, reciproc: false, reverse: true); }

        public T To<T>() where T : SI
        {
            var t = (T)Activator.CreateInstance(typeof(T), _value);
            Contract.Assert(HasEqualUnit(t), string.Format("SI Unit Conversion failed: From {0} to {1}", this, t));
            return t;
        }

        public SI ToBasicUnits()
        {
            var numerator = new List<string>();
            var denominator = new List<string>();

            foreach (var unit in _numerator)
            {
                switch (unit)
                {
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

            foreach (var unit in _denominator)
            {
                switch (unit)
                {
                    case "N":
                        denominator.Add("k");
                        denominator.Add("g");
                        denominator.Add("m");
                        denominator.Add("m");
                        numerator.Add("s");
                        numerator.Add("s");
                        numerator.Add("s");
                        break;
                    case "W":
                        denominator.Add("k");
                        denominator.Add("g");
                        denominator.Add("m");
                        numerator.Add("s");
                        numerator.Add("s");
                        break;
                    default:
                        denominator.Add(unit);
                        break;
                }
            }

            return new SI(_value, numerator, denominator);
        }


        /// <summary>
        /// Gets the basic scalar value. 
        /// </summary>
        protected double ScalarValue() { return _value; }

        public SI Value()
        {
            return new SI(_value, _numerator, _denominator);
        }

        #region Operators
        public static SI operator +(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));

            return new SI(si1._value + si2._value, si1._numerator, si1._denominator);
        }

        public static SI operator -(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));

            return new SI(si1._value - si2._value, si1._numerator, si1._denominator);
        }

        public static SI operator *(SI si1, SI si2)
        {
            var numerator = si1._numerator.Concat(si2._numerator).Where(d => d != "rad");
            var denominator = si1._denominator.Concat(si2._denominator).Where(d => d != "rad");
            return new SI(si1._value * si2._value, numerator, denominator);
        }

        public static SI operator /(SI si1, SI si2)
        {
            var numerator = si1._numerator.Concat(si2._denominator).Where(d => d != "rad");
            var denominator = si1._denominator.Concat(si2._numerator).Where(d => d != "rad");
            return new SI(si1._value / si2._value, numerator, denominator);
        }

        public static SI operator +(SI si1, double d)
        {
            return new SI(si1._value + d, si1);
        }

        public static SI operator -(SI si1, double d)
        {
            return new SI(si1._value - d, si1);
        }

        public static SI operator *(SI si1, double d)
        {
            return new SI(si1._value * d, si1);
        }

        public static SI operator *(double d, SI si1)
        {
            return new SI(d * si1._value, si1);
        }

        public static SI operator /(SI si1, double d)
        {
            return new SI(si1._value / d, si1);
        }

        public static SI operator /(double d, SI si1)
        {
            return new SI(d / si1._value, si1);
        }

        public static bool operator <(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));
            return si1._value < si2._value;
        }

        public static bool operator >(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));
            return si1._value > si2._value;
        }

        public static bool operator <=(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));
            return si1._value <= si2._value;
        }

        public static bool operator >=(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));
            return si1._value >= si2._value;
        }

        public static bool operator <(SI si1, double d)
        {
            return si1._value < d;
        }

        public static bool operator >(SI si1, double d)
        {
            return si1._value > d;
        }

        public static bool operator <=(SI si1, double d)
        {
            return si1._value <= d;
        }

        public static bool operator >=(SI si1, double d)
        {
            return si1._value >= d;
        }





        #endregion

        #region Double Conversion
        /// <summary>
        /// Casts an SI Unit to an double.
        /// </summary>
        /// <param name="si"></param>
        /// <returns></returns>
        public static explicit operator double(SI si)
        {
            return si._value;
        }

        /// <summary>
        /// Casts a double to an SI Unit.
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
        /// Returns the Unit Part of the SI Unit Expression.
        /// </summary>
        private string GetUnitString()
        {
            if (_denominator.Any())
                if (_numerator.Any())
                    return string.Format("{0}/{1}", string.Join("", _numerator), string.Join("", _denominator));
                else
                    return string.Format("1/{0}", string.Join("", _denominator));

            if (_numerator.Any())
                return string.Format("{0}", string.Join("", _numerator));

            return "-";
        }

        /// <summary>
        /// Returns the String representation.
        /// </summary>
        public override string ToString() { return string.Format("{0} [{1}]", _value, GetUnitString()); }
        #endregion

        #region Equality members
        /// <summary>
        /// Compares the Unit-Parts of two SI Units.
        /// </summary>
        [Pure]
        public bool HasEqualUnit(SI si)
        {
            return ToBasicUnits()._denominator.OrderBy(x => x).SequenceEqual(si.ToBasicUnits()._denominator.OrderBy(x => x))
                && ToBasicUnits()._numerator.OrderBy(x => x).SequenceEqual(si.ToBasicUnits()._numerator.OrderBy(x => x));
        }

        protected bool Equals(SI other)
        {
            return _value.Equals(other._value) && HasEqualUnit(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as SI;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _value.GetHashCode();
                hashCode = (hashCode * 397) ^ (_numerator != null ? _numerator.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_denominator != null ? _denominator.GetHashCode() : 0);
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

        public SI Abs()
        {
            return new SI(Math.Abs(_value), this);
        }
    }



}