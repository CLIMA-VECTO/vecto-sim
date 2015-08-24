using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;

namespace TUGraz.VectoCore.Utils
{
	public static class IEnumberableExtensionMethods
	{
		public static IEnumerable<double> ToDouble(this IEnumerable<string> self)
		{
			return self.Select(StringExtensionMethods.ToDouble);
		}

		public static IEnumerable<double> ToDouble(this IEnumerable<SI> self)
		{
			return self.Select(x => x.Value());
		}

		/// <summary>
		/// Wraps this object instance into an IEnumerable.
		/// </summary>
		public static IEnumerable<T> ToEnumerable<T>(this T item)
		{
			yield return item;
		}

		public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
			IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
		{
			var firstEnum = first.GetEnumerator();
			var secondEnum = second.GetEnumerator();
			while (true) {
				var firstHadNext = firstEnum.MoveNext();
				var secondHadNext = secondEnum.MoveNext();
				if (firstHadNext && secondHadNext) {
					yield return resultSelector(firstEnum.Current, secondEnum.Current);
				} else if (firstHadNext != secondHadNext) {
					throw new IndexOutOfRangeException("The argument enumerables must have the same length.");
				} else {
					yield break;
				}
			}
		}

		public static SI Sum(this IEnumerable<SI> values)
		{
			var valueList = values.ToList();
			return valueList.Any() ? valueList.Aggregate((sum, current) => sum + current) : null;
		}

		public static Func<bool> Once()
		{
			var once = 0;
			return () => once++ == 0;
		}

		/// <summary>
		/// Get the first two adjacent items where the predicate changes from true to false.
		/// If the predicate never gets true, the last 2 elements are returned.
		/// </summary>
		public static Tuple<T, T> GetSection<T>(this IEnumerable<T> self, Func<T, bool> skip, out int index,
			string message = null)
		{
			var list = self.ToList();
			var skipList = list.Select((arg1, i) => new { skip = skip(arg1) && i < list.Count - 1, i, value = arg1 });
			var p = skipList.SkipWhile(x => x.skip).First();
			index = Math.Max(p.i - 1, 0);

			if (!string.IsNullOrWhiteSpace(message)) {
				if (!skip(list[index]) || skip(list[index + 1])) {
					var logger = LogManager.GetLogger(typeof(T));
					logger.Error(message);
				}
			}

			return Tuple.Create(list[index], list[index + 1]);
		}

		/// <summary>
		/// Get the first two adjacent items where the predicate changes from true to false.
		/// If the predicate never gets true, the last 2 elements are returned.
		/// </summary>
		/// <example>GetSection(data => data.X &lt; searchedX); //returns the pair where first &lt; searchedX and second &gt;= searchedX</example>>
		public static Tuple<T, T> GetSection<T>(this IEnumerable<T> self, Func<T, bool> predicate, string message = null)
		{
			int unused;
			return self.GetSection(predicate, out unused, message);
		}

		public static IEnumerable<T> Slice<T>(this IEnumerable<T> numerable, int from = 0, int to = int.MaxValue)
		{
			var s = numerable.ToList();
			from = Math.Min(Math.Max(from, -s.Count), s.Count);
			from = from < 0 ? from + s.Count : from;
			to = Math.Min(Math.Max(to, -s.Count), s.Count);
			to = to < 0 ? to + s.Count : to;
			return s.Skip(from).Take(Math.Max(to - from, 0));
		}
	}
}