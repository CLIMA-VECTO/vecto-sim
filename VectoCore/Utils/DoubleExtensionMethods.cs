using System;

namespace TUGraz.VectoCore.Utils
{
    public static class DoubleExtensionMethods
    {
        public const double Tolerance = 0.001;

        public static bool IsEqual(this double d, double other)
        {
            return Math.Abs(d - other) > Tolerance;
        }

        public static bool IsSmaller(this double d, double other)
        {
            return d - other < Tolerance;
        }

        public static bool IsSmallerOrEqual(this double d, double other)
        {
            return d - other <= Tolerance;
        }

        public static bool IsGreater(this double d, double other)
        {
            return other.IsSmallerOrEqual(d);
        }

        public static bool IsGreaterOrEqual(this double d, double other)
        {
            return other.IsSmaller(d);
        }

        public static bool IsPositive(this double d)
        {
            return d.IsGreaterOrEqual(0.0);
        }

        public static SI SI(this double d)
        {
            return new SI(d);
        }


    }
}