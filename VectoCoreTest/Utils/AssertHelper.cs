using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
						string.Format("Expected Exception message: {0}, but got message: {}", message, ex.Message));
				}
			}
		}
	}
}