using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	[TestClass]
	public class DelauneyMapTest
	{
		[TestMethod]
		public void Test_Simple_DelauneyMap()
		{
			var map = new DelauneyMap();
			map.AddPoint(0, 0, 0);
			map.AddPoint(1, 0, 0);
			map.AddPoint(0, 1, 0);

			map.Triangulate();

			var result = map.Interpolate(0.25, 0.25);

			AssertHelper.AreRelativeEqual(0, result);
		}

		[TestMethod]
		public void Test_DelauneyMapTriangle()
		{
			var map = new DelauneyMap();
			map.AddPoint(0, 0, 0);
			map.AddPoint(1, 0, 1);
			map.AddPoint(0, 1, 2);

			map.Triangulate();

			// fixed points
			AssertHelper.AreRelativeEqual(0, map.Interpolate(0, 0));
			AssertHelper.AreRelativeEqual(1, map.Interpolate(1, 0));
			AssertHelper.AreRelativeEqual(2, map.Interpolate(0, 1));

			// interpolations
			AssertHelper.AreRelativeEqual(0.5, map.Interpolate(0.5, 0));
			AssertHelper.AreRelativeEqual(1, map.Interpolate(0, 0.5));
			AssertHelper.AreRelativeEqual(1.5, map.Interpolate(0.5, 0.5));

			AssertHelper.AreRelativeEqual(0.25, map.Interpolate(0.25, 0));
			AssertHelper.AreRelativeEqual(0.5, map.Interpolate(0, 0.25));
			AssertHelper.AreRelativeEqual(0.75, map.Interpolate(0.25, 0.25));

			AssertHelper.AreRelativeEqual(0.75, map.Interpolate(0.75, 0));
			AssertHelper.AreRelativeEqual(1.5, map.Interpolate(0, 0.75));

			// extrapolation (should fail)
			AssertHelper.Exception<VectoException>(() => map.Interpolate(1, 1), "Interpolation failed. x: 1, y: 1");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(-1, -1), "Interpolation failed. x: -1, y: -1");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(1, -1), "Interpolation failed. x: 1, y: -1");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(-1, 1), "Interpolation failed. x: -1, y: 1");
		}

		public void Test_DelauneyMapPlane()
		{
			var map = new DelauneyMap();
			map.AddPoint(0, 0, 0);
			map.AddPoint(1, 0, 1);
			map.AddPoint(0, 1, 2);
			map.AddPoint(1, 1, 3);

			map.Triangulate();

			// fixed points
			AssertHelper.AreRelativeEqual(0, map.Interpolate(0, 0));
			AssertHelper.AreRelativeEqual(1, map.Interpolate(1, 0));
			AssertHelper.AreRelativeEqual(2, map.Interpolate(0, 1));
			AssertHelper.AreRelativeEqual(3, map.Interpolate(1, 1));

			// interpolations
			AssertHelper.AreRelativeEqual(0.5, map.Interpolate(0.5, 0));
			AssertHelper.AreRelativeEqual(1, map.Interpolate(0, 0.5));
			AssertHelper.AreRelativeEqual(2, map.Interpolate(1, 0.5));
			AssertHelper.AreRelativeEqual(2.5, map.Interpolate(0.5, 1));

			AssertHelper.AreRelativeEqual(1.5, map.Interpolate(0.5, 0.5));

			AssertHelper.AreRelativeEqual(0.75, map.Interpolate(0.25, 0.25));
			AssertHelper.AreRelativeEqual(2.25, map.Interpolate(0.75, 0.75));

			AssertHelper.AreRelativeEqual(1.75, map.Interpolate(0.25, 0.75));
			AssertHelper.AreRelativeEqual(1.25, map.Interpolate(0.75, 0.25));

			// extrapolation (should fail)
			AssertHelper.Exception<VectoException>(() => map.Interpolate(1.5, 0.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(1.5, 1.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(0.5, 1.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(-0.5, 1.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(-0.5, 0.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(-1.5, -1.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(0.5, -0.5), "Interpolation failed.");
			AssertHelper.Exception<VectoException>(() => map.Interpolate(-1.5, -0.5), "Interpolation failed.");
		}

		[TestMethod]
		public void Test_Delauney_LessThan3Points()
		{
			AssertHelper.Exception<ArgumentException>(() => new DelauneyMap().Triangulate(),
				"Triangulation needs at least 3 Points. Got 0 Points.");

			AssertHelper.Exception<ArgumentException>(() => {
				var map1 = new DelauneyMap();
				map1.AddPoint(0, 0, 0);
				map1.Triangulate();
			}, "Triangulation needs at least 3 Points. Got 1 Points.");

			AssertHelper.Exception<ArgumentException>(() => {
				var map2 = new DelauneyMap();
				map2.AddPoint(0, 0, 0);
				map2.AddPoint(0, 0, 0);
				map2.Triangulate();
			}, "Triangulation needs at least 3 Points. Got 2 Points.");

			var map = new DelauneyMap();
			map.AddPoint(0, 0, 0);
			map.AddPoint(1, 0, 0);
			map.AddPoint(0, 1, 0);
			map.Triangulate();
		}
	}
}