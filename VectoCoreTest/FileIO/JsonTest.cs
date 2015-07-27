using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO;

namespace TUGraz.VectoCore.Tests.FileIO
{
	[TestClass]
	public class JsonTest
	{
		[TestMethod]
		public void TestJsonHeaderEquality()
		{
			var h1 = new JsonDataHeader {
				AppVersion = "MyVecto3",
				CreatedBy = "UnitTest",
				Date = "1.1.1970",
				FileVersion = 3
			};
			var h2 = new JsonDataHeader {
				AppVersion = "MyVecto3",
				CreatedBy = "UnitTest",
				Date = "1.1.1970",
				FileVersion = 3
			};
			Assert.AreEqual(h1, h1);
			Assert.AreEqual(h1, h2);
			Assert.AreNotEqual(h1, null);
			Assert.AreNotEqual(h1, "hello world");
		}
	}
}