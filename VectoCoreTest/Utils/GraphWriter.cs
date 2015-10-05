using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;
using Point = System.Drawing.Point;

namespace TUGraz.VectoCore.Tests.Utils
{
	public static class GraphWriter
	{
		private static bool _enabled = true;

		private static Size diagramSize = new Size(2000, 440);

		public static void Enabled()
		{
			_enabled = true;
		}

		public static void Disable()
		{
			_enabled = false;
		}

		public static void Write(string fileNameV3, string fileNameV22)
		{
			if (!_enabled) {
				return;
			}

			var modDataV3 = VectoCSVFile.Read(fileNameV3);
			if (!File.Exists(fileNameV22)) {
				LogManager.GetCurrentClassLogger().Error("Modfile V2.2 not found: " + fileNameV22);
				//Write(fileNameV3);
				return;
			}
			var modDataV22 = VectoCSVFile.Read(fileNameV22);

			var xfields = new[] { ModalResultField.time, ModalResultField.dist };

			var yfields = new[] {
				ModalResultField.v_act, ModalResultField.acc, ModalResultField.n, ModalResultField.Gear, ModalResultField.Pe_eng,
				ModalResultField.Tq_eng, ModalResultField.FCMap
			};

			var titleHeight = (50 * 100.0f) / (diagramSize.Height * yfields.Count());

			foreach (var xfield in xfields) {
				var fileName = string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(fileNameV3), xfield.GetName());

				var x = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(xfield.GetName()).ToDouble()).ToArray();
				var x2 = modDataV22.Rows.Cast<DataRow>().Select(v => v.Field<string>(xfield.GetName()).ToDouble()).ToArray();

				var plotSize = new Size(diagramSize.Width, diagramSize.Height * yfields.Count());
				var maxX = (int)(Math.Ceiling(Math.Max(x.Max(), x2.Max()) * 1.01 / 10.0) * 10.0);
				var minX = (int)(Math.Floor(Math.Max(x.Min(), x2.Min()) / 10.0) * 10.0);
				var chart = new Chart { Size = plotSize };


				for (var i = 0; i < yfields.Length; i++) {
					var yfield = yfields[i];
					var y =
						modDataV3.Rows.Cast<DataRow>()
							.Select(
								v => v.Field<string>(yfield.GetName()).Length == 0 ? Double.NaN : v.Field<string>(yfield.GetName()).ToDouble())
							.ToArray();
					var y2 =
						modDataV22.Rows.Cast<DataRow>()
							.Select(
								v =>
									v.Field<string>(yfield.GetName()).Length == 0 ? Double.NaN : v.Field<string>(yfield.GetName()).ToDouble())
							.ToArray();


					var chartArea = new ChartArea { Name = yfield.ToString() };
					chartArea.AxisX.MajorGrid.LineColor = Color.DarkGray;
					chartArea.AxisY.MajorGrid.LineColor = Color.DarkGray;
					chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 10);
					chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 10);

					chartArea.AxisX.Interval = maxX / 20.0;
					chartArea.AxisX.Maximum = maxX;
					chartArea.AxisX.Minimum = minX;
					chartArea.AxisX.MinorGrid.Enabled = true;
					chartArea.AxisX.MinorGrid.Interval = maxX / 100.0;
					chartArea.AxisX.MinorGrid.LineColor = Color.LightGray;
					chartArea.AxisX.Title = xfield.GetCaption();
					chartArea.AxisX.TitleFont = new Font("Verdana", 12);
					chartArea.AxisX.RoundAxisValues();
					chartArea.AxisX.MajorTickMark.Size = 2 * 100.0f / diagramSize.Height;

					chartArea.AxisY.Title = yfield.GetCaption();
					chartArea.AxisY.TitleFont = new Font("Verdana", 12);
					chartArea.AxisY.RoundAxisValues();
					if (yfield == ModalResultField.Gear) {
						chartArea.AxisY.MajorGrid.Interval = 1;
						chartArea.AxisY.MinorGrid.Enabled = false;
					} else {
						chartArea.AxisY.MinorGrid.Enabled = true;
					}
					chartArea.AxisY.MinorGrid.LineColor = Color.LightGray;
					chartArea.AxisY.MajorTickMark.Size = 5 * 100.0f / diagramSize.Width;

					chart.ChartAreas.Add(chartArea);

					var legend = new Legend(yfield.ToString()) {
						Docking = Docking.Right,
						IsDockedInsideChartArea = false,
						DockedToChartArea = yfield.ToString(),
						Font = new Font("Verdana", 14),
						
					};
					chart.Legends.Add(legend);

