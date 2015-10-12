using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
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
		public class ResultContainer
		{
			public Mission Mission;
			public Bitmap ChartTqN;
			public Bitmap ChartCycle;
			public Dictionary<LoadingType, IModalDataWriter> ModData = new Dictionary<LoadingType, IModalDataWriter>();
		}

		private readonly Dictionary<MissionType, ResultContainer> _missions = new Dictionary<MissionType, ResultContainer>();
		private Bitmap _chartCO2speed;
		private Bitmap _chartCO2Missions;

		private readonly FullLoadCurve _flc;
		private readonly Segment _segment;

		private readonly string _creator;
		private readonly string _engineModel;
		private readonly string _engineStr;
		private readonly string _outputFilePath;
		private readonly string _gearboxModel;
		private readonly string _gearboxStr;
		private readonly string _jobFile;

		public Report(FullLoadCurve flc, Segment segment, string creator, string engineModel, string engineStr,
			string outputFilePath, string gearboxModel, string gearboxStr, string jobFile)
		{
			_flc = flc;
			_segment = segment;
			_creator = creator;
			_engineModel = engineModel;
			_engineStr = engineStr;
			_outputFilePath = outputFilePath;
			_gearboxModel = gearboxModel;
			_gearboxStr = gearboxStr;
			_jobFile = jobFile;
		}

		public void AddResult(LoadingType loadingType, Mission mission, IModalDataWriter modData)
		{
			if (!_missions.ContainsKey(mission.MissionType)) {
				_missions[mission.MissionType] = new ResultContainer {
					Mission = mission,
				};
			}

			_missions[mission.MissionType].ModData[loadingType] = modData;


			//ChartTqN = DrawOperatingPointsChart(modData, _flc),
			//ChartCycle = DrawCycleChart(modData)
		}


		private static Bitmap DrawCycleChart(ResultContainer results)
		{
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

			var distance = results.ModData.First().Value.GetValues<SI>(ModalResultField.dist).ToDouble().ToList();
			var altitudeValues = results.ModData.First().Value.GetValues<SI>(ModalResultField.altitude).ToDouble().ToList();

			altitude.Points.DataBindXY(distance, altitudeValues);
			missionCycleChart.Series.Add(altitude);

			var targetSpeed = new Series { ChartType = SeriesChartType.FastLine, BorderWidth = 3, Name = "Target speed" };
			targetSpeed.Points.DataBindXY(distance,
				results.ModData.First().Value.GetValues<SI>(ModalResultField.v_targ).ToDouble());
			missionCycleChart.Series.Add(targetSpeed);

			foreach (var result in results.ModData) {
				var name = result.Key.ToString();
				var values = result.Value;

				var series = new Series { ChartType = SeriesChartType.FastLine, Name = name };
				series.Points.DataBindXY(values.GetValues<SI>(ModalResultField.dist), values.GetValues<SI>(ModalResultField.v_act));
				missionCycleChart.Series.Add(series);
			}
			missionCycleChart.Update();

			var cycleChart = new Bitmap(missionCycleChart.Width, missionCycleChart.Height, PixelFormat.Format32bppArgb);
			missionCycleChart.DrawToBitmap(cycleChart, new Rectangle(0, 0, missionCycleChart.Width, missionCycleChart.Height));
			return cycleChart;
		}


		private static Bitmap DrawOperatingPointsChart(IModalDataWriter modData, FullLoadCurve flc)
		{
			var operatingPointsChart = new Chart { Width = 1000, Height = 427 };
			operatingPointsChart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 14),
				BorderColor = Color.Black,
				BorderWidth = 3
			});

			operatingPointsChart.ChartAreas.Add(new ChartArea("main") {
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

			var n = flc.FullLoadEntries.Select(x => x.EngineSpeed).ToDouble().ToList();
			var torqueFull = flc.FullLoadEntries.Select(x => x.TorqueFullLoad).ToDouble();
			var torqueDrag = flc.FullLoadEntries.Select(x => x.TorqueDrag).ToDouble();

			var fullLoadCurve = new Series("Full load curve") {
				ChartType = SeriesChartType.FastLine,
				BorderWidth = 3,
				Color = Color.DarkBlue
			};
			fullLoadCurve.Points.DataBindXY(n, torqueFull);
			operatingPointsChart.Series.Add(fullLoadCurve);

			var dragLoadCurve = new Series("Drag curve") {
				ChartType = SeriesChartType.FastLine,
				BorderWidth = 3,
				Color = Color.Blue
			};
			dragLoadCurve.Points.DataBindXY(n, torqueDrag);
			operatingPointsChart.Series.Add(dragLoadCurve);

			var dataPoints = new Series("load points (Ref. load.)") { ChartType = SeriesChartType.Point, Color = Color.Red };
			dataPoints.Points.DataBindXY(modData.GetValues<SI>(ModalResultField.n).ToDouble(),
				modData.GetValues<SI>(ModalResultField.Tq_eng).ToDouble());
			operatingPointsChart.Series.Add(dataPoints);

			operatingPointsChart.Update();

			var tqnBitmap = new Bitmap(operatingPointsChart.Width, operatingPointsChart.Height, PixelFormat.Format32bppArgb);
			operatingPointsChart.DrawToBitmap(tqnBitmap, new Rectangle(0, 0, tqnBitmap.Width, tqnBitmap.Height));
			return tqnBitmap;
		}


		public void CreateCharts()
		{
			_chartCO2Missions = DrawCO2MissionsChart(_missions);
			_chartCO2speed = DrawCO2SpeedChart(_missions);
		}

		private static Bitmap DrawCO2MissionsChart(Dictionary<MissionType, ResultContainer> missions)
		{
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


			foreach (var missionResult in missions) {
				var series = new Series(missionResult.Key + " (Ref. load.)");
				series.Points[0].Label = series.Points[0].YValues[0].ToString("0.0") + " [g/tkm]";
				series.Points[0].Font = new Font("Helvetica", 20);
				series.Points[0].LabelBackColor = Color.White;

				var co2sum = missionResult.Value.ModData[LoadingType.Reference].GetValues<SI>(ModalResultField.FCMap).Sum();

				var maxDistance = missionResult.Value.ModData[LoadingType.Reference].GetValues<SI>(ModalResultField.dist).Max();
				var loading = GetLoading(LoadingType.Reference, missionResult.Value.Mission);
				var co2pertkm = co2sum / maxDistance / loading * 1000;

				series.Points.AddXY(missionResult.Key, co2pertkm.Value());
				co2Chart.Series.Add(series);
			}

			co2Chart.Update();

			var chartCO2tkm = new Bitmap(co2Chart.Width, co2Chart.Height, PixelFormat.Format32bppArgb);
			co2Chart.DrawToBitmap(chartCO2tkm, new Rectangle(0, 0, chartCO2tkm.Width, chartCO2tkm.Height));
			return chartCO2tkm;
		}

		private static Bitmap DrawCO2SpeedChart(Dictionary<MissionType, ResultContainer> missions)
		{
			var co2speedChart = new Chart { Width = 1500, Height = 700 };
			co2speedChart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 20),
				BorderColor = Color.Black,
				BorderWidth = 3,
			});
			co2speedChart.ChartAreas.Add(new ChartArea("main") {
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

			foreach (var missionResult in missions) {
				var series = new Series { MarkerSize = 15, MarkerStyle = MarkerStyle.Circle, ChartType = SeriesChartType.Point };
				foreach (var pair in missionResult.Value.ModData) {
					var dt = pair.Value.GetValues<SI>(ModalResultField.simulationInterval).ToDouble();
					var speed = pair.Value.GetValues<SI>(ModalResultField.v_act).ToDouble();
					var distance = pair.Value.GetValues<SI>(ModalResultField.dist).Max().Value();

					//todo get co2 value
					var co2 = pair.Value.GetValues<SI>(ModalResultField.FCMap).ToDouble();

					var avgSpeed = speed.Zip(dt, (v, t) => v / t).Average();
					var avgCO2km = co2.Zip(dt, (co, t) => co / t).Average() / distance * 1000;

					var loading = GetLoading(pair.Key, missionResult.Value.Mission);

					var point = new DataPoint(avgSpeed, avgCO2km) {
						Label = loading.Value().ToString("0.0") + " t",
						Font = new Font("Helvetica", 16),
						LabelBackColor = Color.White
					};

					if (pair.Key != LoadingType.Reference) {
						point.MarkerSize = 10;
						point.Font = new Font("Helvetica", 14);
					}
					series.Points.Add(point);
				}
				series.Name = missionResult.Key.ToString();
				co2speedChart.Series.Add(series);
			}

			co2speedChart.Update();
			var chartCO2speed = new Bitmap(co2speedChart.Width, co2speedChart.Height, PixelFormat.Format32bppArgb);
			co2speedChart.DrawToBitmap(chartCO2speed, new Rectangle(0, 0, co2speedChart.Width, co2speedChart.Height));
			return chartCO2speed;
		}


		public void WritePdfs(Dictionary<MissionType, ResultContainer> missions)
		{
			var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

			var pgMax = missions.Count;
			var filename = string.Format("Reports\rep{0}C.pdf", pgMax);
			var temppdfs = new List<string>();

			var reportOutputPath = Path.Combine(assemblyPath, "Declaration");

			var pdfTempMR = reportOutputPath + @"Reports\repMR.pdf";
			var temppath = reportOutputPath + @"Reports\temp0.pdf";
			temppdfs.Add(temppath);

			WriteOverviewPDF(missions, filename, temppath);

			var i = 1;
			foreach (var results in missions.Values) {
				temppath = reportOutputPath + @"Reports\temp" + i + ".pdf";
				temppdfs.Add(temppath);

				WriteMissionPDF(pdfTempMR, temppath, i, pgMax, results);
				i++;
			}

			MergeDocuments(temppdfs, temppath);
		}

		private void WriteOverviewPDF(Dictionary<MissionType, ResultContainer> missions, string filename,
			string temppath)
		{
			var reader = new PdfReader(filename);
			var stamper = new PdfStamper(reader, new FileStream(temppath, FileMode.Create));

			var pdfFields = stamper.AcroFields;
			pdfFields.SetField("version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			pdfFields.SetField("Job", _jobFile);
			pdfFields.SetField("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture));
			pdfFields.SetField("Created", _creator);
			pdfFields.SetField("Config",
				string.Format("{0:0.0}t {1} {2}", _segment.GrossVehicleWeightMax / 1000, _segment.AxleConfiguration.GetName(),
					_segment.VehicleCategory));
			pdfFields.SetField("HDVclass", "HDV Class " + _segment.VehicleClass);
			pdfFields.SetField("Engine", _engineStr);
			pdfFields.SetField("EngM", _engineModel);
			pdfFields.SetField("Gearbox", _gearboxStr);
			pdfFields.SetField("GbxM", _gearboxModel);
			pdfFields.SetField("PageNr", "Page 1 of " + missions.Count);

			var i = 1;
			foreach (var results in missions.Values) {
				pdfFields.SetField("Mission" + i, results.Mission.MissionType.ToString());

				var m = results.ModData[LoadingType.Reference];
				var dt = m.GetValues<SI>(ModalResultField.simulationInterval);
				var distance = m.GetValues<SI>(ModalResultField.dist).Max().Value();

				Func<ModalResultField, double> avgWeighted =
					field => m.GetValues<SI>(field).Zip(dt, (v, t) => v / t).Average().Value();

				pdfFields.SetField("Loading" + i, results.Mission.RefLoad.Value().ToString("0.0") + " t");
				pdfFields.SetField("Speed" + i, avgWeighted(ModalResultField.v_act).ToString("0.0") + " km/h");

				var fc = avgWeighted(ModalResultField.FCMap) / distance * 1000;
				// todo: calc FCt, co2, co2t
				pdfFields.SetField("FC" + i, fc.ToString("0.0"));
				pdfFields.SetField("FCt" + i, (fc * 1000).ToString("0.0"));
				pdfFields.SetField("CO2" + i, fc.ToString("0.0"));
				pdfFields.SetField("CO2t" + i, fc.ToString("0.0"));
				i++;
			}

			// Add Images
			var content = stamper.GetOverContent(1);
			var img = Image.GetInstance(_chartCO2Missions, BaseColor.WHITE);
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 270);
			content.AddImage(img);

			img = Image.GetInstance(_chartCO2speed, BaseColor.WHITE);
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
		}

		private void WriteMissionPDF(string pdfTempMR, string temppath, int i, int pgMax, ResultContainer results)
		{
			var reader = new PdfReader(pdfTempMR);
			var stamper = new PdfStamper(reader, new FileStream(temppath, FileMode.Create));

			var pdfFields = stamper.AcroFields;
			pdfFields.SetField("version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			pdfFields.SetField("Job", _jobFile);
			pdfFields.SetField("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture));
			pdfFields.SetField("Created", _creator);
			pdfFields.SetField("Config",
				string.Format("{0:0.0}t {1} {2}", _segment.GrossVehicleWeightMax / 1000, _segment.AxleConfiguration.GetName(),
					_segment.VehicleCategory));
			pdfFields.SetField("HDVclass", "HDV Class " + _segment.VehicleClass);
			pdfFields.SetField("PageNr", "Page " + (i + 1) + " of " + pgMax);
			pdfFields.SetField("Mission", results.Mission.MissionType.ToString());


			foreach (var pair in results.ModData) {
				var loadingType = pair.Key;
				var m = pair.Value;

				var loadString = loadingType.GetName();
				pdfFields.SetField("Load" + loadString, GetLoading(loadingType, results.Mission).ToString("0.0") + " t");

				var dt = m.GetValues<SI>(ModalResultField.simulationInterval);
				var distance = m.GetValues<SI>(ModalResultField.dist).Max().Value();

				Func<ModalResultField, double> avgWeighted =
					field => m.GetValues<SI>(field).Zip(dt, (v, t) => v / t).Average().Value();

				pdfFields.SetField("Speed" + loadString, avgWeighted(ModalResultField.v_act).ToString("0.0"));

				var fc = avgWeighted(ModalResultField.FCMap) / distance * 1000;
				var co2 = fc;

				var loading = GetLoading(loadingType, results.Mission);

				pdfFields.SetField("FCkm" + loadString, fc.ToString("0.0"));
				pdfFields.SetField("CO2km" + loadString, co2.ToString("0.0"));

				if (loading.IsEqual(0)) {
					pdfFields.SetField("FCtkm" + loadString, "-");
					pdfFields.SetField("CO2tkm" + loadString, "-");
				} else {
					pdfFields.SetField("FCtkm" + loadString, (fc / loading).Value().ToString("0.0"));
					pdfFields.SetField("CO2tkm" + loadString, (fc / loading).ToString("0.0"));
				}
			}

			var content = stamper.GetOverContent(1);

			//todo hdvClass Image Path
			var hdvClassImagePath = "";

			var img = Image.GetInstance(hdvClassImagePath);
			img.ScaleAbsolute(180, 50);
			img.SetAbsolutePosition(600, 475);
			content.AddImage(img);

			img = Image.GetInstance(results.ChartCycle, BaseColor.WHITE);
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


		private void MergeDocuments(IEnumerable<string> temppdfs, string temppath)
		{
			var document = new Document(PageSize.A4.Rotate(), 12, 12, 12, 12);
			var writer = PdfWriter.GetInstance(document, new FileStream(_outputFilePath, FileMode.Create));

			document.Open();

			foreach (var path in temppdfs) {
				var importedPage = writer.GetImportedPage(new PdfReader(temppath), 1);
				document.Add(Image.GetInstance(importedPage));
				File.Delete(path);
			}

			document.Close();
		}


		private static Kilogram GetLoading(LoadingType loadingType, Mission mission)
		{
			return loadingType == LoadingType.Full
				? mission.MaxLoad
				: loadingType == LoadingType.Reference
					? mission.RefLoad
					: mission.MinLoad;
		}
	}
}