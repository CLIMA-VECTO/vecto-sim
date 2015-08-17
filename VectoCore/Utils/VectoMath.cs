using System;
using System.Collections.Generic;
using System.Diagnostics;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	/// <summary>
	/// Provides helper methods for mathematical functions.
	/// </summary>
	public class VectoMath
	{
		/// <summary>
		/// Linearly interpolates a value between two points.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="x1">First Value on the X-Axis.</param>
		/// <param name="x2">Second Value on the X-Axis.</param>
		/// <param name="y1">First Value on the Y-Axis.</param>
		/// <param name="y2">Second Value on the Y-Axis.</param>
		/// <param name="xint">Value on the X-Axis, for which the Y-Value should be interpolated.</param>
		/// <returns></returns>
		public static TResult Interpolate<T, TResult>(T x1, T x2, TResult y1, TResult y2, T xint) where T : SI
			where TResult : SIBase<TResult>
		{
			return ((xint - x1) * (y2 - y1) / (x2 - x1) + y1).Cast<TResult>();
		}


		public static double Interpolate<T>(T x1, T x2, double y1, double y2, T xint)
			where T : SI
		{
			return (((xint - x1) * (y2 - y1) / (x2 - x1)).Cast<Scalar>() + y1).Value();
		}

		public static TResult Interpolate<TResult>(double x1, double x2, TResult y1, TResult y2, double xint)
			where TResult : SIBase<TResult>
		{
			return ((xint - x1) * (y2 - y1) / (x2 - x1) + y1).Cast<TResult>();
		}

		/// <summary>
		/// Linearly interpolates a value between two points.
		/// </summary>
		public static double Interpolate(double x1, double x2, double y1, double y2, double xint)
		{
			return ((xint - x1) * (y2 - y1) / (x2 - x1) + y1);
		}


		/// <summary>
		/// Returns the absolute value.
		/// </summary>
		public static SI Abs(SI si)
		{
			return si.Abs();
		}

		/// <summary>
		/// Returns the absolute value.
		/// </summary>
		public static T Abs<T>(T si) where T : SIBase<T>
		{
			return si.Abs().Cast<T>();
		}

		/// <summary>
		/// Returns the minimum of two values.
		/// </summary>
		public static T Min<T>(T c1, T c2) where T : IComparable
		{
			return c1.CompareTo(c2) <= 0 ? c1 : c2;
		}

		/// <summary>
		/// Returns the maximum of two values.
		/// </summary>
		public static T Max<T>(T c1, T c2) where T : IComparable
		{
			return c1.CompareTo(c2) >= 0 ? c1 : c2;
		}

		public static T Limit<T>(T value, T lowerBound, T upperBound) where T : SIBase<T>
		{
			if (lowerBound > upperBound)
				throw new VectoException("VectoMath.Limit: lowerBound must not be greater than upperBound");

			if (value > upperBound) {
				return upperBound;
			}
			if (value < lowerBound) {
				return lowerBound;
			}
			return value;
		}

		public static T Sqrt<T>(SI si) where T : SIBase<T>
		{
			return si.Sqrt().Cast<T>();
		}

		/// <summary>
		///		converts the given inclination in percent (0-1+) into Radians
		/// </summary>
		/// <param name="inclinationPercent"></param>
		/// <returns></returns>
		public static Radian InclinationToAngle(double inclinationPercent)
		{
			return Math.Atan(inclinationPercent).SI<Radian>();
		}

		public static List<double> QuadraticEquationSolver(double a, double b, double c)
		{
			var retVal = new List<double>();
			var D = b * b - 4 * a * c;

			if (D < 0) {
				return retVal;
			} else if (D > 0) {
				// two solutions possible
				retVal.Add((-b + Math.Sqrt(D)) / (2 * a));
				retVal.Add((-b - Math.Sqrt(D)) / (2 * a));
			} else {
				// only one solution possible
				retVal.Add((-b / (2 * a)));
			}
			return retVal;
		}
	}
}