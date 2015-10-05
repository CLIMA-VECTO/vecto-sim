using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using Image = iTextSharp.text.Image;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Report
	{
		public AxleConfiguration AxleConf;
		public System.Drawing.Image ChartCO2speed;
		public System.Drawing.Image ChartCO2tkm;
		public string Creator = "";
		public cMissionResults CurrentMR;
		public string DateStr = "";
		public string EngModelStr = "";
		public string EngStr = "";
		public string Filepath = "";
		public string GbxModelStr = "";
		public string GbxStr = "";
		public string HDVclassStr = "";
		public string JobFile = "";
		public string MassMaxStr = "";
		public List<cMissionResults> MissionResults = new List<cMissionResults>();
		public VehicleCategory VehCat;

		public void CreateCharts()
		{
			ChartArea area;
			Chart chart;
			Series series;
			System.Drawing.Rectangle rectangle;

			foreach (var results in MissionResults) {
				chart = new Chart { Width = 0x3e8, Height = 0x1ab };
				area = new ChartArea();

				series = new Series();
				series.Points.DataBindXY(VECTO_Global.ENG.FLD.LnU, new IEnumerable[] { VECTO_Global.ENG.FLD.LTq });
				series.ChartType = SeriesChartType.FastLine;
				series.BorderWidth = 3;
				series.Color = Color.DarkBlue;
				series.Name = "Full load curve";
				chart.Series.Add(series);

				series = new Series();
				series.Points.DataBindXY(VECTO_Global.ENG.FLD.LnU, new IEnumerable[] { VECTO_Global.ENG.FLD.LTqDrag });
				series.ChartType = SeriesChartType.FastLine;
				series.BorderWidth = 3;
				series.Color = Color.Blue;
				series.Name = "Drag curve";
				chart.Series.Add(series);

				series = new Series();
				series.Points.DataBindXY(results.Results[tLoading.RefLoaded].nU,
					new IEnumerable[] { results.Results[tLoading.RefLoaded].Tq });
				series.ChartType = SeriesChartType.Point;
				series.Color = Color.Red;
				series.Name = "load points (Ref. load.)";
				chart.Series.Add(series);

				area.Name = "main";
				area.AxisX.Title = "engine speed [1/min]";
				area.AxisX.TitleFont = new System.Drawing.Font("Helvetica", 20f);
				area.AxisX.LabelStyle.Font = new System.Drawing.Font("Helvetica", 20f);
				area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
				area.AxisY.Title = "engine torque [Nm]";
				area.AxisY.TitleFont = new System.Drawing.Font("Helvetica", 20f);
				area.AxisY.LabelStyle.Font = new System.Drawing.Font("Helvetica", 20f);
				area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
				area.AxisX.Minimum = 300.0;
				area.BorderDashStyle = ChartDashStyle.Solid;
				area.BorderWidth = 3;
				chart.ChartAreas.Add(area);

				var area2 = chart.ChartAreas[0];
				area2.Position.X = 0f;
				area2.Position.Y = 0f;
				area2.Position.Width = 70f;
				area2.Position.Height = 100f;

				chart.Legends.Add("main");
				chart.Legends[0].Font = new System.Drawing.Font("Helvetica", 14f);
				chart.Legends[0].BorderColor = Color.Black;
				chart.Legends[0].BorderWidth = 3;
				chart.Update();

				results.ChartTqN = new Bitmap(chart.Width, chart.Height, PixelFormat.Format32bppArgb);
				rectangle = new System.Drawing.Rectangle(0, 0, results.ChartTqN.Size.Width, results.ChartTqN.Size.Height);
				chart.DrawToBitmap((Bitmap)results.ChartTqN, rectangle);
			}
			foreach (var results in MissionResults) {
				chart = new Chart { Width = 0x7d0, Height = 400 };

				area = new ChartArea();

				series = new Series();
				series.Points.DataBindXY(results.Results[tLoading.RefLoaded].Distance,
					new IEnumerable[] { results.Results[tLoading.RefLoaded].Alt });
				series.ChartType = SeriesChartType.Area;
				series.Color = Color.Lavender;
				series.Name = "Altitude";
				series.YAxisType = AxisType.Secondary;
				chart.Series.Add(series);

				series = new Series();
				series.Points.DataBindXY(results.Results[tLoading.RefLoaded].Distance,
					new IEnumerable[] { results.Results[tLoading.RefLoaded].TargetSpeed });
				series.ChartType = SeriesChartType.FastLine;
				series.BorderWidth = 3;
				series.Name = "Target speed";
				chart.Series.Add(series);

				foreach (KeyValuePair<tLoading, cLoadingResults> pair in results.Results) {
					series = new Series();
					series.Points.DataBindXY(pair.Value.Distance, new IEnumerable[] { pair.Value.ActualSpeed });
					series.ChartType = SeriesChartType.FastLine;
					series.Name = VECTO_Global.ConvLoading(pair.Key);
					chart.Series.Add(series);
				}

				area.Name = "main";
				area.AxisX.Title = "distance [km]";
				area.AxisX.TitleFont = new System.Drawing.Font("Helvetica", 16f);
				area.AxisX.LabelStyle.Font = new System.Drawing.Font("Helvetica", 16f);
				area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
				area.AxisX.LabelStyle.Format = "0.0";
				area.AxisY.Title = "vehicle speed [km/h]";
				area.AxisY.TitleFont = new System.Drawing.Font("Helvetica", 16f);
				area.AxisY.LabelStyle.Font = new System.Drawing.Font("Helvetica", 16f);
				area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
				area.AxisY2.Title = "altitude [m]";
				area.AxisY2.TitleFont = new System.Drawing.Font("Helvetica", 16f);
				area.AxisY2.LabelStyle.Font = new System.Drawing.Font("Helvetica", 16f);
				area.AxisY2.LabelAutoFitStyle = LabelAutoFitStyles.None;
				area.AxisY2.MinorGrid.Enabled = false;
				area.AxisY2.MajorGrid.Enabled = false;
				area.AxisX.Minimum = 0.0;
				area.BorderDashStyle = ChartDashStyle.Solid;
				area.BorderWidth = 3;
				chart.ChartAreas.Add(area);

				var area3 = chart.ChartAreas[0];
				area3.Position.X = 0f;
				area3.Position.Y = 0f;
				area3.Position.Width = 90f;
				area3.Position.Height = 100f;
				chart.Legends.Add("main");
				chart.Legends[0].Font = new System.Drawing.Font("Helvetica", 14f);
				chart.Legends[0].BorderColor = Color.Black;
				chart.Legends[0].BorderWidth = 3;
				chart.Legends[0].Position.X = 97f;
				chart.Legends[0].Position.Y = 3f;
				chart.Legends[0].Position.Width = 10f;
				chart.Legends[0].Position.Height = 40f;
				chart.Update();

				results.ChartSpeed = new Bitmap(chart.Width, chart.Height, PixelFormat.Format32bppArgb);
				rectangle = new System.Drawing.Rectangle(0, 0, results.ChartSpeed.Size.Width, results.ChartSpeed.Size.Height);
				chart.DrawToBitmap((Bitmap)results.ChartSpeed, rectangle);
			}

			chart = new Chart();

			area = new ChartArea();

			foreach (var results in MissionResults) {
				series = new Series();
				series.Points.AddXY(results.MissionRef.NameStr, new object[] { results.Results[tLoading.RefLoaded].CO2tkm });
				series.Points[0].Label = series.Points[0].YValues[0].ToString("0.0") + " [g/tkm]";
				series.Points[0].Font = new System.Drawing.Font("Helvetica", 20f);
				series.Points[0].LabelBackColor = Color.White;
				series.Name = results.MissionRef.NameStr + " (Ref. load.)";
				chart.Series.Add(series);
			}

			area.Name = "main";
			area.AxisX.Title = "Missions";
			area.AxisX.TitleFont = new System.Drawing.Font("Helvetica", 20f);
			area.AxisX.LabelStyle.Enabled = false;
			area.AxisY.Title = "CO2 [g/tkm]";
			area.AxisY.TitleFont = new System.Drawing.Font("Helvetica", 20f);
			area.AxisY.LabelStyle.Font = new System.Drawing.Font("Helvetica", 20f);
			area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
			area.BorderDashStyle = ChartDashStyle.Solid;
			area.BorderWidth = 3;
			chart.ChartAreas.Add(area);

			chart.Legends.Add("main");
			chart.Legends[0].Font = new System.Drawing.Font("Helvetica", 20f);
			chart.Legends[0].BorderColor = Color.Black;
			chart.Legends[0].BorderWidth = 3;
			chart.Width = 0x5dc;
			chart.Height = 700;
			chart.Update();

			ChartCO2tkm = new Bitmap(chart.Width, chart.Height, PixelFormat.Format32bppArgb);
			rectangle = new System.Drawing.Rectangle(0, 0, ChartCO2tkm.Size.Width, ChartCO2tkm.Size.Height);
			chart.DrawToBitmap((Bitmap)ChartCO2tkm, rectangle);

			chart = new Chart();

			area = new ChartArea();

			foreach (var results in MissionResults) {
				series = new Series {
					MarkerSize = 15,
					MarkerStyle = MarkerStyle.Circle,
					ChartType = SeriesChartType.Point
				};
				short num = -1;
				foreach (KeyValuePair<tLoading, cLoadingResults> pair in results.Results) {
					num = (short)(num + 1);
					series.Points.AddXY((double)pair.Value.Speed, (double)pair.Value.CO2km);
					series.Points[num].Label = pair.Value.Loading.ToString("0.0") + " t";
					if (((tLoading)pair.Key) == tLoading.RefLoaded) {
						series.Points[num].Font = new System.Drawing.Font("Helvetica", 16f);
					} else {
						series.Points[num].MarkerSize = 10;
						series.Points[num].Font = new System.Drawing.Font("Helvetica", 14f);
					}
					series.Points[num].LabelBackColor = Color.White;
				}
				series.Name = results.MissionRef.NameStr;
				chart.Series.Add(series);
			}

			area.Name = "main";
			area.AxisX.Title = "vehicle speed [km/h]";
			area.AxisX.TitleFont = new System.Drawing.Font("Helvetica", 20f);
			area.AxisX.LabelStyle.Font = new System.Drawing.Font("Helvetica", 20f);
			area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
			area.AxisY.Title = "CO2 [g/km]";
			area.AxisY.TitleFont = new System.Drawing.Font("Helvetica", 20f);
			area.AxisY.LabelStyle.Font = new System.Drawing.Font("Helvetica", 20f);
			area.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
			area.AxisX.Minimum = 20.0;
			area.BorderDashStyle = ChartDashStyle.Solid;
			area.BorderWidth = 3;
			chart.ChartAreas.Add(area);

			chart.Legends.Add("main");
			chart.Legends[0].Font = new System.Drawing.Font("Helvetica", 20f);
			chart.Legends[0].BorderColor = Color.Black;
			chart.Legends[0].BorderWidth = 3;
			chart.Width = 0x5dc;
			chart.Height = 700;
			chart.Update();

			ChartCO2speed = new Bitmap(chart.Width, chart.Height, PixelFormat.Format32bppArgb);
			rectangle = new System.Drawing.Rectangle(0, 0, ChartCO2speed.Size.Width, ChartCO2speed.Size.Height);

			chart.DrawToBitmap((Bitmap)ChartCO2speed, rectangle);
		}

		public void WritePdfs()
		{
			var pgMax = MissionResults.Count;
			var filename = string.Format("Reports\rep{0}C.pdf", pgMax);
			var temppdfs = new List<string>();

			var reportOutputPath = "Declaration"; //todo: myAppPath + \Declaration

			var pdfTempMR = reportOutputPath + @"Reports\repMR.pdf";

			var temppath = reportOutputPath + @"Reports\temp0.pdf";
			temppdfs.Add(temppath);

			var reader = new PdfReader(filename);
			var stamper = new PdfStamper(reader, new FileStream(temppath, FileMode.Create));

			var pdfFields = stamper.AcroFields;
			pdfFields.SetField("version", "3");
			pdfFields.SetField("Job", JobFile);
			pdfFields.SetField("Date", DateStr);
			pdfFields.SetField("Created", Creator);
			pdfFields.SetField("Config", MassMaxStr + " " + AxleConf + " " + VehCat);
			pdfFields.SetField("HDVclass", "HDV Class " + HDVclassStr);
			pdfFields.SetField("Engine", EngStr);
			pdfFields.SetField("EngM", EngModelStr);
			pdfFields.SetField("Gearbox", GbxStr);
			pdfFields.SetField("GbxM", GbxModelStr);
			pdfFields.SetField("PageNr", "Page 1 of " + pgMax);

			for (var i = 1; i <= MissionResults.Count; i++) {
				var results = MissionResults[i];
				pdfFields.SetField("Mission" + i, results.MissionRef.NameStr);
				cLoadingResults results2 = results.Results[tLoading.RefLoaded];
				pdfFields.SetField("Loading" + i, results2.Loading.ToString("0.0") + " t");
				pdfFields.SetField("Speed" + i, results2.Speed.ToString("0.0") + " km/h");
				pdfFields.SetField("FC" + i, results2.FCkm.ToString("0.0"));
				pdfFields.SetField("FCt" + i, results2.FCtkm.ToString("0.0"));
				pdfFields.SetField("CO2" + i, results2.CO2km.ToString("0.0"));
				pdfFields.SetField("CO2t" + i, results2.CO2tkm.ToString("0.0"));
			}

			// Add Images
			var content = stamper.GetOverContent(1);
			var img = Image.GetInstance(ChartCO2tkm, BaseColor.WHITE);
			img.ScaleAbsolute(440f, 195f);
			img.SetAbsolutePosition(360f, 270f);
			content.AddImage(img);

			img = Image.GetInstance(ChartCO2speed, BaseColor.WHITE);
			img.ScaleAbsolute(440f, 195f);
			img.SetAbsolutePosition(360f, 75f);
			content.AddImage(img);

			//todo get image for hdv class
			var hdvClassImagePath = "";
			img = Image.GetInstance(hdvClassImagePath);
			img.ScaleAbsolute(180f, 50f);
			img.SetAbsolutePosition(30f, 475f);
			content.AddImage(img);

			// flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
			stamper.FormFlattening = true;

			// close the pdf
			stamper.Close();

			for (var i = 1; i <= MissionResults.Count; i++) {
				var results = MissionResults[i];

				temppath = reportOutputPath + @"Reports\temp" + i + ".pdf";
				temppdfs.Add(temppath);

				reader = new PdfReader(pdfTempMR);
				stamper = new PdfStamper(reader, new FileStream(temppath, FileMode.Create));

				pdfFields = stamper.AcroFields;
				pdfFields.SetField("version", "3");
				pdfFields.SetField("Job", JobFile);
				pdfFields.SetField("Date", DateStr);
				pdfFields.SetField("Created", Creator);
				pdfFields.SetField("Config", MassMaxStr + " " + AxleConf + " " + VehCat);
				pdfFields.SetField("HDVclass", "HDV Class " + HDVclassStr);
				pdfFields.SetField("PageNr", "Page " + (i + 1) + " of " + pgMax);
				pdfFields.SetField("Mission", results.MissionRef.NameStr);

				foreach (KeyValuePair<tLoading, cLoadingResults> pair in results.Results) {
					var str = "";
					switch (pair.Key) {
						case tLoading.FullLoaded:
							str = "F";
							break;

						case tLoading.EmptyLoaded:
							str = "E";
							break;

						case tLoading.RefLoaded:
							str = "R";
							break;
					}

					var loading = pair.Value;
					pdfFields.SetField("Load" + str, loading.Loading.ToString("0.0") + " t");
					pdfFields.SetField("Speed" + str, loading.Speed.ToString("0.0"));
					pdfFields.SetField("FCkm" + str, loading.FCkm.ToString("0.0"));

					if (loading.Loading == 0) {
						pdfFields.SetField("FCtkm" + str, "-");
						pdfFields.SetField("CO2tkm" + str, "-");
					} else {
						pdfFields.SetField("FCtkm" + str, loading.FCtkm.ToString("0.0"));
						pdfFields.SetField("CO2tkm" + str, loading.CO2tkm.ToString("0.0"));
					}

					pdfFields.SetField("CO2km" + str, loading.CO2km.ToString("0.0"));
				}


				content = stamper.GetOverContent(1);

				img = Image.GetInstance(hdvClassImagePath);
				img.ScaleAbsolute(180f, 50f);
				img.SetAbsolutePosition(600f, 475f);
				content.AddImage(img);

				img = Image.GetInstance(results.ChartSpeed, BaseColor.WHITE);
				img.ScaleAbsolute(780f, 156f);
				img.SetAbsolutePosition(17f, 270f);
				content.AddImage(img);

				img = Image.GetInstance(results.ChartTqN, BaseColor.WHITE);
				img.ScaleAbsolute(420f, 178f);
				img.SetAbsolutePosition(375f, 75f);
				content.AddImage(img);

				// flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
				stamper.FormFlattening = true;

				// close the pdf
				stamper.Close();
			}

			// Merge files
			var document = new Document(PageSize.A4.Rotate(), 12f, 12f, 12f, 12f);
			var writer = PdfWriter.GetInstance(document, new FileStream(Filepath, FileMode.Create));

			document.Open();

			foreach (var path in temppdfs) {
				reader = new PdfReader(temppath);
				var importedPage = writer.GetImportedPage(reader, 1);
				document.Add(Image.GetInstance(importedPage));
				File.Delete(path);
			}

			document.Close();
		}

		public class cLoadingResults
		{
			public List<float> ActualSpeed = new List<float>();
			public List<float> Alt = new List<float>();
			public float CO2km = 0f;
			public float CO2tkm = 0f;
			public List<float> Distance = new List<float>();
			public bool FCerror = false;
			public float FCkm = 0f;
			public float FCtkm = 0f;
			public float Loading = 0f;
			public List<float> nU = new List<float>();
			public float Speed = 0f;
			public List<float> TargetSpeed = new List<float>();
			public List<float> Tq = new List<float>();
		}

		public class cMissionResults
		{
			public System.Drawing.Image ChartSpeed;
			public System.Drawing.Image ChartTqN;
			public cMission MissionRef;
			public Dictionary<tLoading, cReport.cLoadingResults> Results = new Dictionary<tLoading, cReport.cLoadingResults>();
		}
	}
}