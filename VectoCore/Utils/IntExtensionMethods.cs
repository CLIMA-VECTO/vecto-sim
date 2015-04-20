using System;
using System.Diagnostics.Contracts;

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
	}
}