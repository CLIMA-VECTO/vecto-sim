using System;

namespace TUGraz.VectoCore.Utils
{
	public static class IntExtensionMethods
	{
		/// <summary>
		/// Converts the value from rounds per minute to the SI Unit PerSecond
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static PerSecond RPMtoRad(this int d)
		{
			return d.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second.Cast<PerSecond>();
		}

		/// <summary>
		/// Gets the SI representation of the number (unit-less).
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static SI SI(this int d)
		{
			return (SI)d;
		}

		/// <summary>
		/// Gets the special SI class of the number.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static T SI<T>(this int d) where T : SIBase<T>
		{
			return SIBase<T>.Create(d);
		}

		public static double ToRadian(this int self)
		{
			return self * Math.PI / 180.0;
		}

		/// <summary>
		/// Modulo functions which also works on negative Numbers (not like the built-in %-operator which just returns the remainder).
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int Mod(this int a, int b)
		{
			return (a %= b) < 0 ? a + b : a;
		}
	}
}