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


	public class Report
	{
		public Mission Mission;
		public Dictionary<LoadingType, IModalDataWriter> modData = new Dictionary<LoadingType, IModalDataWriter>();

		public System.Drawing.Image ChartSpeed;
		public System.Drawing.Image ChartTqN;
		public System.Drawing.Image ChartCO2speed;
		public System.Drawing.Image ChartCO2tkm;

		public AxleConfiguration AxleConf;
		public VehicleCategory VehCat;
		private readonly FullLoadCurve _flc;

		public string Creator = "";
		public string DateStr = "";
		public string EngModelStr = "";
		public string EngStr = "";
		public string Filepath = "";
		public string GbxModelStr = "";
		public string GbxStr = "";
		public string HDVclassStr = "";
		public string JobFile = "";
		public string MassMaxStr = "";

		public Report(IModalDataWriter fullLoad, IModalDataWriter referenceLoad, IModalDataWriter emptyLoad, FullLoadCurve flc)
		{
			modData[LoadingType.Full] = fullLoad;
			modData[LoadingType.Empty] = emptyLoad;
			modData[LoadingType.Reference] = referenceLoad;
			_flc = flc;
		}


		public void CreateCharts()
		{
			var missionOperatingPointsChart = new Chart { Width = 1000, Height = 427 };
			missionOperatingPointsChart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 14),
				BorderColor = Color.Black,
				BorderWidth = 3
			});

			missionOperatingPointsChart.ChartAreas.Add(new ChartArea("main") {
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
				Position = { X = 0, Y = 0, Width = 70, Height = 100 }
			});

			var n = _flc.FullLoadEntries.Select(x => x.EngineSpeed).ToDouble().ToList();
			var torqueFull = _flc.FullLoadEntries.Select(x => x.TorqueFullLoad).ToDouble();
			var torqueDrag = _flc.FullLoadEntries.Select(x => x.TorqueDrag).ToDouble();

			var fullLoadCurve = new Series("Full load curve") {
				ChartType = SeriesChartType.FastLine,
				BorderWidth = 3,
				Color = Color.DarkBlue
			};
			fullLoadCurve.Points.DataBindXY(n, torqueFull);
			missionOperatingPointsChart.Series.Add(fullLoadCurve);

			var dragLoadCurve = new Series("Drag curve") {
				ChartType = SeriesChartType.FastLine,
				BorderWidth = 3,
				Color = Color.Blue
			};
			dragLoadCurve.Points.DataBindXY(n, torqueDrag);
			missionOperatingPointsChart.Series.Add(dragLoadCurve);

			var dataPoints = new Series("load points (Ref. load.)") { ChartType = SeriesChartType.Point, Color = Color.Red };
			dataPoints.Points.DataBindXY(modData[LoadingType.Reference].GetValues<SI>(ModalResultField.n).ToDouble(),
				modData[LoadingType.Reference].GetValues<SI>(ModalResultField.Tq_eng).ToDouble());
			missionOperatingPointsChart.Series.Add(dataPoints);

			missionOperatingPointsChart.Update();

			ChartTqN = new Bitmap(missionOperatingPointsChart.Width, missionOperatingPointsChart.Height,
				PixelFormat.Format32bppArgb);
			missionOperatingPointsChart.DrawToBitmap((Bitmap)ChartTqN,
				new Rectangle(0, 0, ChartTqN.Size.Width, ChartTqN.Size.Height));
			//----------------------------------------------------------------------------------------------------

			var missionCycleChart = new Chart { Width = 2000, Height = 400 };
			missionCycleChart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 14),
				BorderColor = Color.Black,
				BorderWidth = 3,
				Position = { X = 97, Y = 3, Width = 10, Height = 40 }
			});

			missionCycleChart.ChartAreas.Add(new ChartArea {
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

			var distance = modData[LoadingType.Reference].GetValues<SI>(ModalResultField.dist).ToDouble().ToList();


			// todo: altitude in moddata
			altitude.Points.DataBindXY(distance,
				modData[LoadingType.Reference].GetValues<SI>(ModalResultField.altitude).ToDouble());
			missionCycleChart.Series.Add(altitude);

			var targetSpeed = new Series { ChartType = SeriesChartType.FastLine, BorderWidth = 3, Name = "Target speed" };
			targetSpeed.Points.DataBindXY(distance,
				modData[LoadingType.Reference].GetValues<SI>(ModalResultField.v_targ).ToDouble());
			missionCycleChart.Series.Add(targetSpeed);

			foreach (var result in modData) {
				var name = result.Key.GetName();
				var values = result.Value;

				var series = new Series { ChartType = SeriesChartType.FastLine, Name = name };
				series.Points.DataBindXY(values.GetValues<SI>(ModalResultField.dist), values.GetValues<SI>(ModalResultField.v_act));
				missionCycleChart.Series.Add(series);
			}
			missionCycleChart.Update();

			ChartSpeed = new Bitmap(missionCycleChart.Width, missionCycleChart.Height, PixelFormat.Format32bppArgb);
			missionCycleChart.DrawToBitmap((Bitmap)ChartSpeed,
				new Rectangle(0, 0, ChartSpeed.Size.Width, ChartSpeed.Size.Height));

			//----------------------------------------------------------------------------------------------------


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
				series.Points[0].Font = new Font("Helvetica", 20);
				series.Points[0].LabelBackColor = Color.White;

				series.Points.AddXY(results.Mission.ToString(), results.Results[LoadingType.Reference].CO2tkm);
				co2Chart.Series.Add(series);
			}

			co2Chart.Update();
			ChartCO2tkm = new Bitmap(co2Chart.Width, co2Chart.Height, PixelFormat.Format32bppArgb);
			co2Chart.DrawToBitmap((Bitmap)ChartCO2tkm, new Rectangle(0, 0, ChartCO2tkm.Size.Width, ChartCO2tkm.Size.Height));


			var chart = new Chart { Width = 1500, Height = 700 };
			chart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 20),
				BorderColor = Color.Black,
				BorderWidth = 3,
			});
			chart.ChartAreas.Add(new ChartArea("main") {
				BorderDashStyle = ChartDashStyle.Solid,
				BorderWidth = 3,
				AxisX = {
					Title = "vehicle speed [km/h]",
					TitleFont = new Font("Helvetica", 20),
					LabelStyle = { Font = new Font("Helvetica", 20) },
					LabelAutoFitStyle = LabelAutoFitStyles.None,
					Minimum = 20.0,
				},
				AxisY = {
					Title = "CO2 [g/km]",
					TitleFont = new Font("Helvetica", 20),
					LabelStyle = { Font = new Font("Helvetica", 20) },
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
						series.Points[num].Font = new Font("Helvetica", 16);
					} else {
						series.Points[num].MarkerSize = 10;
						series.Points[num].Font = new Font("Helvetica", 14);
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
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 270);
			content.AddImage(img);

			img = Image.GetInstance(ChartCO2speed, BaseColor.WHITE);
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 75);
			content.AddImage(img);

			//todo get image for hdv class
			var hdvClassImagePath = "";
			img = Image.GetInstance(hdvClassImagePath);
			img.ScaleAbsolute(180, 50);
			img.SetAbsolutePosition(30, 475);
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
				img.ScaleAbsolute(180, 50);
				img.SetAbsolutePosition(600, 475);
				content.AddImage(img);

				img = Image.GetInstance(results.ChartSpeed, BaseColor.WHITE);
				img.ScaleAbsolute(780, 156);
				img.SetAbsolutePosition(17, 270);
				content.AddImage(img);

				img = Image.GetInstance(results.ChartTqN, BaseColor.WHITE);
				img.ScaleAbsolute(420, 178);
				img.SetAbsolutePosition(375, 75);
				content.AddImage(img);

				// flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
				stamper.FormFlattening = true;

				// close the pdf
				stamper.Close();
			}

			// Merge files
			var document = new Document(PageSize.A4.Rotate(), 12, 12, 12, 12);
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
	}
}