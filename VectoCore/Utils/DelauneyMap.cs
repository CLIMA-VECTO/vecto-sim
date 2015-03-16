using System;
using System.Linq;
using System.Collections.Generic;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    class DelauneyMap
    {
        private readonly List<Point> _points = new List<Point>();
        private List<Triangle> _triangles = new List<Triangle>();

        public void AddPoint(double x, double y, double z)
        {
            _points.Add(new Point(x, y, z));
        }

        protected bool Equals(DelauneyMap other)
        {
            return _points.SequenceEqual(other._points) 
                && _triangles.SequenceEqual(other._triangles);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelauneyMap) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_points != null ? _points.GetHashCode() : 0)*397) ^ (_triangles != null ? _triangles.GetHashCode() : 0);
            }
        }

        public void Triangulate()
        {
            const int superTriangleScalingFactor = 10;

            if (_points.Count < 3)
                throw new ArgumentException(string.Format("Triangulations needs at least 3 Points. Got {0} Points.", _points.Count));

            // The "supertriangle" encompasses all triangulation points.
            // This triangle initializes the algorithm and will be removed later.
            var max = _points.Max(point => Math.Max(Math.Abs(point.X), Math.Abs(point.Y))) * superTriangleScalingFactor;
            var superTriangle = new Triangle(new Point(max, 0, 0), new Point(0, max, 0), new Point(-max, -max, 0));

            var triangles = new List<Triangle> { superTriangle };

            foreach (var point in _points)
            {
                var edges = new List<Edge>();

                // If the actual vertex lies inside the circumcircle, then the three edges of the 
                // triangle are added to the edge buffer and the triangle is removed from list.
                foreach (var containerTriangle in triangles.Where(triangle => triangle.ContainsInCircumcircle(point)).ToList())
                {
                    edges.Add(new Edge(containerTriangle.P1, containerTriangle.P2));
                    edges.Add(new Edge(containerTriangle.P2, containerTriangle.P3));
                    edges.Add(new Edge(containerTriangle.P3, containerTriangle.P1));
                    triangles.Remove(containerTriangle);
                }

                // Remove duplicate edges. This leaves the convex hull of the edges.
                // The edges in this convex hull are oriented counterclockwise!
                var convexHullEdges = edges.GroupBy(edge => edge).Where(group => group.Count() == 1).SelectMany(group => group);

                // Generate new counterclockwise oriented triangles filling the "hole" in
                // the existing triangulation. These triangles all share the actual vertex.
                var counterTriangles = convexHullEdges.Select(edge => new Triangle(edge.P1, edge.P2, point));
                triangles.AddRange(counterTriangles);
            }

            // Remove all triangles sharing a vertex with the supertriangle.
            _triangles = triangles.Where(triangle => !triangle.SharesVertexWith(superTriangle)).ToList();
        }

        public double Interpolate(double x, double y)
        {
            var tr = _triangles.Find(triangle => triangle.IsInside(x, y, exact: true)) ??
                     _triangles.Find(triangle => triangle.IsInside(x, y, exact: false));

            if (tr == null)
                throw new VectoException("Interpolation failed.");

            var plane = new Plane(tr);
            return (plane.W - plane.X * x - plane.Y * y) / plane.Z;
        }
    }

    public class Point
    {
        protected bool Equals(Point other)
        {
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

        public override string ToString()
        {
            return string.Format("Point({0}, {1}, {2})", X, Y, Z);
        }

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
            return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }
    }

    public class Plane
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

    public class Triangle
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

        public override string ToString()
        {
            return string.Format("Triangle({0}, {1}, {2})", P1, P2, P3);
        }

        public bool IsInside(double x, double y, bool exact = true)
        {
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

            return (u >= -0.001) && (v >= -0.001) && (u + v <= 1.001);
        }

        public bool ContainsInCircumcircle(Point p)
        {
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

        public bool SharesVertexWith(Triangle t)
        {
            return (P1.Equals(t.P1) || P1.Equals(t.P2) || P1.Equals(t.P3)) ||
                   (P2.Equals(t.P1) || P2.Equals(t.P2) || P2.Equals(t.P3)) ||
                   (P3.Equals(t.P1) || P3.Equals(t.P2) || P3.Equals(t.P3));
        }

        protected bool Equals(Triangle other)
        {
            return Equals(P1, other.P1) && Equals(P2, other.P2) && Equals(P3, other.P3);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Triangle) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (P1 != null ? P1.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (P2 != null ? P2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (P3 != null ? P3.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class Edge
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

        protected bool Equals(Edge other)
        {
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
    }
}
