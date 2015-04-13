using System;
using System.Diagnostics.Contracts;

namespace TUGraz.VectoCore.Utils
{
	public static class IntExtensionMethods
	{
		/// <summary>
		/// Converts the value from rounds per minute to the SI Unit RadianPerSecond
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static RadianPerSecond RPMtoRad(this double d)
		{
			return d.SI().Rounds.Per.Minute.ConvertTo().Radian.Per.Second.Cast<RadianPerSecond>();
		}

		/// <summary>
		/// Gets the SI representation of the number (unit-less).
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static SI SI(this int d)
		{
			return (SI) d;
		}

		/// <summary>
		/// Gets the special SI class of the number.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static T SI<T>(this int d) where T : SIBase<T>
		{
			return (T) Activator.CreateInstance(typeof (T), d);
		}
	}
}