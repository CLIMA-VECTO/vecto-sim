using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    /// <summary>
    /// Class for Reading and Writing VECTO CSV Files.
    /// </summary>
    /// <remarks>
    /// The following format applies to all CSV (Comma-separated values) Input Files used in VECTO: 
    /// List Separator: Comma "," 
    /// Decimal-Mark: Dot "." 
    /// Comments: "#" at the beginning of the comment line. Number and position of comment lines is not limited. 
    /// Header: One header line (not a comment line) at the beginning of the file. 
    ///         All Combinations between max-format and min-format possible. Only "id"-field is used.
    ///         max: <id> (name) [unit], <id> (name) [unit], ... 
    ///         min: id,id,...
    /// </remarks>
    public static class VectoCSVFile
    {
        private const char Separator = ',';
        private const char Comment = '#';

        /// <summary>
        /// Reads a CSV file which is stored in Vecto-CSV-Format.
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="FileIOException"></exception>
        /// <returns>A DataTable which represents the CSV File.</returns>

        public static DataTable Read(string fileName)
        {
            try
            {
                var lines = File.ReadAllLines(fileName);
                var header = lines.First();
                header = Regex.Replace(header, @"\[.*?\]", "");
                header = Regex.Replace(header, @"\(.*?\)", "");
                header = header.Replace("<", "");
                header = header.Replace(">", "");
                // or all in one regex (incl. trim):
                // Regex.Replace(header, @"\s*\[.*?\]\s*|\s*\(.*?\)\s*|\s*<|>\s*|\s*(?=,)|(?<=,)\s*", "");
                var cols = header.Split(Separator);

                var table = new DataTable();
                foreach (var col in cols)
                    table.Columns.Add(col.Trim(), typeof(string));

                // skip header! --> begin with index 1
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    //todo: do more sophisticated splitting of csv-columns (or use a good csv library!)

                    if (line.Contains(Comment))
                        line = line.Substring(0, line.IndexOf(Comment));

                    var cells = line.Split(Separator);
                    if (cells.Length != cols.Length)
                        throw new CSVReadException(string.Format("Line {0}: The number of values is not correct.", i));

                    try
                    {
                        table.Rows.Add(line.Split(Separator));
                    }
                    catch (InvalidCastException e)
                    {
                        throw new CSVReadException(string.Format("Line {0}: The data format of a value is not correct. {1}", i, e.Message), e);
                    }
                }

                return table;
            }
            catch (Exception e)
            {
                throw new VectoException(string.Format("File {0}: {1}", fileName, e.Message));
            }
        }

        public static void Write(string fileName, DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            var header = table.Columns.Cast<DataColumn>().Select(col => col.Caption ?? col.ColumnName);
            sb.AppendLine(string.Join(", ", header));

            foreach (DataRow row in table.Rows)
            {
                List<string> formattedList = new List<string>();
                
                foreach (var item in row.ItemArray)
                {
                    var formattable = item as IFormattable;
                    formattedList.Add(formattable != null
                                      ? formattable.ToString("", CultureInfo.InvariantCulture)
                                      : item.ToString());
                }

                var line = string.Join(Separator.ToString(), formattedList);
                sb.AppendLine(line);
            }

            File.WriteAllText(fileName, sb.ToString());
        }
    }
}