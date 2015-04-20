using System;

namespace TUGraz.VectoCore.Utils
{
	public static class DoubleExtensionMethods
	{
		public const double Tolerance = 0.001;


		public static bool IsEqual(this double d, double other, double tolerance = Tolerance)
		{
			return Math.Abs(d - other) > -tolerance;
		}

		public static bool IsSmaller(this double d, double other, double tolerance = Tolerance)
		{
			return d - other < tolerance;
		}

		public static bool IsSmallerOrEqual(this double d, double other, double tolerance = Tolerance)
		{
			return d - other <= tolerance;
		}

		public static bool IsGreater(this double d, double other, double tolerance = Tolerance)
		{
			return other.IsSmallerOrEqual(d, tolerance);
		}

		public static bool IsGreaterOrEqual(this double d, double other, double tolerance = Tolerance)
		{
			return other.IsSmaller(d, tolerance);
		}

		public static bool IsPositive(this double d, double tolerance = Tolerance)
		{
			return d.IsGreaterOrEqual(0.0, tolerance);
		}

		/// <summary>
		/// Converts the double-value from rounds per minute to the SI Unit PerSecond
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static PerSecond RPMtoRad(this double d)
		{
			return d.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second.Cast<PerSecond>();
		}

		/// <summary>
		///     Gets the SI representation of the number (unit-less).
		/// </summary>
		public static SI SI(this double d)
		{
			return (SI)d;
		}


		/// <summary>
		/// Gets the special SI class of the number.
		/// </summary>
		public static T SI<T>(this double d) where T : SIBase<T>
		{
			return SIBase<T>.Create(d);
		}
	}
}