using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.Utils;

namespace DeclarationCycleZip
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(args[0]);

			var table = new DataTable();
			table.Columns.Add("<s>");
			table.Columns.Add("<v>");
			table.Columns.Add("<grad>");
			table.Columns.Add("<stop>");

			var lastDistance = cycleData.Entries.First().Distance - 1.SI<Meter>();
			foreach (var x in cycleData.Entries) {
				if (x.Distance.IsEqual(lastDistance)) {
					continue;
				}
				var row = table.NewRow();
				row["<s>"] = x.Distance.Value();
				row["<v>"] = x.VehicleTargetSpeed.ConvertTo().Kilo.Meter.Per.Hour.Value();
				row["<grad>"] = x.RoadGradientPercent;
				row["<stop>"] = x.StoppingTime.Value();
				table.Rows.Add(row);
				lastDistance = x.Distance;
			}

			VectoCSVFile.Write(Path.GetFileName(args[0]), table);
		}
	}
}