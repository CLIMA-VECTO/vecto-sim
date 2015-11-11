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

        private static Size _diagramSize = new Size(2000, 440);

        private static readonly Font AxisLabelFont = new Font("Consolas", 10);
        private static readonly Font AxisTitleFont = new Font("Verdana", 12);
        private static readonly Font LegendFont = new Font("Verdana", 14);

        public static void Enabled()
        {
            _enabled = true;
        }

        public static void Disable()
        {
            _enabled = false;
        }

        public static void Write(string fileNameV3, string fileNameV22 = null)
        {
            if (!_enabled) {
                return;
            }

            var modDataV3 = VectoCSVFile.Read(fileNameV3);
            if (!File.Exists(fileNameV22)) {
                LogManager.GetCurrentClassLogger().Error("Modfile V2.2 not found: " + fileNameV22);
                //Write(fileNameV3);
                //return;
            }
            DataTable modDataV22 = null;
            if (fileNameV22 != null) {
                modDataV22 = VectoCSVFile.Read(fileNameV22);
            }
            var xfields = new[] { ModalResultField.time, ModalResultField.dist };

            var yfields = new[] {
                ModalResultField.v_act, ModalResultField.acc, ModalResultField.n, ModalResultField.Gear,
                ModalResultField.Pe_eng, ModalResultField.Tq_eng, ModalResultField.FCMap
            };

            var titleHeight = (50 * 100.0f) / (_diagramSize.Height * yfields.Count());

            foreach (var xfield in xfields) {
                var fileName = string.Format("{0}_{1}.png", Path.GetFileNameWithoutExtension(fileNameV3),
                    xfield.GetName());

                var x = LoadData(modDataV3, xfield.GetName());
                var x2 = new double[] { double.NegativeInfinity };
                if (fileNameV22 != null && modDataV22 != null) {
                    x2 = LoadData(modDataV22, xfield.GetName());
                }
                var plotSize = new Size(_diagramSize.Width, _diagramSize.Height * yfields.Count());
                var maxX = (int)(Math.Ceiling(Math.Max(x.Max(), x2.Max()) * 1.01 / 10.0) * 10.0);
                var minX = (int)(Math.Floor(Math.Max(x.Min(), x2.Min()) / 10.0) * 10.0);
                var chart = new Chart { Size = plotSize };


                for (var i = 0; i < yfields.Length; i++) {
                    var yfield = yfields[i];
                    var y = LoadData(modDataV3, yfield.GetName());


                    var chartArea = AddChartArea(chart, yfield.ToString(), xfield.GetCaption(), maxX, minX,
                        yfield.GetCaption(), yfield == ModalResultField.Gear);

                    var legend = CreateLegend(chart, yfield.ToString());

                    if (yfield == ModalResultField.v_act) {
                        var y3 = LoadData(modDataV3, ModalResultField.v_targ.GetName());
                        var series3 = CreateSeries("v_target", legend, chartArea, chart, Color.Green, x, y3);

                        var grad = LoadData(modDataV3, ModalResultField.grad.GetName());

                        chartArea.AxisY2.Enabled = AxisEnabled.True;
                        chartArea.AxisY2.Title = "gradient [%]";
                        chartArea.AxisY2.TitleFont = AxisTitleFont;
                        chartArea.AxisY2.LabelStyle.Font = AxisLabelFont;
                        chartArea.AxisY2.LabelAutoFitStyle = LabelAutoFitStyles.None;
                        chartArea.AxisY2.MinorGrid.Enabled = false;
                        chartArea.AxisY2.MajorGrid.Enabled = false;
                        var max = Math.Max(-Math.Round(grad.Min() * 2), Math.Round(grad.Max() * 2));
                        //chartArea.AxisY2.si
                        //chartArea.AxisY2.Minimum = -max;
                        //chartArea.AxisY2.Maximum = max;
                        chartArea.AxisY2.RoundAxisValues();
                        chartArea.AxisY2.Interval = Math.Round(max / 5);

                        var seriesGrad = CreateSeries("Gradient", legend, chartArea, chart, Color.Coral, x, grad);
                        seriesGrad.YAxisType = AxisType.Secondary;
                    }

                    var series1 = CreateSeries(string.Format("Vecto 3 - {0}", yfield), legend, chartArea, chart,
                        Color.Blue, x, y);

                    if (fileNameV22 != null) {
                        var y2 = LoadData(modDataV22, yfield.GetName());
                        var series2 = CreateSeries(string.Format("Vecto 2.2 - {0}", yfield), legend, chartArea, chart,
                            Color.Red, x2,
                            y2);
                    }

                    PositionChartArea(chartArea, titleHeight, i, yfields.Count());

                    if (i > 0) {
                        AlignChart(chart, yfield.ToString(), yfields[0].ToString());
                    }
                }

                AddTitle(chart, Path.GetFileNameWithoutExtension(fileName), yfields[0].ToString());

                chart.Invalidate();
                chart.SaveImage(fileName, ChartImageFormat.Png);
            }
        }

        public static bool WriteDistanceSlice(string fileNameV3, string fileNameV22, double start, double end)
        {
            if (!_enabled) {
                return true;
            }

            var modDataV3Iput = VectoCSVFile.Read(fileNameV3);
            //var modDataV3View = new DataView(modDataV3Iput) {
            //    RowFilter = string.Format(@"dist > {0} AND dist < {1}", start, end)
            //};
            //var modDataV3 = modDataV3View.ToTable();
            var modDataV3tmp = modDataV3Iput.AsEnumerable().Where(row => {
                var s = row.ParseDouble("dist");
                return s >= start && s <= end;
            });


            if (!File.Exists(fileNameV22)) {
                //LogManager.GetCurrentClassLogger().Error("Modfile V2.2 not found: " + fileNameV22);
                //Write(fileNameV3);
                //return;
            }
            DataTable modDataV22 = null;
            if (fileNameV22 != null) {
                var modDataV22Input = VectoCSVFile.Read(fileNameV22);
                //var modDataV22View = new DataView(modDataV22Input) {
                //    RowFilter = string.Format(@"dist > {0} AND dist < {1}", start, end)
                //};
                var modDataV22tmp = modDataV22Input.AsEnumerable().Where(row => {
                    var s = row.ParseDouble("dist");
                    return s >= start && s <= end;
                });
                if (!(modDataV3tmp.Any() || modDataV22tmp.Any())) {
                    return false;
                }
                modDataV22 = modDataV22tmp.CopyToDataTable();
            } else {
                if (!modDataV3tmp.Any()) {
                    return false;
                }
            }
            var modDataV3 = modDataV3tmp.CopyToDataTable();

            //var xfields = new[] { ModalResultField.dist };
            var xfield = ModalResultField.dist;
            var yfields = new[] {
                ModalResultField.v_act, ModalResultField.acc, ModalResultField.n, ModalResultField.Gear,
                ModalResultField.Pe_eng, ModalResultField.Tq_eng, ModalResultField.FCMap
            };

            var titleHeight = (50 * 100.0f) / (_diagramSize.Height * yfields.Count());

            //foreach (var xfield in xfields) {
            var fileName = string.Format("{0}_{1}-{2:D3}_{3:D3}.png", Path.GetFileNameWithoutExtension(fileNameV3),
                xfield.GetName(), (int)(start / 1000), (int)(end / 1000));

            var x = LoadData(modDataV3, xfield.GetName());
            var x2 = new double[] { double.NegativeInfinity };
            if (fileNameV22 != null && modDataV22 != null) {
                x2 = LoadData(modDataV22, xfield.GetName());
            }
            var plotSize = new Size(_diagramSize.Width, _diagramSize.Height * yfields.Count());
            var maxX = (int)(Math.Ceiling(Math.Max(x.Max(), x2.Max()) * 1.01 / 10.0) * 10.0);
            var minX = (int)(Math.Floor(Math.Max(x.Min(), x2.Min()) / 10.0) * 10.0);
            var chart = new Chart { Size = plotSize };


            for (var i = 0; i < yfields.Length; i++) {
                var yfield = yfields[i];
                var y = LoadData(modDataV3, yfield.GetName());


                var chartArea = AddChartArea(chart, yfield.ToString(), xfield.GetCaption(), maxX, minX,
                    yfield.GetCaption(), yfield == ModalResultField.Gear);

                var legend = CreateLegend(chart, yfield.ToString());

                if (yfield == ModalResultField.v_act) {
                    var y3 = LoadData(modDataV3, ModalResultField.v_targ.GetName());
                    var series3 = CreateSeries("v_target", legend, chartArea, chart, Color.Green, x, y3);

                    var grad = LoadData(modDataV3, ModalResultField.grad.GetName());

                    chartArea.AxisY2.Enabled = AxisEnabled.True;
                    chartArea.AxisY2.Title = "gradient [%]";
                    chartArea.AxisY2.TitleFont = AxisTitleFont;
                    chartArea.AxisY2.LabelStyle.Font = AxisLabelFont;
                    chartArea.AxisY2.LabelAutoFitStyle = LabelAutoFitStyles.None;
                    chartArea.AxisY2.MinorGrid.Enabled = false;
                    chartArea.AxisY2.MajorGrid.Enabled = false;
                    var max = Math.Max(-Math.Round(grad.Min() * 2), Math.Round(grad.Max() * 2));
                    //chartArea.AxisY2.si
                    chartArea.AxisY2.Minimum = -max;
                    chartArea.AxisY2.Maximum = max;
                    chartArea.AxisY2.RoundAxisValues();

                    var seriesGrad = CreateSeries("Gradient", legend, chartArea, chart, Color.Coral, x, grad);
                    seriesGrad.YAxisType = AxisType.Secondary;
                }


                var series1 = CreateSeries(string.Format("Vecto 3 - {0}", yfield), legend, chartArea, chart,
                    Color.Blue, x, y);

                if (fileNameV22 != null) {
                    var y2 = LoadData(modDataV22, yfield.GetName());
                    var series2 = CreateSeries(string.Format("Vecto 2.2 - {0}", yfield), legend, chartArea, chart,
                        Color.Red, x2, y2);
                }

                PositionChartArea(chartArea, titleHeight, i, yfields.Count());

                if (i > 0) {
                    AlignChart(chart, yfield.ToString(), yfields[0].ToString());
                }
                //}

                AddTitle(chart, Path.GetFileNameWithoutExtension(fileName), yfields[0].ToString());

                chart.Invalidate();
                chart.SaveImage(fileName, ChartImageFormat.Png);
            }
            return true;
        }


        private static void AddTitle(Chart chart, string titleText, string dockToChartArea)
        {
            var title = new Title();
            title.Text = titleText;
            title.DockedToChartArea = dockToChartArea;
            title.IsDockedInsideChartArea = false;
            title.Font = new Font("Verdana", 18, FontStyle.Bold);
            chart.Titles.Add(title);
        }

        private static double[] LoadData(DataTable modDataV3, string field)
        {
            return modDataV3.Rows.Cast<DataRow>()
                .Select(v => v.Field<string>(field).Length == 0
                    ? Double.NaN
                    : v.Field<string>(field).ToDouble())
                .ToArray();
        }


        private static void AlignChart(Chart chart, string chartToAlign, string chartToAlignWith)
        {
            chart.ChartAreas[chartToAlign].AlignWithChartArea = chartToAlignWith;
            chart.ChartAreas[chartToAlign].AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            chart.ChartAreas[chartToAlign].AlignmentStyle = AreaAlignmentStyles.All;
        }

        private static void PositionChartArea(ChartArea chartArea, float titleHeight, int i, int numCharts)
        {
            chartArea.Position.Auto = false;
            chartArea.Position.Width = 85;
            chartArea.Position.Height = (100.0f - titleHeight) / numCharts;
            chartArea.Position.X = 0;
            chartArea.Position.Y = (i * (100.0f - titleHeight)) / numCharts + titleHeight;
        }

        private static ChartArea AddChartArea(Chart chart, string name, string axisXTitle, int xMax, int xMin,
            string axisYTitle, bool discreteValues)
        {
            var chartArea = new ChartArea { Name = name };
            chartArea.AxisX.MajorGrid.LineColor = Color.DarkGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.DarkGray;
            chartArea.AxisX.LabelStyle.Font = AxisLabelFont;
            chartArea.AxisY.LabelStyle.Font = AxisLabelFont;

            chartArea.AxisX.Interval = xMax / 20.0;
            chartArea.AxisX.Maximum = xMax;
            chartArea.AxisX.Minimum = xMin;
            chartArea.AxisX.MinorGrid.Enabled = true;
            chartArea.AxisX.MinorGrid.Interval = (xMax - xMin) / 100.0;
            chartArea.AxisX.MinorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.Title = axisXTitle;
            chartArea.AxisX.TitleFont = AxisTitleFont;
            chartArea.AxisX.RoundAxisValues();
            chartArea.AxisX.MajorTickMark.Size = 2 * 100.0f / _diagramSize.Height;

            chartArea.AxisY.Title = axisYTitle;
            chartArea.AxisY.TitleFont = AxisTitleFont;
            chartArea.AxisY.RoundAxisValues();
            if (discreteValues) {
                chartArea.AxisY.MajorGrid.Interval = 1;
                chartArea.AxisY.MinorGrid.Enabled = false;
            } else {
                chartArea.AxisY.MinorGrid.Enabled = true;
            }
            chartArea.AxisY.MinorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorTickMark.Size = 5 * 100.0f / _diagramSize.Width;

            chart.ChartAreas.Add(chartArea);
            return chartArea;
        }

        private static Legend CreateLegend(Chart chart, string dockToChartArea)
        {
            var legend = new Legend(dockToChartArea.ToString()) {
                Docking = Docking.Right,
                IsDockedInsideChartArea = false,
                DockedToChartArea = dockToChartArea,
                Font = LegendFont,
            };
            chart.Legends.Add(legend);
            return legend;
        }

        private static Series CreateSeries(String name, Legend legend, ChartArea chartArea, Chart chart, Color color,
            double[] x, double[] y)
        {
            ModalResultField yfield;
            var series1 = new Series {
                Name = name,
                ChartType = SeriesChartType.Line,
                Color = color,
                BorderWidth = 2,
                Legend = legend.Name,
                IsVisibleInLegend = true,
                ChartArea = chartArea.Name,
            };

            chart.Series.Add(series1);
            chart.Series[series1.Name].Points.DataBindXY(x, y);
            return series1;
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