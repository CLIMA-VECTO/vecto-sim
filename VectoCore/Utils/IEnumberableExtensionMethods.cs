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
	}
}