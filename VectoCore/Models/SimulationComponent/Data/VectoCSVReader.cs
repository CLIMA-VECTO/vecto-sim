using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public static class VectoCSVReader
    {
        /// <summary>
        /// Reads a CSV file which is stored in Vecto-CSV-Format.
        /// </summary>
        /// <param name="fileName"></param>
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
            var cols = header.Split(',');

            var table = new DataTable();
            foreach (var col in cols)
                table.Columns.Add(col.Trim(), typeof(string));

            foreach (string line in lines.Skip(1))
            {
                //todo: do more sophisticated splitting of csv-columns (or use a good csv library!)
                table.Rows.Add(line.Split(','));
            }

            return table;
        }
    }
}