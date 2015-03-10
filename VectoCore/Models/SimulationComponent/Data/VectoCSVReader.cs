using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public static class VectoCSVReader
    {
        private const char Separator = ',';
        private const char Comment = '#';

        /// <summary>
        /// Reads a CSV file which is stored in Vecto-CSV-Format.
        /// </summary>
        /// <param name="fileName"></param>
        /// <exception cref="FileIOException"></exception>
        /// <returns>A DataTable which represents the CSV File.</returns>
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
        public static DataTable Read(string fileName)
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
                    throw new CSVReadException("The number of values is not correct. Line: " + i);

                try
                {
                    table.Rows.Add(line.Split(Separator));
                }
                catch (InvalidCastException e)
                {
                    throw new CSVReadException("The data format of a value is not correct. Line " + i, e);
                }
            }

            return table;
        }
    }
}