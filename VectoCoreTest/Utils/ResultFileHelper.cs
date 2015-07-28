using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public static class ResultFileHelper
	{
		public static void TestModFile(string expectedFile, string actualFile)
		{
			TestModFiles(new[] { expectedFile }, new[] { actualFile });
		}

		public static void TestModFiles(IEnumerable<string> expectedFiles, IEnumerable<string> actualFiles)
		{
			var resultFiles = expectedFiles.ZipAll(actualFiles, (expectedFile, actualFile) => new { expectedFile, actualFile });
			foreach (var result in resultFiles) {
				Assert.IsTrue(File.Exists(result.actualFile), "MOD File is missing: " + result);
				Assert.IsTrue(File.Exists(result.expectedFile), "Expected File is missing: " + result);

				var expected = VectoCSVFile.Read(result.expectedFile);
				var actual = VectoCSVFile.Read(result.actualFile);

				Assert.AreEqual(expected.Rows.Count, actual.Rows.Count,
					string.Format("Moddata: Row count differs.\nExpected {0} Rows in {1}\nGot {2} Rows in {3}", expected.Rows.Count,
						result.expectedFile, actual.Rows.Count, result.actualFile));

				var actualCols = actual.Columns.Cast<DataColumn>().Select(x => x.ColumnName).OrderBy(x => x).ToList();
				var expectedCols = expected.Columns.Cast<DataColumn>().Select(x => x.ColumnName).OrderBy(x => x).ToList();

				Assert.IsTrue(expectedCols.SequenceEqual(actualCols),
					string.Format("Moddata: Columns differ:\nExpected: {0}\nActual: {1}", string.Join(", ", expectedCols),
						string.Join(", ", actualCols)));

				// todo: Test Contents of MOD File.
			}
		}

		public static void TestSumFile(string expectedFile, string actualFile)
		{
			Assert.IsTrue(File.Exists(actualFile), "SUM File is missing: " + actualFile);

			var expected = File.ReadAllLines(expectedFile);
			var actual = File.ReadAllLines(actualFile);

			Assert.AreEqual(expected.Length, actual.Length,
				string.Format("SUM File row count differs.\nExpected {0} Rows in {1}\nGot {2} Rows in {3}", expected.Length,
					expectedFile, actual.Length, actualFile));

			Assert.AreEqual(expected.First(), actual.First(),
				string.Format("SUM File Header differs:\nExpected: '{0}'\nActual  : '{1}'", expected.First(), actual.First()));

			// todo: test contents of sum file
		}
	}
}