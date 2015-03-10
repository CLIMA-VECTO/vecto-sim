using System;
using System.Collections.Generic;

namespace TUGraz.VectoCore.Utils
{
    class DelauneyMap
    {
        public int ptDim;
        public List<Point> ptList;
        private List<Triangle> lDT;
        private List<double[]> planes;
        public bool DualMode;
        private List<Point> ptListXZ;
        private List<double[]> planesXZ;
        private List<Triangle> lDTXZ;
        public bool ExtrapolError;

        public DelauneyMap()
        {
            ptList = new List<Point>();
            ptListXZ = new List<Point>();
            DualMode = false;
        }

        public void AddPoints(double x, double y, double z)
        {
            ptList.Add(new Point(x, y, z));
            if (DualMode)
                ptListXZ.Add(new Point(x, z, y));
        }

        public void Triangulate()
        {
            ptDim = ptList.Count - 1;
            lDT = new dTriangulation().Triangulate(ptList);
            planes = new List<double[]>();
            foreach (var tr in lDT)
                planes.Add(GetPlane(tr));

            if (DualMode)
            {
                if (ptDim != ptListXZ.Count - 1)
                    throw new Exception("ptDim != ptListXZ.Count - 1");

                lDTXZ = new dTriangulation().Triangulate(ptListXZ);
                planesXZ = new List<double[]>();
                foreach (var tr in lDTXZ)
                    planesXZ.Add(GetPlane(tr));
            }
        }

        public double Intpol(double x, double y)
        {
            //ExtrapolError = false;
            //int index1 = -1;
            //List<Triangle>.Enumerator enumerator1;
            //try
            //{
            //    enumerator1 = lDT.GetEnumerator();
            //    while (enumerator1.MoveNext())
            //    {
            //        Triangle current = enumerator1.Current;
            //        checked { ++index1; }
            //        if (IsInside(ref current, x, y, true))
            //        {
            //            double[] numArray = planes[index1];
            //            return (numArray[3] - x * numArray[0] - y * numArray[1]) / numArray[2];
            //        }
            //    }
            //}
            //finally
            //{
            //    enumerator1.Dispose();
            //}
            //int index2 = -1;
            //List<Triangle>.Enumerator enumerator2;
            //try
            //{
            //    enumerator2 = lDT.GetEnumerator();
            //    while (enumerator2.MoveNext())
            //    {
            //        Triangle current = enumerator2.Current;
            //        checked { ++index2; }
            //        if (IsInside(ref current, x, y, false))
            //        {
            //            double[] numArray = planes[index2];
            //            return (numArray[3] - x * numArray[0] - y * numArray[1]) / numArray[2];
            //        }
            //    }
            //}
            //finally
            //{
            //    enumerator2.Dispose();
            //}
            //ExtrapolError = true;
            return 0.0;
        }

        public double IntpolXZ(double x, double z)
        {
            //ExtrapolError = false;
            //if (DualMode)
            //{
            //    int index1 = -1;
            //    List<Triangle>.Enumerator enumerator1;
            //    try
            //    {
            //        enumerator1 = lDTXZ.GetEnumerator();
            //        while (enumerator1.MoveNext())
            //        {
            //            Triangle current = enumerator1.Current;
            //            checked { ++index1; }
            //            if (IsInside(ref current, x, z, true))
            //            {
            //                double[] numArray = planesXZ[index1];
            //                return (numArray[3] - x * numArray[0] - z * numArray[1]) / numArray[2];
            //            }
            //        }
            //    }
            //    finally
            //    {
            //        enumerator1.Dispose();
            //    }
            //    int index2 = -1;
            //    List<Triangle>.Enumerator enumerator2;
            //    try
            //    {
            //        enumerator2 = lDTXZ.GetEnumerator();
            //        while (enumerator2.MoveNext())
            //        {
            //            Triangle current = enumerator2.Current;
            //            checked { ++index2; }
            //            if (IsInside(ref current, x, z, false))
            //            {
            //                double[] numArray = planesXZ[index2];
            //                return (numArray[3] - x * numArray[0] - z * numArray[1]) / numArray[2];
            //            }
            //        }
            //    }
            //    finally
            //    {
            //        enumerator2.Dispose();
            //    }
            //    ExtrapolError = true;
            //    return 0.0;
            //}
            //ExtrapolError = true;
            return 0.0;
        }

        private double[] GetPlane(Triangle tr)
        {
            Point p1 = tr.P1;
            Point p2 = tr.P2;
            Point p3 = tr.P3;

            Point point4 = p2 - p1;
            Point point5 = p3 - p1;

            Point point6 = point4 * point5;

            double[] numArray = { point6.X, point6.Y, point6.Z, p1.X * point6.X + p1.Y * point6.Y + p1.Z * point6.Z };
            return numArray;
        }

