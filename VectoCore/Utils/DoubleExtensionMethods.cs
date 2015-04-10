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
        /// Converts the double-value from rounds per minute to the SI Unit RadianPerSecond
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static RadianPerSecond RPMtoRad(this double d)
        {
            return d.SI().Rounds.Per.Minute.To().Radian.Per.Second.As<RadianPerSecond>();
        }

        /// <summary>
        ///     Gets the SI representation of the double (unit-less).
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static SI SI(this double d)
        {
            return (SI) d;
        }

        public static T SI<T>(this double d) where T : SIBase<T>
        {
            return (T) Activator.CreateInstance(typeof (T), d);
        }
    }
}