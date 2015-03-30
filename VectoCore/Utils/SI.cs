using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    [DataContract]
    public class SI
    {
        [DataMember]
        private readonly double _value;

        [DataMember]
        private readonly string[] _divident;

        [DataMember]
        private readonly string[] _divisor;

        [DataMember]
        private readonly bool _reciproc;

        [DataMember]
        private readonly bool _reverse;

        [DataMember]
        private readonly int _exponent;

        public SI(double value = 0.0)
        {
            _value = value;
            _reciproc = false;
            _reverse = false;
            _divident = new string[0];
            _divisor = new string[0];
            _exponent = 1;
        }

        private SI(double value, IEnumerable<string> divident, IEnumerable<string> divisor, bool reciproc = false, bool reverse = false, int exponent = 1)
        {
            Contract.Requires(divident != null);
            Contract.Requires(divisor != null);

            _value = value;
            _reciproc = reciproc;
            _reverse = reverse;
            _exponent = exponent;

            var tmpDivident = divident.ToList();
            var tmpDivisor = divisor.ToList();

            foreach (var v in tmpDivisor.ToArray().Where(v => tmpDivident.Contains(v)))
            {
                tmpDivident.Remove(v);
                tmpDivisor.Remove(v);
            }

            _divident = tmpDivident.ToArray();
            _divisor = tmpDivisor.ToArray();
        }

        private SI(SI si, double? factor = null, string fromUnit = null, string toUnit = null,
            bool? reciproc = null, bool? reverse = null, int? exponent = null)
        {
            Contract.Requires(si != null);

            var divisor = si._divisor.ToList();
            var divident = si._divident.ToList();

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
                        if (divident.Contains(fromUnit))
                            divident.Remove(fromUnit);
                        else
                            throw new VectoException("Unit missing. Conversion not possible.");

                    if (!string.IsNullOrEmpty(toUnit))
                        divident.Add(toUnit);

                    if (factor.HasValue)
                        _value *= factor.Value;
                }
                else
                {
                    if (_reverse && !string.IsNullOrEmpty(fromUnit))
                        if (divisor.Contains(fromUnit))
                            divisor.Remove(fromUnit);
                        else
                            throw new VectoException("Unit missing. Conversion not possible.");

                    if (!string.IsNullOrEmpty(toUnit))
                        divisor.Add(toUnit);

                    if (factor.HasValue)
                        _value /= factor.Value;
                }
            }

            foreach (var v in divisor.ToArray().Where(v => divident.Contains(v)))
            {
                divident.Remove(v);
                divisor.Remove(v);
            }

            _divident = divident.ToArray();
            _divisor = divisor.ToArray();
        }

        /// <summary>
        /// Takes all following terms as cubic terms (=to the power of 3).
        /// </summary>
        public SI Cubic { get { return new SI(this, exponent: 3); } }

        /// <summary>
        /// Takes all following terms as quadratic terms (=to the power of 2).
        /// </summary>
        public SI Square { get { return new SI(this, exponent: 2); } }

        /// <summary>
        /// Takes all following terms as linear terms (=to the power of 1).
        /// </summary>
        public SI Linear { get { return new SI(this, exponent: 1); } }

        /// <summary>
        /// Defines the divisor by the following terms.
        /// </summary>
        public SI Per { get { return new SI(Linear, reciproc: !_reciproc); } }

        /// <summary>
        /// Convert an SI unit into another SI unit, defined by following terms.
        /// </summary>
        /// <returns></returns>
        public SI To() { return new SI(Linear, reciproc: false, reverse: true); }

        /// <summary>
        /// [g]
        /// </summary>
        public SI Gramm { get { return new SI(this, fromUnit: "g", toUnit: "g"); } }

        /// <summary>
        /// [N]
        /// </summary>
        public SI Newton { get { return new SI(this, fromUnit: "N", toUnit: "N"); } }

        public SI ToBasicUnits()
        {
            var divident = new List<string>();
            var divisor = new List<string>();

            foreach (var unit in _divident)
            {
                switch (unit)
                {
                    case "W":
                        divident.Add("k");
                        divident.Add("g");
                        divident.Add("m");
                        divident.Add("m");
                        divisor.Add("s");
                        divisor.Add("s");
                        divisor.Add("s");
                        break;
                    case "N":
                        divident.Add("k");
                        divident.Add("g");
                        divident.Add("m");
                        divisor.Add("s");
                        divisor.Add("s");
                        break;
                    default:
                        divident.Add(unit);
                        break;
                }
            }

            foreach (var unit in _divisor)
            {
                switch (unit)
                {
                    case "N":
                        divisor.Add("k");
                        divisor.Add("g");
                        divisor.Add("m");
                        divisor.Add("m");
                        divident.Add("s");
                        divident.Add("s");
                        divident.Add("s");
                        break;
                    case "W":
                        divisor.Add("k");
                        divisor.Add("g");
                        divisor.Add("m");
                        divident.Add("s");
                        divident.Add("s");
                        break;
                    default:
                        divisor.Add(unit);
                        break;
                }
            }

            return new SI(_value, divident, divisor);
        }

        /// <summary>
        /// [W]
        /// </summary>
        public SI Watt { get { return new SI(this, fromUnit: "W", toUnit: "W"); } }

        /// <summary>
        /// [m]
        /// </summary>
        public SI Meter { get { return new SI(this, fromUnit: "m", toUnit: "m"); } }

        /// <summary>
        /// [s]
        /// </summary>
        public SI Second { get { return new SI(this, fromUnit: "s", toUnit: "s"); } }

        /// <summary>
        /// [rad]
        /// </summary>
        public SI Radian { get { return new SI(this, fromUnit: "rad", toUnit: "rad"); } }

        /// <summary>
        /// Converts to/from Radiant
        /// </summary>
        public SI Rounds { get { return new SI(this, 2 * Math.PI, toUnit: "rad"); } }

        /// <summary>
        /// Converts to/from Second
        /// </summary>
        public SI Hour { get { return new SI(this, factor: 3600.0, fromUnit: "h", toUnit: "s"); } }

        /// <summary>
        /// Converts to/from Second
        /// </summary>
        public SI Minute { get { return new SI(this, factor: 60.0, fromUnit: "min", toUnit: "s"); } }

        /// <summary>
        /// Converts to/from 1000 * Basic Unit 
        /// </summary>
        public SI Kilo { get { return new SI(this, factor: 1000.0, fromUnit: "k"); } }

        /// <summary>
        /// Converts to/from Basic Unit / 100 
        /// </summary>
        public SI Centi { get { return new SI(this, factor: 1.0 / 100.0, fromUnit: "c"); } }

        /// <summary>
        /// Gets the basic scalar value. 
        /// </summary>
        public double ScalarValue() { return _value; }

        public SI Value()
        {
            return new SI(_value, _divident, _divisor);
        }

        /// <summary>
        /// Returns the Unit Part of the SI Unit Expression.
        /// </summary>
        private string Unit()
        {
            if (_divisor.Any())
                if (_divident.Any())
                    return string.Format("{0}/{1}", string.Join("", _divident), string.Join("", _divisor));
                else
                    return string.Format("1/{0}", string.Join("", _divisor));

            if (_divident.Any())
                return string.Format("{0}", string.Join("", _divident));

            return "-";
        }

        #region Operators
        public static SI operator +(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));

            return new SI(si1._value + si2._value, si1._divident, si1._divisor);
        }

        public static SI operator -(SI si1, SI si2)
        {
            Contract.Requires(si1.HasEqualUnit(si2));

            return new SI(si1._value - si2._value, si1._divident, si1._divisor);
        }

        public static SI operator *(SI si1, SI si2)
        {
            Contract.Assume(si1._divisor != null);
            Contract.Assume(si2._divisor != null);
            Contract.Assume(si1._divident != null);
            Contract.Assume(si2._divident != null);

            var divident = si1._divident.Concat(si2._divident).Where(d => d != "rad");
            var divisor = si1._divisor.Concat(si2._divisor).Where(d => d != "rad");
            return new SI(si1._value * si2._value, divident, divisor);
        }

        public static SI operator /(SI si1, SI si2)
        {
            Contract.Assume(si1._divisor != null);
            Contract.Assume(si2._divisor != null);
            Contract.Assume(si1._divident != null);
            Contract.Assume(si2._divident != null);

            var divident = si1._divident.Concat(si2._divisor).Where(d => d != "rad");
            var divisor = si1._divisor.Concat(si2._divident).Where(d => d != "rad");
            return new SI(si1._value / si2._value, divident, divisor);
        }
        #endregion



        #region Double Conversion
        /// <summary>
        /// Casts an SI Unit to an double.
        /// </summary>
        /// <param name="si"></param>
        /// <returns></returns>
        public static implicit operator double(SI si)
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
        /// Returns the String representation.
        /// </summary>
        public override string ToString() { return string.Format("{0} [{1}]", _value, Unit()); }
        #endregion

        /// <summary>
        /// Compares the Unit-Parts of two SI Units.
        /// </summary>
        [Pure]
        public bool HasEqualUnit(SI si)
        {
            var quot = si.ToBasicUnits() / ToBasicUnits();
            return quot._divisor.Length == 0 && quot._divident.Length == 0;
        }
    }
}