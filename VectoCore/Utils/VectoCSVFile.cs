using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	/// <summary>
	///     Class for Reading and Writing VECTO CSV Files.
	/// </summary>
	/// <remarks>
	///     The following format applies to all CSV (Comma-separated values) Input Files used in VECTO:
	///     List DELIMITER: Comma ","
	///     Decimal-Mark: Dot "."
	///     Comments: "#" at the beginning of the comment line. Number and position of comment lines is not limited.
	///     Header: One header line (not a comment line) at the beginning of the file.
	///     All Combinations between max-format and min-format possible. Only "id"-field is used.
	///     max: id (name) [unit], id (name) [unit], ...
	///     min: id,id,...
	/// </remarks>
	public static class VectoCSVFile
	{
		private const char Delimiter = ',';
		private const char Comment = '#';

		/// <summary>
		/// Reads a CSV file which is stored in Vecto-CSV-Format.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="ignoreEmptyColumns"></param>
		/// <exception cref="FileIOException"></exception>
		/// <returns>A DataTable which represents the CSV File.</returns>
		public static DataTable Read(string fileName, bool ignoreEmptyColumns = false)
		{
			try {
				var lines = File.ReadAllLines(fileName);
				lines = RemoveComments(lines);

				var validColumns = GetValidHeaderColumns(lines.First());

				if (validColumns.Length > 0) {
					// Valid Columns found => header was valid => skip header line
					lines = lines.Skip(1).ToArray();
				} else {
					var log = LogManager.GetLogger(typeof(VectoCSVFile));
					log.Warn("No valid Data Header found. Interpreting the first line as data line.");
					// set the validColumns to: {"0", "1", "2", "3", ...} for all columns in first line.
					validColumns = GetColumns(lines.First()).Select((_, index) => index.ToString()).ToArray();
				}

				var table = new DataTable();
				foreach (var col in validColumns) {
					table.Columns.Add(col);
				}

				for (var i = 0; i < lines.Length; i++) {
					var line = lines[i];

					var cells = line.Split(Delimiter);
					if (!ignoreEmptyColumns && cells.Length != table.Columns.Count) {
						throw new CSVReadException(string.Format("Line {0}: The number of values is not correct.", i));
					}

					try {
						table.Rows.Add(line.Split(Delimiter));
					} catch (InvalidCastException e) {
						throw new CSVReadException(
							string.Format("Line {0}: The data format of a value is not correct. {1}", i, e.Message), e);
					}
				}

				return table;
			} catch (Exception e) {
				throw new VectoException(string.Format("File {0}: {1}", fileName, e.Message));
			}
		}

		private static string[] GetValidHeaderColumns(string line)
		{
			Contract.Requires(line != null);
			double test;
			var validColumns = GetColumns(line).
				Where(col => !double.TryParse(col, NumberStyles.Any, CultureInfo.InvariantCulture, out test));
			return validColumns.ToArray();
		}

		private static IEnumerable<string> GetColumns(string line)
		{
			Contract.Requires(line != null);

			line = Regex.Replace(line, @"\[.*?\]", "");
			line = Regex.Replace(line, @"\(.*?\)", "");
			line = line.Replace("<", "");
			line = line.Replace(">", "");
			return line.Split(Delimiter).Select(col => col.Trim());
		}

		private static string[] RemoveComments(string[] lines)
		{
			Contract.Requires(lines != null);

			lines = lines.
				Select(line => line.Contains('#') ? line.Substring(0, line.IndexOf(Comment)) : line).
				Where(line => !string.IsNullOrEmpty(line)).
				ToArray();
			return lines;
		}

		/// <summary>
		/// Writes the datatable to the csv file.
		/// Uses the column caption as header (with fallback to column name) for the csv header.
		/// </summary>
		/// <param name="fileName">Path to the file.</param>
		/// <param name="table">The Datatable.</param>
		public static void Write(string fileName, DataTable table)
		{
			var sb = new StringBuilder();

			var header = table.Columns.Cast<DataColumn>().Select(col => col.Caption ?? col.ColumnName);
			sb.AppendLine(string.Join(Delimiter.ToString(), header));

			foreach (DataRow row in table.Rows) {
				var formattedList = new List<string>();
				foreach (var item in row.ItemArray) {
					var formattable = item as IFormattable;
					var formattedValue = formattable != null
						? formattable.ToString("", CultureInfo.InvariantCulture)
						: item.ToString();
					formattedList.Add(formattedValue);
				}
				sb.AppendLine(string.Join(Delimiter.ToString(), formattedList));
			}

			File.WriteAllText(fileName, sb.ToString());
		}
	}
}