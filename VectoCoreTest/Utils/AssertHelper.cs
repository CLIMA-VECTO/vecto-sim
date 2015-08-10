using System;
using System.Diagnostics;
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
					Assert.AreEqual(message, ex.Message);
				}
			}
		}

		[DebuggerHidden]
		public static void AreRelativeEqual(SI expected, SI actual)
		{
			Assert.IsTrue(actual.HasEqualUnit(expected),
				string.Format("Wrong SI Units: expected: {0}, actual: {1}", expected.ToBasicUnits(), actual.ToBasicUnits()));
			AreRelativeEqual(expected.Value(), actual.Value());
		}

		[DebuggerHidden]
		public static void AreRelativeEqual(double expected, double actual, string message = null,
			double toleranceFactor = DoubleExtensionMethods.Tolerance)
		{
			if (!string.IsNullOrWhiteSpace(message)) {
				message = "\n" + message;
			} else {
				message = "";
			}

			if (double.IsNaN(expected)) {
				Assert.IsTrue(double.IsNaN(actual),
					string.Format("Actual value is not NaN. Expected: {0}, Actual: {1}{2}", expected, actual, message));
				return;
			}

			if (expected.IsEqual(0.0)) {
				Assert.AreEqual(expected, actual, DoubleExtensionMethods.Tolerance,
					string.Format("Actual value is different. Expected: {0}, Actual: {1}, Difference: {3}, ToleranceFactor: {2}{4}",
						expected, actual, toleranceFactor, expected - actual, message));
				return;
			}

			Assert.IsTrue(Math.Abs(actual / expected - 1) < toleranceFactor,
				string.Format("Actual value is different. Expected: {0}, Actual: {1}, Difference: {3}, ToleranceFactor: {2}{4}",
					expected, actual, toleranceFactor, expected - actual, message));
		}
	}
}