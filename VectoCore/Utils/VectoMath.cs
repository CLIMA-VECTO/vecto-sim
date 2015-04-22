using System;
using Common.Logging.Factory;
using NLog.LayoutRenderers.Wrappers;

namespace TUGraz.VectoCore.Utils
{
	public class VectoMath
	{
		public static T2 Interpolate<T1, T2>(T1 x1, T1 x2, T2 y1, T2 y2, T1 xint) where T1 : SI where T2 : SIBase<T2>, new()
		{
			return ((xint - x1) * (y2 - y1) / (x2 - x1) + y1).Cast<T2>();
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