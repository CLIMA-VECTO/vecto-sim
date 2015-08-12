﻿using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class AssertHelper
	{
		/// <summary>
		/// Assert an expected Exception.
		/// </summary>
		[DebuggerHidden]
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
		public static void AreRelativeEqual(SI expected, SI actual,
			double toleranceFactor = DoubleExtensionMethods.ToleranceFactor)
		{
			Assert.IsTrue(actual.HasEqualUnit(expected),
				string.Format("Wrong SI Units: expected: {0}, actual: {1}", expected.ToBasicUnits(), actual.ToBasicUnits()));
			AreRelativeEqual(expected.Value(), actual.Value(), toleranceFactor: toleranceFactor);
		}

		[DebuggerHidden]
		public static void AreRelativeEqual(Scalar expected, Scalar actual,
			double toleranceFactor = DoubleExtensionMethods.ToleranceFactor)
		{
			Assert.IsTrue(expected.HasEqualUnit(new SI()) && actual.HasEqualUnit(new SI()), "Units of Scalars must be empty.");
			AreRelativeEqual(expected.Value(), actual.Value(), toleranceFactor: toleranceFactor);
		}

		[DebuggerHidden]
		public static void AreRelativeEqual(double expected, double actual, string message = null,
			double toleranceFactor = DoubleExtensionMethods.ToleranceFactor)
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

			var ratio = expected == 0 ? Math.Abs(actual) : Math.Abs(actual / expected - 1);
			Assert.IsTrue(ratio < toleranceFactor, string.Format(CultureInfo.InvariantCulture,
				"Given values are not equal. Expected: {0}, Actual: {1}, Difference: {3} (Tolerance Factor: {2}){4}",
				expected, actual, toleranceFactor, expected - actual, message));
		}
	}
}