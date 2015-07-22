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

		public static T Sum<T>(this IEnumerable<T> list) where T : SIBase<T>
		{
			return list.Aggregate((sum, current) => sum + current);
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
		public static Tuple<T, T> GetSamples<T>(this IEnumerable<T> self, Func<T, bool> skip, out int index)
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
		public static Tuple<T, T> GetSamples<T>(this IEnumerable<T> self, Func<T, bool> predicate)
		{
			int unused;
			return self.GetSamples(predicate, out unused);
		}
	}
}