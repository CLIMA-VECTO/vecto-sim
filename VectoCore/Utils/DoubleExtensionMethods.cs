using System;
using System.Collections.Generic;
using System.Linq;

namespace TUGraz.VectoCore.Utils
{
	/// <summary>
	/// Extension methods for double.
	/// </summary>
	public static class DoubleExtensionMethods
	{
		/// <summary>
		/// The tolerance.
		/// </summary>
		public const double Tolerance = 1e-6;

		/// <summary>
		/// The tolerancefactor for relative comparisons.
		/// </summary>
		public const double ToleranceFactor = 1e-6;


		/// <summary>
		/// Determines whether the specified other is equal within tolerance.
		/// </summary>
		/// <param name="self">The self.</param>
		/// <param name="other">The other.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static bool IsEqual(this double self, double other, double tolerance = Tolerance)
		{
			return Math.Abs(self - other) < tolerance;
		}

		/// <summary>
		/// Determines whether the specified other is smaller within tolerance.
		/// </summary>
		/// <param name="self">The self.</param>
		/// <param name="other">The other.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static bool IsSmaller(this double self, double other, double tolerance = Tolerance)
		{
			return self < other - tolerance;
		}

		/// <summary>
		/// Determines whether the specified other is smaller or equal within tolerance.
		/// </summary>
		/// <param name="self">The self.</param>
		/// <param name="other">The other.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static bool IsSmallerOrEqual(this double self, double other, double tolerance = Tolerance)
		{
			return self <= other + tolerance;
		}

		/// <summary>
		/// Determines whether the specified other is greater within tolerance.
		/// </summary>
		/// <param name="self">The self.</param>
		/// <param name="other">The other.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static bool IsGreater(this double self, double other, double tolerance = Tolerance)
		{
			return self > other + tolerance;
		}

		/// <summary>
		/// Determines whether the specified other is greater or equal within tolerance.
		/// </summary>
		/// <param name="self">The self.</param>
		/// <param name="other">The other.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static bool IsGreaterOrEqual(this double self, double other, double tolerance = Tolerance)
		{
			return self >= other - tolerance;
		}

		/// <summary>
		/// Determines whether the specified tolerance is positive within tolerance.
		/// </summary>
		/// <param name="self">The self.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns></returns>
		public static bool IsPositive(this double self, double tolerance = Tolerance)
		{
			return self.IsGreaterOrEqual(0.0, tolerance);
		}

		/// <summary>
		/// Converts the double-value from RPM (rounds per minute) to the SI Unit PerSecond.
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static PerSecond RPMtoRad(this double self)
		{
			return self.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second.Cast<PerSecond>();
		}

		public static MeterPerSecond KMPHtoMeterPerSecond(this double self)
		{
			return SI<MeterPerSecond>(self / 3.6);
			//return self.SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>();
		}

		public static double ToRadian(this double self)
		{
			return self * Math.PI / 180.0;
		}

		public static double ToDegree(this double self)
		{
			return self * 180.0 / Math.PI;
		}


		/// <summary>
		/// Creates an SI object for the number (unit-less: [-]).
		/// </summary>
		public static SI SI(this double value)
		{
			return new SI(value);
		}

		/// <summary>
		/// Creates an templated SI object for the number.
		/// </summary>
		public static T SI<T>(this double value) where T : SIBase<T>
		{
			return SIBase<T>.Create(value);
		}

		public static IEnumerable<T> SI<T>(this IEnumerable<double> self) where T : SIBase<T>
		{
			return self.Select(x => x.SI<T>());
		}
	}
}