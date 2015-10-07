using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;
using Font = System.Drawing.Font;
using Image = iTextSharp.text.Image;
using Rectangle = System.Drawing.Rectangle;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Report
	{
		public AxleConfiguration AxleConf;
		public System.Drawing.Image ChartCO2speed;
		public System.Drawing.Image ChartCO2tkm;
		public string Creator = "";
		public MissionResults CurrentMR;
		public string DateStr = "";
		public string EngModelStr = "";
		public string EngStr = "";
		public string Filepath = "";
		public string GbxModelStr = "";
		public string GbxStr = "";
		public string HDVclassStr = "";
		public string JobFile = "";
		public string MassMaxStr = "";
		public List<MissionResults> missionResults = new List<MissionResults>();
		public VehicleCategory VehCat;


		private IModalDataWriter _modalData;

		public Report(IModalDataWriter modalData)
		{
			_modalData = modalData;
		}


		public void CreateCharts()
		{
			foreach (var results in missionResults) {
				var missionResultChart = new Chart { Width = 1000, Height = 427 };
				missionResultChart.Legends.Add(new Legend("main") {
					Font = new Font("Helvetica", 14),
					BorderColor = Color.Black,
					BorderWidth = 3,
				});

				missionResultChart.ChartAreas.Add(new ChartArea("main") {
					BorderDashStyle = ChartDashStyle.Solid,
					BorderWidth = 3,
					AxisX = {
						Title = "engine speed [1/min]",
						TitleFont = new Font("Helvetica", 20),
						LabelStyle = { Font = new Font("Helvetica", 20) },
						LabelAutoFitStyle = LabelAutoFitStyles.None,
						Minimum = 300.0
					},
					AxisY = {
						Title = "engine torque [Nm]",
						TitleFont = new Font("Helvetica", 20),
						LabelStyle = { Font = new Font("Helvetica", 20) },
						LabelAutoFitStyle = LabelAutoFitStyles.None
					},
					Position = {
						X = 0,
						Y = 0,
						Width = 70,
						Height = 100
					},
				});

				// todo: get full load curve from engine
				var flc = new FullLoadCurve();
				var n = flc.FullLoadEntries.Select(x => x.EngineSpeed).ToDouble().ToList();

				var fullLoadCurve = new Series("Full load curve") {
					ChartType = SeriesChartType.FastLine,
					BorderWidth = 3,
					Color = Color.DarkBlue
				};
				fullLoadCurve.Points.DataBindXY(n, flc.FullLoadEntries.Select(x => x.TorqueFullLoad).ToDouble());
				missionResultChart.Series.Add(fullLoadCurve);

				var dragLoadCurve = new Series("Drag curve") {
					ChartType = SeriesChartType.FastLine,
					BorderWidth = 3,
					Color = Color.Blue
				};
				dragLoadCurve.Points.DataBindXY(n, flc.FullLoadEntries.Select(x => x.TorqueDrag).ToDouble());
				missionResultChart.Series.Add(dragLoadCurve);

				var dataPoints = new Series("load points (Ref. load.)") { ChartType = SeriesChartType.Point, Color = Color.Red };
				dataPoints.Points.DataBindXY(results.Results[LoadingType.Reference].nU, results.Results[LoadingType.Reference].Tq);
				missionResultChart.Series.Add(dataPoints);

				missionResultChart.Update();

				results.ChartTqN = new Bitmap(missionResultChart.Width, missionResultChart.Height, PixelFormat.Format32bppArgb);
				missionResultChart.DrawToBitmap((Bitmap)results.ChartTqN,
					new Rectangle(0, 0, results.ChartTqN.Size.Width, results.ChartTqN.Size.Height));
			}


			foreach (var results in missionResults) {
				var missionResultChart = new Chart { Width = 2000, Height = 400 };
				missionResultChart.Legends.Add(new Legend("main") {
					Font = new Font("Helvetica", 14),
					BorderColor = Color.Black,
					BorderWidth = 3,
					Position = {
						X = 97f,
						Y = 3f,
						Width = 10f,
						Height = 40f
					}
				});
				missionResultChart.ChartAreas.Add(new ChartArea {
					Name = "main",
					BorderDashStyle = ChartDashStyle.Solid,
					BorderWidth = 3,
					AxisX = {
						Title = "distance [km]",
						TitleFont = new Font("Helvetica", 16),
						LabelStyle = { Font = new Font("Helvetica", 16), Format = "0.0" },
						LabelAutoFitStyle = LabelAutoFitStyles.None,
						Minimum = 0
					},
					AxisY = {
						Title = "vehicle speed [km/h]",
						TitleFont = new Font("Helvetica", 16),
						LabelStyle = { Font = new Font("Helvetica", 16) },
						LabelAutoFitStyle = LabelAutoFitStyles.None
					},
					AxisY2 = {
						Title = "altitude [m]",
						TitleFont = new Font("Helvetica", 16),
						LabelStyle = { Font = new Font("Helvetica", 16) },
						LabelAutoFitStyle = LabelAutoFitStyles.None,
						MinorGrid = { Enabled = false },
						MajorGrid = { Enabled = false }
					},
					Position = { X = 0f, Y = 0f, Width = 90f, Height = 100f },
				});

				var altitude = new Series {
					ChartType = SeriesChartType.Area,
					Color = Color.Lavender,
					Name = "Altitude",
					YAxisType = AxisType.Secondary
				};
				altitude.Points.DataBindXY(results.Results[LoadingType.Reference].Distance,
					results.Results[LoadingType.Reference].Alt);
				missionResultChart.Series.Add(altitude);

				var targetSpeed = new Series {
					ChartType = SeriesChartType.FastLine,
					BorderWidth = 3,
					Name = "Target speed"
				};
				targetSpeed.Points.DataBindXY(results.Results[LoadingType.Reference].Distance,
					results.Results[LoadingType.Reference].TargetSpeed);
				missionResultChart.Series.Add(targetSpeed);

				foreach (var pair in results.Results) {
					var series = new Series {
						ChartType = SeriesChartType.FastLine,
						Name = pair.Key.GetName(),
					};
					series.Points.DataBindXY(pair.Value.Distance, pair.Value.ActualSpeed);
					missionResultChart.Series.Add(series);
				}

				missionResultChart.Update();

				results.ChartSpeed = new Bitmap(missionResultChart.Width, missionResultChart.Height, PixelFormat.Format32bppArgb);
				missionResultChart.DrawToBitmap((Bitmap)results.ChartSpeed,
					new Rectangle(0, 0, results.ChartSpeed.Size.Width, results.ChartSpeed.Size.Height));
			}


			var co2Chart = new Chart { Width = 1500, Height = 700 };
			co2Chart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 20),
				BorderColor = Color.Black,
				BorderWidth = 3,
			});
			co2Chart.ChartAreas.Add(new ChartArea {
				Name = "main",
				AxisX = {
					Title = "Missions",
					TitleFont = new Font("Helvetica", 20),
					LabelStyle = { Enabled = false }
				},
				AxisY = {
					Title = "CO2 [g/tkm]",
					TitleFont = new Font("Helvetica", 20),
					LabelStyle = { Font = new Font("Helvetica", 20) },
					LabelAutoFitStyle = LabelAutoFitStyles.None
				},
				BorderDashStyle = ChartDashStyle.Solid,
				BorderWidth = 3
			});

			foreach (var results in missionResults) {
				var series = new Series(results.Mission + " (Ref. load.)");
				series.Points[0].Label = series.Points[0].YValues[0].ToString("0.0") + " [g/tkm]";
				series.Points[0].Font = new Font("Helvetica", 20f);
				series.Points[0].LabelBackColor = Color.White;

				series.Points.AddXY(results.Mission.ToString(), results.Results[LoadingType.Reference].CO2tkm);
				co2Chart.Series.Add(series);
			}

			co2Chart.Update();
			ChartCO2tkm = new Bitmap(co2Chart.Width, co2Chart.Height, PixelFormat.Format32bppArgb);
			co2Chart.DrawToBitmap((Bitmap)ChartCO2tkm, new Rectangle(0, 0, ChartCO2tkm.Size.Width, ChartCO2tkm.Size.Height));



			var chart = new Chart { Width = 1500, Height = 700 };
			chart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 20f),
				BorderColor = Color.Black,
				BorderWidth = 3,
			});
			chart.ChartAreas.Add(new ChartArea("main")
			{
				BorderDashStyle = ChartDashStyle.Solid,
				BorderWidth = 3,
				AxisX =
				{
					Title = "vehicle speed [km/h]",
					TitleFont = new Font("Helvetica", 20f),
					LabelStyle = { Font = new Font("Helvetica", 20f) },
					LabelAutoFitStyle = LabelAutoFitStyles.None,
					Minimum = 20.0,
				},
				AxisY =
				{
					Title = "CO2 [g/km]",
					TitleFont = new Font("Helvetica", 20f),
					LabelStyle = { Font = new Font("Helvetica", 20f) },
					LabelAutoFitStyle = LabelAutoFitStyles.None
				}
			});


			foreach (var results in missionResults) {
				var series = new Series {
					MarkerSize = 15,
					MarkerStyle = MarkerStyle.Circle,
					ChartType = SeriesChartType.Point
				};
				short num = -1;
				foreach (var pair in results.Results) {
					num = (short)(num + 1);
					series.Points.AddXY(pair.Value.Speed, pair.Value.CO2km);
					series.Points[num].Label = pair.Value.Loading.ToString("0.0") + " t";
					if (pair.Key == LoadingType.Reference) {
						series.Points[num].Font = new Font("Helvetica", 16f);
					} else {
						series.Points[num].MarkerSize = 10;
						series.Points[num].Font = new Font("Helvetica", 14f);
					}
					series.Points[num].LabelBackColor = Color.White;
				}
				series.Name = results.Mission.ToString();
				chart.Series.Add(series);
			}

			chart.Update();
			ChartCO2speed = new Bitmap(chart.Width, chart.Height, PixelFormat.Format32bppArgb);
			chart.DrawToBitmap((Bitmap)ChartCO2speed, new Rectangle(0, 0, ChartCO2speed.Size.Width, ChartCO2speed.Size.Height));
		}

		public void WritePdfs()
		{
			var pgMax = missionResults.Count;
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

			for (var i = 1; i <= missionResults.Count; i++) {
				var results = missionResults[i];
				pdfFields.SetField("Mission" + i, results.Mission.MissionType.ToString());
				LoadingResults results2 = results.Results[LoadingType.Reference];
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

			for (var i = 1; i <= missionResults.Count; i++) {
				var results = missionResults[i];

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
				pdfFields.SetField("Mission", results.Mission.MissionType.ToString());


				foreach (var pair in results.Results) {
					var loadingType = pair.Key;
					var loadingResult = pair.Value;

					var loadString = loadingType.GetName();

					pdfFields.SetField("Load" + loadString, loadingResult.Loading.ToString("0.0") + " t");
					pdfFields.SetField("Speed" + loadString, loadingResult.Speed.ToString("0.0"));
					pdfFields.SetField("FCkm" + loadString, loadingResult.FCkm.ToString("0.0"));

					if (loadingResult.Loading == 0) {
						pdfFields.SetField("FCtkm" + loadString, "-");
						pdfFields.SetField("CO2tkm" + loadString, "-");
					} else {
						pdfFields.SetField("FCtkm" + loadString, loadingResult.FCtkm.ToString("0.0"));
						pdfFields.SetField("CO2tkm" + loadString, loadingResult.CO2tkm.ToString("0.0"));
					}

					pdfFields.SetField("CO2km" + loadString, loadingResult.CO2km.ToString("0.0"));
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

		public class LoadingResults
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

		public class MissionResults
		{
			public System.Drawing.Image ChartSpeed;
			public System.Drawing.Image ChartTqN;

			public Mission Mission;
			public Dictionary<LoadingType, LoadingResults> Results = new Dictionary<LoadingType, LoadingResults>();
		}
	}

	public enum LoadingType
	{
		Full,
		Reference,
		Empty,
		UserDefined
	}

	public static class LoadingTypeHelper
	{
		private static readonly Dictionary<LoadingType, string> LoadingTypeToString = new Dictionary<LoadingType, string> {
			{ LoadingType.Empty, "E" },
			{ LoadingType.Full, "F" },
			{ LoadingType.Reference, "R" }
		};


		public static string GetName(this LoadingType loadingType)
		{
			return LoadingTypeToString[loadingType];
		}
	}
}