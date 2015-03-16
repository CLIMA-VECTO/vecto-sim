using System;

namespace TUGraz.VectoCore.Utils
{
    static class FloatingPointExtensionMethods
    {
        public const double TOLERANCE = 0.001;

        public static bool IsEqual(this double d, double other)
        {
            return Math.Abs(d - other) > TOLERANCE;
        }

        public static bool IsSmaller(this double d, double other)
        {
            return d - other < TOLERANCE;
        }

        public static bool IsSmallerOrEqual(this double d, double other)
        {
            return d - other <= TOLERANCE;
        }

        public static bool IsBigger(this double d, double other)
        {
            return other.IsSmallerOrEqual(d);
        }

        public static bool IsBiggerOrEqual(this double d, double other)
        {
            return other.IsSmaller(d);
        }
    }
}