        private bool IsInside(ref Triangle tr, double xges, double yges, bool Exact)
        {
            double[] numArray1 = new double[2];
            double[] numArray2 = new double[2];
            double[] numArray3 = new double[2];
            Point point1 = tr.P1;
            Point point2 = tr.P2;
            Point point3 = tr.P3;
            numArray1[0] = point3.X - point1.X;
            numArray1[1] = point3.Y - point1.Y;
            numArray2[0] = point2.X - point1.X;
            numArray2[1] = point2.Y - point1.Y;
            numArray3[0] = xges - point1.X;
            numArray3[1] = yges - point1.Y;
            double num1 = numArray1[0] * numArray1[0] + numArray1[1] * numArray1[1];
            double num2 = numArray1[0] * numArray2[0] + numArray1[1] * numArray2[1];
            double num3 = numArray1[0] * numArray3[0] + numArray1[1] * numArray3[1];
            double num4 = numArray2[0] * numArray2[0] + numArray2[1] * numArray2[1];
            double num5 = numArray2[0] * numArray3[0] + numArray2[1] * numArray3[1];
            double num6 = 1.0 / (num1 * num4 - num2 * num2);
            double num7 = (num4 * num3 - num2 * num5) * num6;
            double num8 = (num1 * num5 - num2 * num3) * num6;
            if (Exact)
                return num7 >= 0.0 & num8 >= 0.0 & num7 + num8 <= 1.0;
            return num7 >= -0.001 & num8 >= -0.001 & num7 + num8 <= 1001.0 / 1000.0;
        }


    }

    public class Point
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
    }

    public class Triangle
    {
        public Point P1;
        public Point P2;
        public Point P3;

        public Triangle(ref Point pp1, ref Point pp2, ref Point pp3)
        {
            P1 = pp1;
            P2 = pp2;
            P3 = pp3;
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
            return P1.X == triangle.P1.X && P1.Y == triangle.P1.Y || P1.X == triangle.P2.X && P1.Y == triangle.P2.Y || (P1.X == triangle.P3.X && P1.Y == triangle.P3.Y || P2.X == triangle.P1.X && P2.Y == triangle.P1.Y) || (P2.X == triangle.P2.X && P2.Y == triangle.P2.Y || P2.X == triangle.P3.X && P2.Y == triangle.P3.Y || (P3.X == triangle.P1.X && P3.Y == triangle.P1.Y || P3.X == triangle.P2.X && P3.Y == triangle.P2.Y)) || P3.X == triangle.P3.X && P3.Y == triangle.P3.Y;
        }
    }

    public class dEdge
    {
        public Point StartPoint;
        public Point EndPoint;

        public dEdge(ref Point p1, ref Point p2)
        {
            StartPoint = p1;
            EndPoint = p2;
        }

        public static bool operator ==(dEdge left, dEdge right)
        {
            return left.StartPoint == right.StartPoint && left.EndPoint == right.EndPoint || left.StartPoint == right.EndPoint && left.EndPoint == right.StartPoint;
        }

        public static bool operator !=(dEdge left, dEdge right)
        {
            return !(left == right);
        }
    }

    public class dTriangulation
    {

        public List<Triangle> Triangulate(List<Point> triangulationPoints)
        {
            if (triangulationPoints.Count < 3)
                throw new ArgumentException("Can not triangulate less than three vertices!");

            List<Triangle> list1 = new List<Triangle>();

            Triangle superTriangle = SuperTriangle(triangulationPoints);
            list1.Add(superTriangle);

            for (int index1 = 0; index1 < triangulationPoints.Count; index1++)
            {
                List<dEdge> list2 = new List<dEdge>();
                int index2 = checked(list1.Count - 1);
                while (index2 >= 0)
                {
                    Triangle triangle = list1[index2];
                    if (triangle.ContainsInCircumcircle(triangulationPoints[index1]) > 0.0)
                    {
                        list2.Add(new dEdge(ref triangle.P1, ref triangle.P2));
                        list2.Add(new dEdge(ref triangle.P2, ref triangle.P3));
                        list2.Add(new dEdge(ref triangle.P3, ref triangle.P1));
                        list1.RemoveAt(index2);
                    }
                    checked { index2 += -1; }
                }
                int index3 = checked(list2.Count - 2);
                while (index3 >= 0)
                {
                    int num3 = checked(list2.Count - 1);
                    int num4 = checked(index3 + 1);
                    int index4 = num3;
                    while (index4 >= num4)
                    {
                        if (list2[index3] == list2[index4])
                        {
                            list2.RemoveAt(index4);
                            list2.RemoveAt(index3);
                            checked { --index4; }
                        }
                        checked { index4 += -1; }
                    }
                    checked { index3 += -1; }
                }
                int num5 = 0;
                int num6 = checked(list2.Count - 1);
                int index5 = num5;
                while (index5 <= num6)
                {
                    List<Triangle> list3 = list1;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    Point pp1 = @list2[index5].StartPoint;
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    Point pp2 = @list2[index5].EndPoint;
                    List<Point> list4 = triangulationPoints;
                    List<Point> list5 = list4;
                    int index4 = index1;
                    int index6 = index4;
                    Point point = list5[index6];
                    // ISSUE: explicit reference operation
                    // ISSUE: variable of a reference type
                    Point pp3 = point;
                    Triangle triangle = new Triangle(ref pp1, ref pp2, ref pp3);
                    list4[index4] = point;
                    list3.Add(triangle);
                    checked { ++index5; }
                }
                checked { ++index1; }
            }
            int index7 = checked(list1.Count - 1);
            while (index7 >= 0)
            {
                if (list1[index7].SharesVertexWith(superTriangle))
                    list1.RemoveAt(index7);
                checked { index7 += -1; }
            }
            return list1;
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
}
