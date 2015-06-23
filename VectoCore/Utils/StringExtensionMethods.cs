using System.Globalization;

namespace TUGraz.VectoCore.Utils
{
	public static class StringExtensionMethods
	{
		public static double ToDouble(this string self)
		{
			return double.Parse(self, CultureInfo.InvariantCulture);
		}
	}
}