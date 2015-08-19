using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
			if (lowerBound > upperBound) {
				throw new VectoException("VectoMath.Limit: lowerBound must not be greater than upperBound");
			}

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

	public class Point
	{
		public double X;
		public double Y;
		public double Z;

		public Point(double x, double y, double z = 0)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Point operator -(Point p1, Point p2)
		{
			Contract.Requires(p1 != null);
			Contract.Requires(p2 != null);
			return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
		}

		public Point CrossProduct(Point other)
		{
			return new Point(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);
		}

		/// <summary>
		/// Determines whether this point is left of the given edge.
		/// </summary>
		/// <remarks>Calculates the cross product and checks if the z-component is positive or negative.</remarks>
		public bool IsLeftOf(Edge e)
		{
			var ab = e.P2 - e.P1;
			var ac = this - e.P1;
			var cross = ab.CrossProduct(ac);
			return cross.Z.IsGreater(0);
		}

		/// <summary>
		/// Determines whether this point is right of the given edge.
		/// </summary>
		/// <remarks>Calculates the cross product and checks if the z-component is positive or negative.</remarks>
		public bool IsRightOf(Edge e)
		{
			var ab = e.P2 - e.P1;
			var ac = this - e.P1;
			var cross = ab.CrossProduct(ac);
			return cross.Z.IsSmaller(0);
		}

		public override string ToString()
		{
			return string.Format("Point({0}, {1}, {2})", X, Y, Z);
		}

		#region Equality members

		protected bool Equals(Point other)
		{
			Contract.Requires(other != null);

			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			return obj.GetType() == GetType() && Equals((Point)obj);
		}

		public override int GetHashCode()
		{
			return unchecked((((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Z.GetHashCode());
		}

		#endregion
	}

	public class Plane
	{
		public double X;
		public double Y;
		public double Z;
		public double W;

		public Plane(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Plane(Triangle tr)
		{
			Contract.Requires(tr != null);
			Contract.Requires(tr.P1 != null);
			Contract.Requires(tr.P2 != null);
			Contract.Requires(tr.P3 != null);

			var ab = tr.P2 - tr.P1;
			var ac = tr.P3 - tr.P1;

			var cross = ab.CrossProduct(ac);

			X = cross.X;
			Y = cross.Y;
			Z = cross.Z;
			W = tr.P1.X * cross.X + tr.P1.Y * cross.Y + tr.P1.Z * cross.Z;
		}

		public override string ToString()
		{
			return string.Format("Plane({0}, {1}, {2}, {3})", X, Y, Z, W);
		}
	}

	public class Triangle
	{
		public Point P1;
		public Point P2;
		public Point P3;

		public Triangle(Point p1, Point p2, Point p3)
		{
			P1 = p1;
			P2 = p2;
			P3 = p3;
		}

		public bool IsInside(double x, double y, bool exact = true)
		{
			Contract.Requires(P1 != null);
			Contract.Requires(P2 != null);
			Contract.Requires(P3 != null);

			//Barycentric Technique: http://www.blackpawn.com/texts/pointinpoly/default.html
			var p = new Point(x, y, 0);

			var v0 = P3 - P1;
			var v1 = P2 - P1;
			var v2 = p - P1;

			var dot00 = v0.X * v0.X + v0.Y * v0.Y;
			var dot01 = v0.X * v1.X + v0.Y * v1.Y;
			var dot02 = v0.X * v2.X + v0.Y * v2.Y;
			var dot11 = v1.X * v1.X + v1.Y * v1.Y;
			var dot12 = v1.X * v2.X + v1.Y * v2.Y;

			var invDenom = 1.0 / (dot00 * dot11 - dot01 * dot01);
			var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
			var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

			if (exact) {
				return u >= 0 && v >= 0 && u + v <= 1;
			}

			return u.IsPositive() && v.IsPositive() && (u + v).IsSmallerOrEqual(1);
		}

		public bool ContainsInCircumcircle(Point p)
		{
			Contract.Requires(p != null);
			Contract.Requires(P1 != null);
			Contract.Requires(P2 != null);
			Contract.Requires(P3 != null);

			var p0 = P1 - p;
			var p1 = P2 - p;
			var p2 = P3 - p;

			var p0square = p0.X * p0.X + p0.Y * p0.Y;
			var p1square = p1.X * p1.X + p1.Y * p1.Y;
			var p2square = p2.X * p2.X + p2.Y * p2.Y;

			var det01 = p0.X * p1.Y - p1.X * p0.Y;
			var det12 = p1.X * p2.Y - p2.X * p1.Y;
			var det20 = p2.X * p0.Y - p0.X * p2.Y;

			var result = p0square * det12 + p1square * det20 + p2square * det01;

			return result > 0;
		}

		private bool Contains(Point p)
		{
			return p.Equals(P1) || p.Equals(P2) || p.Equals(P3);
		}

		public bool SharesVertexWith(Triangle t)
		{
			Contract.Requires(t != null);
			return Contains(t.P1) || Contains(t.P2) || Contains(t.P3);
		}

		public IEnumerable<Edge> GetEdges()
		{
			yield return new Edge(P1, P2);
			yield return new Edge(P2, P3);
			yield return new Edge(P3, P1);
		}

		public override string ToString()
		{
			return string.Format("Triangle({0}, {1}, {2})", P1, P2, P3);
		}

		#region Equality members

		protected bool Equals(Triangle other)
		{
			Contract.Requires(other != null);
			return Equals(P1, other.P1) && Equals(P2, other.P2) && Equals(P3, other.P3);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != GetType()) {
				return false;
			}
			return Equals((Triangle)obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (P1 != null ? P1.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (P2 != null ? P2.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (P3 != null ? P3.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion
	}

	public class Edge
	{
		public Point P1;
		public Point P2;

		public Edge(Point p1, Point p2)
		{
			P1 = p1;
			P2 = p2;
		}

		public override string ToString()
		{
			return string.Format("Edge({0}, {1})", P1, P2);
		}

		#region Equality members

		protected bool Equals(Edge other)
		{
			Contract.Requires(other != null);
			return Equals(P1, other.P1) && Equals(P2, other.P2)
					|| Equals(P1, other.P2) && Equals(P1, other.P2);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			return obj.GetType() == GetType() && Equals((Edge)obj);
		}

		public override int GetHashCode()
		{
			return ((P1 != null ? P1.GetHashCode() : 0)) ^ (P2 != null ? P2.GetHashCode() : 0);
		}

		#endregion
	}
}