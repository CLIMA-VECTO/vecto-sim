using System;
using System.Collections;
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
		public const double Tolerance = 0.001;

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
			return self - other < tolerance;
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
			return self - other <= tolerance;
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
			return other.IsSmallerOrEqual(self, tolerance);
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
			return other.IsSmaller(self, tolerance);
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

		/// <summary>
		/// Creates an SI object for the number (unit-less: [-]).
		/// </summary>
		/// <param name="self">The self.</param>
		/// <returns></returns>
		public static SI SI(this double self)
		{
			return (SI) self;
		}

		/// <summary>
		/// Creates an templated SI object for the number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self">The self.</param>
		/// <returns></returns>
		public static T SI<T>(this double self) where T : SIBase<T>
		{
			return SIBase<T>.Create(self);
		}

		public static IEnumerable<T> SI<T>(this IEnumerable<double> self) where T : SIBase<T>
		{
			return self.Select(x => x.SI<T>());
		}
	}
}