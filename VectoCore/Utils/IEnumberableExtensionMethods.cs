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
		/// Get the two adjacent items where the predicate is true.
		/// If the predicate never gets true, the last 2 elements are returned.
		/// </summary>
		public static Tuple<T, T> GetSection<T>(this IEnumerable<T> self, Func<T, bool> predicate, out int index)
		{
			var list = self.ToList();
			if (list.Count < 2) {
				throw new InvalidOperationException("GetSection expects a sequence with at least 2 elements.");
			}
			var p = list.Select((arg1, i) => new { skip = i < list.Count - 2 || predicate(arg1), i, value = arg1 }).
				SkipWhile(x => x.skip).Take(2).ToArray();

			index = p[0].i;
			return Tuple.Create(p[0].value, p[1].value);
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