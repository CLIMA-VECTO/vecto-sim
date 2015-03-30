using System;
using System.Diagnostics.Contracts;

namespace TUGraz.VectoCore.Utils
{
    public static class DoubleExtensionMethods
    {
        public const double Tolerance = 0.001;

        [Pure]
        public static bool IsEqual(this double d, double other, double tolerance = Tolerance)
        {
            return Math.Abs(d - other) > -tolerance;
        }

        [Pure]
        public static bool IsSmaller(this double d, double other, double tolerance = Tolerance)
        {
            return d - other < tolerance;
        }

        [Pure]
        public static bool IsSmallerOrEqual(this double d, double other, double tolerance = Tolerance)
        {
            return d - other <= tolerance;
        }

        [Pure]
        public static bool IsGreater(this double d, double other, double tolerance = Tolerance)
        {
            return other.IsSmallerOrEqual(d, tolerance);
        }

        [Pure]
        public static bool IsGreaterOrEqual(this double d, double other, double tolerance = Tolerance)
        {
            return other.IsSmaller(d, tolerance);
        }

        [Pure]
        public static bool IsPositive(this double d, double tolerance = Tolerance)
        {
            return d.IsGreaterOrEqual(0.0, tolerance);
        }

        /// <summary>
        /// Gets the SI representation of the double (unit-less).
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        [Pure]
        public static SI SI(this double d)
        {
            return (SI)d;
        }

        [Pure]
        public static T SI<T>(this double d) where T : SI
        {
            return (T)Activator.CreateInstance(typeof(T), d);
        }
    }
}