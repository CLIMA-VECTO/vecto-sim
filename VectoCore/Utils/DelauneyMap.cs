using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	[JsonObject(MemberSerialization.Fields)]
	public class DelauneyMap
	{
		private readonly List<Point> _points = new List<Point>();
		private List<Triangle> _triangles = new List<Triangle>();

		[ContractInvariantMethod]
		private void Invariant()
		{
			Contract.Invariant(_points != null);
			Contract.Invariant(_triangles != null);
		}

		public void AddPoint(double x, double y, double z)
		{
			_points.Add(new Point(x, y, z));
		}

		public void Triangulate()
		{
			if (_points.Count < 3) {
				throw new ArgumentException(string.Format("Triangulation needs at least 3 Points. Got {0} Points.", _points.Count));
			}

			// The "supertriangle" encompasses all triangulation points.
			// This is just a helper triangle which initializes the algorithm and will be removed later.
			const int superTriangleScalingFactor = 10;
			var max = _points.Max(point => Math.Max(Math.Abs(point.X), Math.Abs(point.Y))) * superTriangleScalingFactor;
			var superTriangle = new Triangle(new Point(max, 0), new Point(0, max), new Point(-max, -max));
			var triangles = new List<Triangle> { superTriangle };

			foreach (var point in _points) {
				// If the actual vertex lies inside a triangle, the edges of the triangle are 
				// added to the edge buffer and the triangle is removed from list.
				var containerTriangles = triangles.FindAll(t => t.ContainsInCircumcircle(point));
				triangles.RemoveAll(t => t.ContainsInCircumcircle(point));

				// Remove duplicate edges. This leaves the convex hull of the edges.
				// The edges in this convex hull are oriented counterclockwise!
				var convexHullEdges = containerTriangles.
					SelectMany(t => t.GetEdges()).
					GroupBy(edge => edge).
					Where(group => group.Count() == 1).
					SelectMany(group => group);

				var newTriangles = convexHullEdges.Select(edge => new Triangle(edge.P1, edge.P2, point));

				triangles.AddRange(newTriangles);
			}

			_triangles = triangles.FindAll(t => !t.SharesVertexWith(superTriangle));
		}

		public double Interpolate(double x, double y)
		{
			var tr = _triangles.Find(triangle => triangle.IsInside(x, y, exact: true));
			if (tr == null) {
				LogManager.GetLogger(GetType()).Info("Exact search found no fitting triangle. Approximation will be used.");
				tr = _triangles.Find(triangle => triangle.IsInside(x, y, exact: false));
				if (tr == null) {
					throw new VectoException(string.Format("Interpolation failed. x: {0}, y: {1}", x, y));
				}
			}

			var plane = new Plane(tr);
			return (plane.W - plane.X * x - plane.Y * y) / plane.Z;
		}

		#region Equality members

		protected bool Equals(DelauneyMap other)
		{
			return _points.SequenceEqual(other._points) && _triangles.SequenceEqual(other._triangles);
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
			return Equals((DelauneyMap)obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				return ((_points != null ? _points.GetHashCode() : 0) * 397) ^
						(_triangles != null ? _triangles.GetHashCode() : 0);
			}
		}

		#endregion
	}
}