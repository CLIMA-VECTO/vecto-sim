using System;
using System.Collections.Generic;
using System.Linq;

namespace TUGraz.VectoCore.Utils
{
	public static class IEnumberableExtensionMethods
	{
		public static IEnumerable<double> ToDouble(this IEnumerable<string> self)
		{
			return self.Select(StringExtensionMethods.ToDouble);
		}

		/// <summary>
		/// Wraps this object instance into an IEnumerable.
		/// </summary>
		public static IEnumerable<T> ToEnumerable<T>(this T item)
		{
			yield return item;
		}

		public static Func<bool> Once()
		{
			var once = 0;
			return () => once++ == 0;
		}

		/// <summary>
		/// Get the two adjacent items where the predicate is true.
		/// If the predicate never gets true, the last 2 elements are returned.
		/// </summary>
		public static Tuple<T, T> GetSection<T>(this IEnumerable<T> self, Func<T, bool> skip, out int index)
		{
			var list = self.ToList();
			var skipList = list.Select((arg1, i) => new { skip = skip(arg1) && i < list.Count - 1, i, value = arg1 });
			var p = skipList.SkipWhile(x => x.skip).First();
			index = Math.Max(p.i - 1, 0);
			return Tuple.Create(list[index], list[index + 1]);
		}

		/// <summary>
		/// Get the two adjacent items where the predicate is true.
		/// If the predicate never gets true, the last 2 elements are returned.
		/// </summary>
		public static Tuple<T, T> GetSection<T>(this IEnumerable<T> self, Func<T, bool> predicate)
		{
			int unused;
			return self.GetSection(predicate, out unused);
		}
	}
}