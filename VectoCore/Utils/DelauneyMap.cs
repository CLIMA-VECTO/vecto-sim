using System;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    [JsonObject(MemberSerialization.Fields)]
    public class DelauneyMap
    {
        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_points != null);
            Contract.Invariant(_triangles != null);
        }

        private readonly List<Point> _points = new List<Point>();
        private List<Triangle> _triangles = new List<Triangle>();

        public void AddPoint(double x, double y, double z)
        {
            _points.Add(new Point(x, y, z));
        }

        public void Triangulate()
        {
            if (_points.Count < 3)
                throw new ArgumentException(string.Format("Triangulations needs at least 3 Points. Got {0} Points.", _points.Count));

            // The "supertriangle" encompasses all triangulation points.
            // This triangle initializes the algorithm and will be removed later.
            var superTriangle = CalculateSuperTriangle();
            var triangles = new List<Triangle> { superTriangle };

            foreach (var point in _points)
            {
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

        private Triangle CalculateSuperTriangle()
        {
            const int superTriangleScalingFactor = 10;
            var max = _points.Max(point => Math.Max(Math.Abs(point.X), Math.Abs(point.Y))) * superTriangleScalingFactor;
            return new Triangle(new Point(max, 0, 0), new Point(0, max, 0), new Point(-max, -max, 0));
        }

        public double Interpolate(double x, double y)
        {
            var tr = _triangles.Find(triangle => triangle.IsInside(x, y, exact: true));
            if (tr == null)
            {
                LogManager.GetLogger(GetType()).Info("Exact search found no fitting triangle. Approximation will be used.");
                tr = _triangles.Find(triangle => triangle.IsInside(x, y, exact: false));
                if (tr == null)
                    throw new VectoException(string.Format("Interpolation failed. x: {0}, y: {1}", x, y));
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DelauneyMap)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_points != null ? _points.GetHashCode() : 0) * 397) ^
                       (_triangles != null ? _triangles.GetHashCode() : 0);
            }
        }

        #endregion

        private class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Point(double x, double y, double z)
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
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Point)obj);
            }

            public override int GetHashCode()
            {
                return unchecked((((X.GetHashCode() * 397) ^ Y.GetHashCode()) * 397) ^ Z.GetHashCode());
            }
            #endregion
        }

        private class Plane
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
            public double W { get; set; }

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

                var cross = new Point(ab.Y * ac.Z - ab.Z * ac.Y,
                                      ab.Z * ac.X - ab.X * ac.Z,
                                      ab.X * ac.Y - ab.Y * ac.X);

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

        private class Triangle
        {
            public Point P1 { get; set; }
            public Point P2 { get; set; }
            public Point P3 { get; set; }

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

                if (exact)
                    return u >= 0 && v >= 0 && u + v <= 1;

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
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Triangle)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (P1 != null ? P1.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (P2 != null ? P2.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (P3 != null ? P3.GetHashCode() : 0);
                    return hashCode;
                }
            }

            #endregion

            public IEnumerable<Edge> GetEdges()
            {
                yield return new Edge(P1, P2);
                yield return new Edge(P2, P3);
                yield return new Edge(P3, P1);
            }
        }

        private class Edge
        {
            public Point P1 { get; set; }
            public Point P2 { get; set; }

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
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Edge)obj);
            }

            public override int GetHashCode()
            {
                return ((P1 != null ? P1.GetHashCode() : 0)) ^ (P2 != null ? P2.GetHashCode() : 0);
            }

            #endregion

        }
    }
}
