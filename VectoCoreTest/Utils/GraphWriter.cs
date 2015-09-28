using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public static class GraphWriter
	{
		public static void Write(string fileNameV3, string fileNameV22)
		{
			var modDataV3 = VectoCSVFile.Read(fileNameV3);
			var modDataV22 = VectoCSVFile.Read(fileNameV22);

			var xfields = new[] { ModalResultField.time, ModalResultField.dist };

			var yfields = new[] {
				ModalResultField.v_act, ModalResultField.n, ModalResultField.Gear, ModalResultField.Pe_eng, ModalResultField.Tq_eng,
				ModalResultField.FCMap
			};

			foreach (var xfield in xfields) {
				for (var i = 1; i <= yfields.Length; i++) {
					var yfield = yfields[i - 1];
					var x = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(ModalResultField.time.GetName())).ToArray();
					var y = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(yfield.GetName())).ToArray();

					var x2 = modDataV22.Rows.Cast<DataRow>().Select(v => v.Field<string>(ModalResultField.time.GetName())).ToArray();
					var y2 = modDataV22.Rows.Cast<DataRow>().Select(v => v.Field<string>(yfield.GetName())).ToArray();


					var fileName = string.Format("{0}_{1}_{2}.png", Path.GetFileNameWithoutExtension(fileNameV3), xfield, i);
					var values = string.Format("{0}|{1}|{2}|{3}", string.Join(",", x), string.Join(",", y), string.Join(",", x2),
						string.Join(",", y2));

					var maxX = (int)Math.Ceiling(Math.Max(x.ToDouble().Max(), x2.ToDouble().Max()));
					CreateGraphFile(fileName, xfield.GetCaption(), yfield.GetCaption(), maxX, values);
				}
			}
		}

		private static void CreateGraphFile(string filename, string xLabel, string yLabel, int xAxisRange, string values)
		{
			using (var client = new WebClient()) {
				byte[] response = client.UploadValues("https://chart.googleapis.com/chart", new NameValueCollection {
					{ "cht", "lxy" },
					{ "chd", "t:" + values },
					{ "chs", "1000x300" },
					{ "chxt", "x,x,y,y" },
					{ "chds", "a" },
					{ "chxr", string.Format("0,0,{0},10", xAxisRange) },
					{ "chco", "0000FF,FF0000" },
					{ "chg", "5,10" },
					{ "chxl", string.Format("1:|{0}|3:|{1}", xLabel, yLabel) },
					{ "chxp", "1,0|3,0" },
					{ "chdl", "V3|V2.2" },
				});

				File.WriteAllBytes(filename, response);
			}
		}
	}
}