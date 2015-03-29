using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace TUGraz.VectoCore.Utils
{
    public class SI
    {
        private readonly double _value;
        private readonly string[] _divident;
        private readonly string[] _divisor;
        private readonly bool _reciproc;
        private readonly bool _reverse;
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
                        divident.Remove(fromUnit);

                    if (!string.IsNullOrEmpty(toUnit))
                        divident.Add(toUnit);

                    if (factor.HasValue)
                        _value *= factor.Value;
                }
                else
                {
                    if (_reverse && !string.IsNullOrEmpty(fromUnit))
                        divisor.Remove(fromUnit);

                    if (!string.IsNullOrEmpty(toUnit))
                        divisor.Add(toUnit);

                    if (factor.HasValue)
                        _value /= factor.Value;
                }
            }

            //shorten out common terms
            foreach (var v in divident.ToArray())
            {
                if (divisor.Contains(v))
                {
                    divident.Remove(v);
                    divisor.Remove(v);
                }
            }

            _divident = divident.ToArray();
            _divisor = divisor.ToArray();
        }

        public double Value { get { return _value; } }

        public SI Cubic { get { return new SI(this, exponent: 3); } }

        public SI Square { get { return new SI(this, exponent: 2); } }

        public SI Linear { get { return new SI(this, exponent: 1); } }

        public SI Per { get { return new SI(Linear, reciproc: !_reciproc); } }

        public SI ConvertTo { get { return new SI(Linear, reciproc: false, reverse: true); } }

        public SI Gramm { get { return new SI(this, fromUnit: "g", toUnit: "g"); } }

        public SI Newton { get { return new SI(this, fromUnit: "N", toUnit: "N"); } }

        public SI Watt { get { return new SI(this, fromUnit: "W", toUnit: "W"); } }

        public SI Meter { get { return new SI(this, fromUnit: "m", toUnit: "m"); } }

        public SI Second { get { return new SI(this, fromUnit: "s", toUnit: "s"); } }

        public SI Radiant { get { return new SI(this, fromUnit: "rad", toUnit: "rad"); } }

        public SI Rounds { get { return new SI(this, 2 * Math.PI, toUnit: "rad"); } }

        public SI Hour { get { return new SI(this, factor: 3600.0, fromUnit: "h", toUnit: "s"); } }

        public SI Minute { get { return new SI(this, factor: 60.0, fromUnit: "min", toUnit: "s"); } }

        public SI Kilo { get { return new SI(this, factor: 1000.0, fromUnit: "K"); } }

        public SI Centi { get { return new SI(this, factor: 1.0 / 100.0, fromUnit: "c"); } }
        public string Unit
        {
            get
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
        }

        #region Double Conversion
        public static implicit operator double(SI si)
        {
            return si._value;
        }

        public static explicit operator SI(double d)
        {
            return new SI(d);
        }
        #endregion

        #region ToString
        public override string ToString() { return string.Format("{0} [{1}]", _value, Unit); }
        #endregion

        [Pure]
        public bool HasEqualUnit(SI si)
        {
            return Equals(Unit, si.Unit);
        }
    }
}