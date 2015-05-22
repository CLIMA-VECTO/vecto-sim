using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Declaration;

namespace TUGraz.VectoCore.Tests.Models.DeclarationData
{
	[TestClass]
	public class DeclarationDataTest
	{
		public const double Tolerance = 0.0001;

		[TestMethod]
		public void WheelDataTest()
		{
			var wheels = VectoCore.Models.DeclarationData.Instance().Wheels;

			var tmp = wheels.Lookup("285/70 R 19.5");

			Assert.AreEqual(7.9, tmp.Inertia.Double(), Tolerance);
			Assert.AreEqual(0.8943, tmp.DynamicTyreRadius.Double(), Tolerance);
			Assert.AreEqual(0, tmp.SizeClass);
		}

		[TestMethod]
		public void RimsDataTest()
		{
			var rims = VectoCore.Models.DeclarationData.Instance().Rims;

			var tmp = rims.Lookup("15° DC Rims");

			Assert.AreEqual(3.03, tmp.F_a, Tolerance);
			Assert.AreEqual(3.05, tmp.F_b, Tolerance);
		}
	}
}