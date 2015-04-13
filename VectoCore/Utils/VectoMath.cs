using System;
using Common.Logging.Factory;
using NLog.LayoutRenderers.Wrappers;

namespace TUGraz.VectoCore.Utils
{
	public class VectoMath
	{
		public static double Interpolate(double x1, double x2, double y1, double y2, double xint)
		{
			return (xint - x1) * (y2 - y1) / (x2 - x1) + y1;
		}

		public static SI Abs(SI si)
		{
			return si.Abs();
		}


		public static T Abs<T>(T si) where T : SIBase<T>, new()
		{
			return si.Abs().Cast<T>();
		}

		public static T Min<T>(T c1, T c2) where T : IComparable
		{
			return c1.CompareTo(c2) <= 0 ? c1 : c2;
		}

		public static T Max<T>(T c1, T c2) where T : IComparable
		{
			return c1.CompareTo(c2) >= 0 ? c1 : c2;
		}
	}
}