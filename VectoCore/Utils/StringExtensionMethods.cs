using System;
using System.Globalization;

namespace TUGraz.VectoCore.Utils
{
	public static class StringExtensionMethods
	{
		public static double ToDouble(this string self)
		{
			return double.Parse(self, CultureInfo.InvariantCulture);
		}

        public static string Slice(this string s, int from = 0, int to = int.MaxValue)
        {
            from = Math.Min(Math.Max(from, -s.Length), s.Length);
            from = from < 0 ? from + s.Length : from;
            to = Math.Min(Math.Max(to, -s.Length), s.Length);
            to = to < 0 ? to + s.Length : to;
            return s.Substring(from, Math.Max(to - from, 0));
        }
	}
}