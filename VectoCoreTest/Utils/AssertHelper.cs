using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class AssertHelper
	{
		/// <summary>
		/// Assert an expected Exception.
		/// </summary>
		public static void Exception<T>(Action func, string message = null) where T : Exception
		{
			try {
				func();
				Assert.Fail("Expected Exception {0}, but no exception occured.", typeof(T));
			} catch (T ex) {
				if (!string.IsNullOrEmpty(message)) {
					Assert.AreEqual(message, ex.Message,
						string.Format("Expected Exception message: {0}, but got message: {1}", message, ex.Message));
				}
			}
		}

		public static void AreEqual(double expected, double actual,
			double toleranceFactor = DoubleExtensionMethods.Tolerance)
		{
			if (actual.IsEqual(0.0)) {
				Assert.AreEqual(expected, 0.0, DoubleExtensionMethods.Tolerance,
					string.Format("AssertHelper.AreEqual failed. Expected: {0}, Actual: {1}, Tolerance: {2}", expected, actual,
						toleranceFactor));
				return;
			}
			Assert.IsTrue(Math.Abs(expected / actual) < 1 + toleranceFactor,
				string.Format("AssertHelper.AreEqual failed. Expected: {0}, Actual: {1}, Tolerance: {2}", expected, actual,
					toleranceFactor));
		}
	}
}