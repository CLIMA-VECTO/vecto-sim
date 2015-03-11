using System;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    static class FloatingPointExtensionMethods
    {
        public const double TOLERANCE = 0.00001;

        public static bool IsEqual(this double d, double other)
        {
            return Math.Abs(d - other) > TOLERANCE;
        }

        public static bool IsSmaller(this double d, double other)
        {
            return d - other < TOLERANCE;
        }

        public static bool IsSmallerOrEqual(this double d, double other)
        {
            return d - other <= TOLERANCE;
        }

        public static bool IsBigger(this double d, double other)
        {
            return other.IsSmaller(d);
        }

        public static bool IsBiggerOrEqual(this double d, double other)
        {
            return other.IsSmallerOrEqual(d);
        }
    }


    class DelauneyMap
    {
        private int ptDim;

        private List<Point> ptList;
        private List<Point> ptListXZ;

        private List<Triangle> lDT;
        private List<Triangle> lDTXZ;

        private bool DualMode { get; set; }


        public DelauneyMap(bool dualMode = false)
        {
            ptList = new List<Point>();
            ptListXZ = new List<Point>();
            DualMode = dualMode;


        }

        public void AddPoints(double x, double y, double z)
        {
            ptList.Add(new Point(x, y, z));
            ptListXZ.Add(new Point(x, z, y));

            double p = 0;

        }

        public void Triangulate()
        {
            lDT = Triangulate(ptList);
            lDTXZ = Triangulate(ptListXZ);
        }

        private List<Triangle> Triangulate(List<Point> points)
        {
            if (points.Count < 3)
                throw new ArgumentException("Can not triangulate less than three vertices!");

            var triangles = new List<Triangle>();

            // The "supertriangle" which encompasses all triangulation points.
            // This triangle initializes the algorithm and will be removed later.
            Triangle superTriangle = CalculateSuperTriangle(points);
            triangles.Add(superTriangle);

            foreach (var p in points)
            {
                var edges = new List<Edge>();

                // If the actual vertex lies inside the circumcircle, then the three edges of the 
                // triangle are added to the edge buffer and the triangle is removed from list.
                var containerTriangles = triangles.Where(t => t.ContainsInCircumcircle(p) > 0).ToList();
                foreach (var t in containerTriangles)
                {
                    edges.Add(new Edge(t.P1, t.P2));
                    edges.Add(new Edge(t.P2, t.P3));
                    edges.Add(new Edge(t.P3, t.P1));
                }
                // Remove all container triangles
                triangles = triangles.Except(containerTriangles).ToList();

                // Remove duplicate edges. This leaves the convex hull of the edges.
                // The edges in this convex hull are oriented counterclockwise!
                edges = edges.GroupBy(e => e)
                    .Where(g => g.Count() == 1)
                    .Select(g => g.Key)
                    .ToList();

                // Generate new counterclockwise oriented triangles filling the "hole" in
                // the existing triangulation. These triangles all share the actual vertex.
                var counterTriangles = edges.Select(e => new Triangle(e.StartPoint, e.EndPoint, p));
                triangles.AddRange(counterTriangles);
            }

            // We don't want the supertriangle in the triangulation, so
            // remove all triangles sharing a vertex with the supertriangle.
            triangles = triangles.Where(t => !t.SharesVertexWith(superTriangle)).ToList();

            return triangles;
        }

        public double Interpolate(double x, double y)
        {
            return Interpolate(lDT, x, y);
        }

        public double InterpolateXZ(double x, double z)
        {
            return Interpolate(lDTXZ, x, z);
        }

        private double Interpolate(List<Triangle> triangles, double x, double y)
        {
            var tr = triangles.FirstOrDefault(t => IsInside(t, x, y, exact: true)) ??
                     triangles.FirstOrDefault(t => IsInside(t, x, y, exact: false));

            if (tr == null)
                throw new VectoException("Interpolation failed.");

            var plane = new Plane(tr);
            return (plane.W - x * plane.X - y * plane.Y) / plane.Z;
        }



        private bool IsInside(Triangle tr, double x, double y, bool exact = true)
        {
            var p = new Point(x, y);

            var v0 = tr.P3 - tr.P1;
            var v1 = tr.P2 - tr.P1;
            var v2 = p - tr.P1;

            var dot00 = v0.DotProduct(v0);
            var dot01 = v0.DotProduct(v1);
            var dot02 = v0.DotProduct(v2);
            var dot11 = v1.DotProduct(v1);
            var dot12 = v1.DotProduct(v2);

            var invDenom = 1.0 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            if (exact)
                return u >= 0 && v >= 0 && u + v <= 1;

            return u.IsBiggerOrEqual(0) && v.IsBiggerOrEqual(0) && (u + v).IsSmallerOrEqual(1);
        }



        private Triangle CalculateSuperTriangle(List<Point> triangulationPoints)
        {
            const int scalingFactor = 10;
            var max = triangulationPoints.Select(t => Math.Max(Math.Abs(t.X), Math.Abs(t.Y))).Max();

            max *= scalingFactor;

            var p1 = new Point(max, 0);
            var p2 = new Point(0, max);
            var p3 = new Point(-max, -max);

            return new Triangle(p1, p2, p3);
        }
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point(double x, double y, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.X.IsEqual(right.X) && left.Y.IsEqual(right.Y);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        /// <summary>
        /// Vectorial Product, also called "Ex"-Product (P1 x P2)
        /// </summary>
        public Point ExProduct(Point p2)
        {
            return new Point(Y * p2.Z - Z * p2.Y,
                             Z * p2.X - X * p2.Z,
                             X * p2.Y - Y * p2.X);
        }

        /// <summary>
        /// Scalar Product, also called "In"-Product or Dot-Product (P1 . P2)
        /// </summary>
        /// <param name="p1"></param>
        /// <returns></returns>
        public double DotProduct(Point p1)
        {
            return X * p1.X + Y * p1.Y + Z * p1.Z;
        }

        public double Determinant(Point p1)
        {
            return X * p1.Y - p1.X * Y;
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

        public Plane(Triangle tr) : this(tr.P1, tr.P2, tr.P3) { }

        public Plane(Point p1, Point p2, Point p3)
        {
            var prod = (p2 - p1).ExProduct(p3 - p1);
            X = prod.X;
            Y = prod.Y;
            Z = prod.Z;
            W = p1.DotProduct(prod);
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

        public double ContainsInCircumcircle(Point pt)
        {
            var p0 = P1 - pt;
            var p1 = P2 - pt;
            var p2 = P3 - pt;

            return p0.DotProduct(p0) * p0.Determinant(p1)
                 + p1.DotProduct(p1) * p2.Determinant(p0)
                 + p2.DotProduct(p2) * p1.Determinant(p2);
        }

        public bool SharesVertexWith(Triangle t)
        {
            return (P1 == t.P1 || P1 == t.P2 || P1 == t.P3) ||
                   (P2 == t.P1 || P2 == t.P2 || P2 == t.P3) ||
                   (P3 == t.P1 || P3 == t.P2 || P3 == t.P3);
        }
    }

    public class Edge
    {
        public Point StartPoint;
        public Point EndPoint;

        public Edge(Point p1, Point p2)
        {
            StartPoint = p1;
            EndPoint = p2;
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return left.StartPoint == right.StartPoint && left.EndPoint == right.EndPoint || left.StartPoint == right.EndPoint && left.EndPoint == right.StartPoint;
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }
    }
}
