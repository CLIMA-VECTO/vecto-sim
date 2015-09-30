using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NLog;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public static class GraphWriter
	{
		private static bool _enabled = true;

		public static void Enabled()
		{
			_enabled = true;
		}

		public static void Disable()
		{
			_enabled = false;
		}


		public static void Write(string fileName)
		{
			if (!_enabled) {
				return;
			}

			var modDataV3 = VectoCSVFile.Read(fileName);

			var xfields = new[] { ModalResultField.time, ModalResultField.dist };

			var yfields = new[] {
				ModalResultField.v_act, ModalResultField.acc, ModalResultField.n, ModalResultField.Gear, ModalResultField.Pe_eng,
				ModalResultField.Tq_eng, ModalResultField.FCMap
			};

			var images = new List<Stream>();
			try {
				foreach (var xfield in xfields) {
					var x = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(xfield.GetName())).ToArray();

					for (var i = 1; i <= yfields.Length; i++) {
						var yfield = yfields[i - 1];
						var y = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(yfield.GetName())).ToArray();

						var values = string.Format("{0}|{1}", string.Join(",", x), string.Join(",", y));

						if (yfield == ModalResultField.v_act) {
							var y3 =
								modDataV3.Rows.Cast<DataRow>()
									.Select(v => v.Field<string>(ModalResultField.v_targ.GetName()))
									.Select(v => string.IsNullOrWhiteSpace(v) ? "0" : v);

							values += string.Format("|{0}|{1}|0|0", string.Join(",", x), string.Join(",", y3));
						}

						values = values.Replace("NaN", "0");
						if (values.Length > 14000) {
							// remove all decimal places to reduce request size
							values = Regex.Replace(values, @"\..*?,", ",");
						}
						var maxX = (int)Math.Ceiling(x.ToDouble().Max());
						images.Add(CreateGraphStream(xfield.GetCaption(), yfield.GetCaption(), maxX, values));
					}
					var outfileName = string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(fileName), xfield.GetName());
					SaveImages(outfileName, images.ToArray());
					images.Clear();
				}
			} finally {
				images.ForEach(x => x.Close());
			}
		}


		public static void Write(string fileNameV3, string fileNameV22)
		{
			if (!_enabled) {
				return;
			}

			var modDataV3 = VectoCSVFile.Read(fileNameV3);
			if (!File.Exists(fileNameV22)) {
				LogManager.GetCurrentClassLogger().Error("Modfile V2.2 not found: " + fileNameV22);
				Write(fileNameV3);
				return;
			}
			var modDataV22 = VectoCSVFile.Read(fileNameV22);

			var xfields = new[] { ModalResultField.time, ModalResultField.dist };

			var yfields = new[] {
				ModalResultField.v_act, ModalResultField.acc, ModalResultField.n, ModalResultField.Gear, ModalResultField.Pe_eng,
				ModalResultField.Tq_eng,
				ModalResultField.FCMap
			};

			var images = new List<Stream>();
			try {
				foreach (var xfield in xfields) {
					var x = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(xfield.GetName())).ToArray();
					var x2 = modDataV22.Rows.Cast<DataRow>().Select(v => v.Field<string>(xfield.GetName())).ToArray();

					for (var i = 1; i <= yfields.Length; i++) {
						var yfield = yfields[i - 1];
						var y = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(yfield.GetName())).ToArray();
						var y2 = modDataV22.Rows.Cast<DataRow>().Select(v => v.Field<string>(yfield.GetName())).ToArray();

						var values = string.Format("{0}|{1}|{2}|{3}", string.Join(",", x), string.Join(",", y), string.Join(",", x2),
							string.Join(",", y2));

						if (yfield == ModalResultField.v_act) {
							var y3 =
								modDataV3.Rows.Cast<DataRow>()
									.Select(v => v.Field<string>(ModalResultField.v_targ.GetName()))
									.Select(v => string.IsNullOrWhiteSpace(v) ? "0" : v);

							values += string.Format("|{0}|{1}", string.Join(",", x), string.Join(",", y3));
						}

						values = values.Replace("NaN", "0");
						if (values.Length > 14000) {
							// remove all decimal places to reduce request size
							values = Regex.Replace(values, @"(\.[0-9]).*?,", "$1,");
						}
						var maxX = (int)Math.Ceiling(Math.Max(x.ToDouble().Max(), x2.ToDouble().Max()));
						images.Add(CreateGraphStream(xfield.GetCaption(), yfield.GetCaption(), maxX, values));
					}
					var fileName = string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(fileNameV3), xfield.GetName());
					SaveImages(fileName, images.ToArray());
					images.Clear();
				}
			} finally {
				images.ForEach(x => x.Close());
			}
		}

		private static Stream CreateGraphStream(string xLabel, string yLabel, int xAxisRange, string values)
		{
			using (var client = new WebClient()) {
				var response = client.UploadValues("https://chart.googleapis.com/chart", new NameValueCollection {
					{ "cht", "lxy" },
					{ "chd", "t:" + values },
					{ "chs", "1000x230" },
					{ "chxt", "x,x,y,y" },
					{ "chds", "a" },
					{ "chxr", string.Format("0,0,{0},{1}", xAxisRange, xAxisRange / 10) },
					{ "chco", "0000FF,FF0000,00FF00" },
					{ "chg", "5,10" },
					{ "chxl", string.Format("1:|{0}|3:|{1}", xLabel, yLabel) },
					{ "chxp", "1,0|3,0" },
					{ "chdl", "V3|V2.2" },
				});

				return new MemoryStream(response);
			}
		}

		public static void SaveImages(string outputFile, params Stream[] inputFiles)
		{
			var titleHeight = 36;
			var images = new List<Image>();
			Bitmap output = null;
			Graphics g = null;

			try {
				images = inputFiles.Select(Image.FromStream).ToList();
				output = new Bitmap(images.Max(x => x.Width), images.Sum(x => x.Height) + titleHeight);
				g = Graphics.FromImage(output);
				g.TextRenderingHint = TextRenderingHint.AntiAlias;

				g.DrawString(outputFile, new Font("Arial", 16), Brushes.Black, output.Width / 2, 10,
					new StringFormat { Alignment = StringAlignment.Center });

				var ypos = titleHeight;
				foreach (var image in images) {
					g.DrawImage(image, new System.Drawing.Point(0, ypos));
					ypos += image.Height;
					image.Dispose();
				}
				output.Save(outputFile);
			} finally {
				images.ForEach(x => x.Dispose());
				if (output != null) {
					output.Dispose();
				}

				if (g != null) {
					g.Dispose();
				}
			}
		}
	}
}