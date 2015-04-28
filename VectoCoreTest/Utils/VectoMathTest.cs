using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	[TestClass]
	public class VectoMathTest
	{
		[TestMethod]
		public void VectoMath_Min()
		{
			var smaller = 0.SI();
			var bigger = 5.SI();
			var negative = -1 * 10.SI();
			var positive = 10.SI();
			Assert.AreEqual(smaller, VectoMath.Min(smaller, bigger));

			Assert.AreEqual(bigger, VectoMath.Max(smaller, bigger));

			Assert.AreEqual(positive, VectoMath.Abs(negative));
			Assert.AreEqual(positive, VectoMath.Abs(positive));


			var smallerWatt = 0.SI<Watt>();
			var biggerWatt = 5.SI<Watt>();
			var negativeWatt = -10.SI<Watt>();
			var positiveWatt = 10.SI<Watt>();
			Assert.AreEqual(smallerWatt, VectoMath.Min(smallerWatt, biggerWatt));

			Assert.AreEqual(biggerWatt, VectoMath.Max(smallerWatt, biggerWatt));


			Assert.AreEqual(positiveWatt, VectoMath.Abs(negativeWatt));
			Assert.AreEqual(positiveWatt, VectoMath.Abs(positiveWatt));
		}
	}
}