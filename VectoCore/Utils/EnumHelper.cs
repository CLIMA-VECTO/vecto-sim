using System;
using System.Collections.Generic;
using System.Linq;

namespace TUGraz.VectoCore.Utils
{
	public static class EnumHelper
	{
		public static T Parse<T>(this string s, bool ignoreCase = true)
		{
			return (T)Enum.Parse(typeof(T), s, ignoreCase);
		}

		public static IEnumerable<T> GetValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}
	}
}