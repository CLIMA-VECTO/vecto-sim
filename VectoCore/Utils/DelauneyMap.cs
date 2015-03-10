using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
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
            Triangle superTriangle = SuperTriangle(points);
            triangles.Add(superTriangle);

            for (var i = 0; i < points.Count; i++)
            {
                var edges = new List<Edge>();

                // If the actual vertex lies inside the circumcircle, then the three edges of the 
                // triangle are added to the edge buffer and the triangle is removed from list.                             
                for (var j = triangles.Count - 1; j >= 0; j--)
                {
                    var t = triangles[j];
                    if (t.ContainsInCircumcircle(points[i]) > 0)
                    {
                        edges.Add(new Edge(t.P1, t.P2));
                        edges.Add(new Edge(t.P2, t.P3));
                        edges.Add(new Edge(t.P3, t.P1));
                        triangles.RemoveAt(j);
                    }
                }

                // Remove duplicate edges. This leaves the convex hull of the edges.
                // The edges in this convex hull are oriented counterclockwise!
                for (var j = edges.Count - 2; j >= 0; j--)
                {
                    for (var k = edges.Count - 1; k > j; k--)
                    {
                        if (edges[j] == edges[k])
                        {
                            edges.RemoveAt(k);
                            edges.RemoveAt(j);
                            k--;
                        }
                    }
                }

                // Generate new counterclockwise oriented triangles filling the "hole" in
                // the existing triangulation. These triangles all share the actual vertex.
                for (var j = 0; j < edges.Count; j++)
                {
                    triangles.Add(new Triangle(edges[j].StartPoint, edges[j].EndPoint, points[i]));
                }
            }

            // We don't want the supertriangle in the triangulation, so
            // remove all triangles sharing a vertex with the supertriangle.
            for (var i = triangles.Count - 1; i >= 0; i--)
            {
                if (triangles[i].SharesVertexWith(superTriangle))
                    triangles.RemoveAt(i);
            }
            return triangles;
        }

        public double Interpolate(double x, double y)
        {
            foreach (var tr in lDT)
            {
                if (IsInside(tr, x, y))
                {
                    var plane = new Plane(tr);
                    return (plane.W - x * plane.X - y * plane.Y) / plane.Z;
                }
            }

            foreach (var tr in lDT)
            {
                if (IsInside(tr, x, y, exact: false))
                {
                    var plane = new Plane(tr);
                    return (plane.W - x * plane.X - y * plane.Y) / plane.Z;
                }
            }

            throw new VectoException("Interpolation failed.");
        }

        public double InterpolateXZ(double x, double z)
        {
            foreach (var tr in lDTXZ)
            {
                if (IsInside(tr, x, z))
                {
                    var plane = new Plane(tr);
                    return (plane.W - x * plane.X - z * plane.Y) / plane.Z;
                }
            }

            foreach (var tr in lDTXZ)
            {
                if (IsInside(tr, x, z, exact: false))
                {
                    var plane = new Plane(tr);
                    return (plane.W - x * plane.X - z * plane.Y) / plane.Z;
                }
            }

            throw new VectoException("Interpolation failed.");
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

            const double tolerance = 0.001;
            return u + tolerance >= 0 & v + tolerance >= 0 & u + v <= 1 + tolerance;
        }



        private Triangle SuperTriangle(List<Point> triangulationPoints)
        {
            double num1 = triangulationPoints[0].X;
            int num2 = 1;
            int num3 = checked(triangulationPoints.Count - 1);
            int index = num2;
            while (index <= num3)
            {
                double num4 = Math.Abs(triangulationPoints[index].X);
                double num5 = Math.Abs(triangulationPoints[index].Y);
                if (num4 > num1)
                    num1 = num4;
                if (num5 > num1)
                    num1 = num5;
                checked { ++index; }
            }
            Point pp1 = new Point(10.0 * num1, 0.0, 0.0);
            Point pp2 = new Point(0.0, 10.0 * num1, 0.0);
            Point pp3 = new Point(-10.0 * num1, -10.0 * num1, 0.0);
            return new Triangle(ref pp1, ref pp2, ref pp3);
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
            return left.X == right.X && left.Y == right.Y;
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
        /// Ex-Product or Vectorial Product
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Point operator *(Point p1, Point p2)
        {
            return new Point(p1.Y * p2.Z - p1.Z * p2.Y,
                             p1.Z * p2.X - p1.X * p2.Z,
                             p1.X * p2.Y - p1.Y * p2.X);
        }

        public double DotProduct(Point p1)
        {
            return X * p1.X + Y * p1.Y + Z * p1.Z;
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
            var prod = (p2 - p1) * (p3 - p1);
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
            double num1 = P1.X - pt.X;
            double num2 = P1.Y - pt.Y;
            double num3 = P2.X - pt.X;
            double num4 = P2.Y - pt.Y;
            double num5 = P3.X - pt.X;
            double num6 = P3.Y - pt.Y;
            double num7 = num1 * num4 - num3 * num2;
            double num8 = num3 * num6 - num5 * num4;
            double num9 = num5 * num2 - num1 * num6;
            double num10 = num1 * num1 + num2 * num2;
            double num11 = num3 * num3 + num4 * num4;
            double num12 = num5 * num5 + num6 * num6;
            return num10 * num8 + num11 * num9 + num12 * num7;
        }

        public bool SharesVertexWith(Triangle triangle)
        {
            return P1.X == triangle.P1.X && P1.Y == triangle.P1.Y
                || P1.X == triangle.P2.X && P1.Y == triangle.P2.Y
                || (P1.X == triangle.P3.X && P1.Y == triangle.P3.Y || P2.X == triangle.P1.X && P2.Y == triangle.P1.Y)
                || (P2.X == triangle.P2.X && P2.Y == triangle.P2.Y
                    || P2.X == triangle.P3.X && P2.Y == triangle.P3.Y
                    || (P3.X == triangle.P1.X && P3.Y == triangle.P1.Y || P3.X == triangle.P2.X && P3.Y == triangle.P2.Y))
                || P3.X == triangle.P3.X && P3.Y == triangle.P3.Y;
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