					if (yfield == ModalResultField.v_act) {
						var y3 = modDataV3.Rows.Cast<DataRow>()
							.Select(
								v =>
									v.Field<string>(ModalResultField.v_targ.GetName()).Length == 0
										? Double.NaN
										: v.Field<string>(ModalResultField.v_targ.GetName()).ToDouble())
							.ToArray();

						var series3 = new Series {
							Name = "v_target",
							ChartType = SeriesChartType.FastLine,
							Color = Color.Green,
							BorderWidth = 3,
							Legend = legend.Name,
							IsVisibleInLegend = true
						};
						chart.Series.Add(series3);
						chart.Series[series3.Name].Points.DataBindXY(x, y3);
						series3.ChartArea = chartArea.Name;
					}

					var series1 = new Series {
						Name = String.Format("Vecto 3 - {0}", yfield),
						ChartType = SeriesChartType.Line,
						Color = Color.Blue,
						BorderWidth = 2,
						Legend = legend.Name,
						IsVisibleInLegend = true,
						//MarkerColor = Color.Blue,
						//MarkerSize = 4,
						//MarkerStyle = MarkerStyle.Circle,
						//MarkerBorderColor = Color.White,
						//MarkerBorderWidth = 1,
					};
					series1.ChartArea = chartArea.Name;

					chart.Series.Add(series1);
					chart.Series[series1.Name].Points.DataBindXY(x, y);

					var series2 = new Series {
						Name = String.Format("Vecto 2.2 - {0}", yfield),
						ChartType = SeriesChartType.Line,
						Color = Color.Red,
						BorderWidth = 2,
						Legend = legend.Name,
						IsVisibleInLegend = true,
						//MarkerColor = Color.Red,
						//MarkerSize = 4,
						//MarkerStyle = MarkerStyle.Circle,
						//MarkerBorderColor = Color.White,
						//MarkerBorderWidth = 1,
					};
					series2.ChartArea = chartArea.Name;

					chart.Series.Add(series2);
					chart.Series[series2.Name].Points.DataBindXY(x2, y2);


					chartArea.Position.Auto = false;
					chartArea.Position.Width = 85;
					chartArea.Position.Height = (100.0f - titleHeight) / yfields.Count();
					chartArea.Position.X = 0;
					chartArea.Position.Y = (i * (100.0f - titleHeight)) / yfields.Count() + titleHeight;

					if (i > 0) {
						chart.ChartAreas[yfield.ToString()].AlignWithChartArea = yfields[0].ToString();
						chart.ChartAreas[yfield.ToString()].AlignmentOrientation = AreaAlignmentOrientations.Vertical;
						chart.ChartAreas[yfield.ToString()].AlignmentStyle = AreaAlignmentStyles.All;
					}
				}

				var title = new Title();
				title.Text = Path.GetFileNameWithoutExtension(fileName);
				title.DockedToChartArea = yfields[0].ToString();
				title.IsDockedInsideChartArea = false;
				title.Font = new Font("Verdana", 18, FontStyle.Bold);
				chart.Titles.Add(title);

				chart.Invalidate();
				chart.SaveImage(fileName, ChartImageFormat.Png);
			}
		}

		//public static void Write(string fileName)
		//{
		//	if (!_enabled) {
		//		return;
		//	}

		//	var modDataV3 = VectoCSVFile.Read(fileName);

		//	var xfields = new[] { ModalResultField.time, ModalResultField.dist };

		//	var yfields = new[] {
		//		ModalResultField.v_act, ModalResultField.acc, ModalResultField.n, ModalResultField.Gear, ModalResultField.Pe_eng,
		//		ModalResultField.Tq_eng, ModalResultField.FCMap
		//	};

		//	var images = new List<Stream>();
		//	try {
		//		foreach (var xfield in xfields) {
		//			var x = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(xfield.GetName())).ToArray();

		//			for (var i = 1; i <= yfields.Length; i++) {
		//				var yfield = yfields[i - 1];
		//				var y = modDataV3.Rows.Cast<DataRow>().Select(v => v.Field<string>(yfield.GetName())).ToArray();

		//				var values = string.Format("{0}|{1}", string.Join(",", x), string.Join(",", y));

		//				if (yfield == ModalResultField.v_act) {
		//					var y3 =
		//						modDataV3.Rows.Cast<DataRow>()
		//							.Select(v => v.Field<string>(ModalResultField.v_targ.GetName()))
		//							.Select(v => string.IsNullOrWhiteSpace(v) ? "0" : v);

		//					values += string.Format("|{0}|{1}|0|0", string.Join(",", x), string.Join(",", y3));
		//				}

		//				values = values.Replace("NaN", "0");
		//				if (values.Length > 14000) {
		//					// remove all decimal places to reduce request size
		//					values = Regex.Replace(values, @"\..*?,", ",");
		//				}
		//				var maxX = (int)Math.Ceiling(x.ToDouble().Max());
		//				images.Add(CreateGraphStream(xfield.GetCaption(), yfield.GetCaption(), maxX, values));
		//			}
		//			var outfileName = string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(fileName), xfield.GetName());
		//			SaveImages(outfileName, images.ToArray());
		//			images.Clear();
		//		}
		//	} finally {
		//		images.ForEach(x => x.Close());
		//	}
		//}
	}
}