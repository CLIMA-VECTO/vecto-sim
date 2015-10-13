using System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms.DataVisualization.Charting;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;
using Font = System.Drawing.Font;
using Image = iTextSharp.text.Image;
using Rectangle = System.Drawing.Rectangle;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationReport
	{
		private class ResultContainer
		{
			public Mission Mission;
			public Dictionary<LoadingType, IModalDataWriter> ModData;
		}

		private readonly Dictionary<MissionType, ResultContainer> _missions = new Dictionary<MissionType, ResultContainer>();

		private readonly FullLoadCurve _flc;
		private readonly Segment _segment;

		private readonly string _creator;
		private readonly string _engineModel;
		private readonly string _engineStr;
		private readonly string _gearboxModel;
		private readonly string _gearboxStr;
		private readonly string _jobFile;
		private readonly int _resultCount;
		private readonly string _basePath;

		public DeclarationReport(FullLoadCurve flc, Segment segment, string creator, string engineModel, string engineStr,
			string gearboxModel, string gearboxStr, string basePath, string jobFile, int resultCount)
		{
			_flc = flc;
			_segment = segment;
			_creator = creator;
			_engineModel = engineModel;
			_engineStr = engineStr;
			_gearboxModel = gearboxModel;
			_gearboxStr = gearboxStr;
			_jobFile = jobFile;
			_resultCount = resultCount;
			_basePath = basePath;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddResult(LoadingType loadingType, Mission mission, IModalDataWriter modData)
		{
			if (!_missions.ContainsKey(mission.MissionType)) {
				_missions[mission.MissionType] = new ResultContainer {
					Mission = mission,
					ModData = new Dictionary<LoadingType, IModalDataWriter> { { loadingType, modData } }
				};
			} else {
				_missions[mission.MissionType].ModData[loadingType] = modData;
			}

			if (_resultCount == _missions.Sum(v => v.Value.ModData.Count)) {
				WriteReport();
			}
		}

		private void WriteReport()
		{
			var titlePage = CreateTitlePage(_missions);
			var cyclePages = _missions.Select((m, i) => CreateCyclePage(m.Value, i, _missions.Count));

			MergeDocuments(titlePage, cyclePages, Path.Combine(_basePath, _jobFile + ".pdf"));
		}

		private Stream CreateTitlePage(Dictionary<MissionType, ResultContainer> missions)
		{
			var stream = new MemoryStream();

			var reader = new PdfReader(RessourceHelper.ReadStream(RessourceHelper.Namespace + "Report.titlePageTemplate.pdf"));
			var stamper = new PdfStamper(reader, stream);

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

				var m = results.ModData[LoadingType.ReferenceLoad];
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
			var img = Image.GetInstance(DrawCO2MissionsChart(missions), BaseColor.WHITE);
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 270);
			content.AddImage(img);

			img = Image.GetInstance(DrawCO2SpeedChart(missions), BaseColor.WHITE);
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 75);
			content.AddImage(img);

			img =
				Image.GetInstance(
					RessourceHelper.ReadStream(RessourceHelper.Namespace + "Report." +
												GetHDVClassImageName(_segment, MissionType.LongHaul)));
			img.ScaleAbsolute(180, 50);
			img.SetAbsolutePosition(30, 475);
			content.AddImage(img);

			// flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
			stamper.FormFlattening = true;
			stamper.Writer.CloseStream = false;
			stamper.Close();

			return stream;
		}

		private static string GetHDVClassImageName(Segment segment, MissionType missionType)
		{
			switch (segment.VehicleClass) {
				case VehicleClass.Class1:
				case VehicleClass.Class2:
				case VehicleClass.Class3:
					return "4x2r.png";
				case VehicleClass.Class4:
					return missionType == MissionType.LongHaul ? "4x2rt.png" : "4x2r.png";
				case VehicleClass.Class5:
					return "4x2tt.png";
				case VehicleClass.Class9:
					return missionType == MissionType.LongHaul ? "6x2rt.png" : "6x2r.png";
				case VehicleClass.Class10:
					return "6x2tt.png";
			}
			return "Undef.png";
		}

		private Stream CreateCyclePage(ResultContainer results, int i, int pgMax)
		{
			var stream = new MemoryStream();
			var cyclePages = RessourceHelper.Namespace + string.Format("Report.report{0}CyclesTemplate.pdf", pgMax);
			var reader = new PdfReader(RessourceHelper.ReadStream(cyclePages));
			var stamper = new PdfStamper(reader, stream);

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

				var loadString = loadingType.GetShortName();
				pdfFields.SetField("Load" + loadString, results.Mission.Loadings[loadingType].ToString("0.0") + " t");

				var dt = m.GetValues<SI>(ModalResultField.simulationInterval);
				var distance = m.GetValues<SI>(ModalResultField.dist).Max().Value();

				Func<ModalResultField, double> avgWeighted =
					field => m.GetValues<SI>(field).Zip(dt, (v, t) => v / t).Average().Value();

				pdfFields.SetField("Speed" + loadString, avgWeighted(ModalResultField.v_act).ToString("0.0"));

				var fc = avgWeighted(ModalResultField.FCMap) / distance * 1000;
				var co2 = fc;

				var loading = results.Mission.Loadings[loadingType];

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

			var img =
				Image.GetInstance(
					RessourceHelper.ReadStream(RessourceHelper.Namespace + "Report." +
												GetHDVClassImageName(_segment, results.Mission.MissionType)));
			img.ScaleAbsolute(180, 50);
			img.SetAbsolutePosition(600, 475);
			content.AddImage(img);

			img = Image.GetInstance(DrawCycleChart(results), BaseColor.WHITE);
			img.ScaleAbsolute(780, 156);
			img.SetAbsolutePosition(17, 270);
			content.AddImage(img);

			img = Image.GetInstance(DrawOperatingPointsChart(results.ModData[LoadingType.ReferenceLoad], _flc), BaseColor.WHITE);
			img.ScaleAbsolute(420, 178);
			img.SetAbsolutePosition(375, 75);
			content.AddImage(img);

			// flatten the form to remove editting options, set it to false  to leave the form open to subsequent manual edits
			stamper.FormFlattening = true;

			stamper.Writer.CloseStream = false;
			stamper.Close();
			return stream;
		}

		private static void MergeDocuments(Stream titlePage, IEnumerable<Stream> cyclePages, string outputFileName)
		{
			var document = new Document(PageSize.A4.Rotate(), 12, 12, 12, 12);
			var writer = PdfWriter.GetInstance(document, new FileStream(outputFileName, FileMode.Create));

			document.Open();

			titlePage.Position = 0;
			document.Add(Image.GetInstance(writer.GetImportedPage(new PdfReader(titlePage), 1)));

			foreach (var cyclePage in cyclePages) {
				cyclePage.Position = 0;
				document.Add(Image.GetInstance(writer.GetImportedPage(new PdfReader(cyclePage), 1)));
			}

			document.Close();
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
				var co2sum = missionResult.Value.ModData[LoadingType.ReferenceLoad].GetValues<SI>(ModalResultField.FCMap).Sum();

				var maxDistance = missionResult.Value.ModData[LoadingType.ReferenceLoad].GetValues<SI>(ModalResultField.dist).Max();
				var loading = missionResult.Value.Mission.Loadings[LoadingType.ReferenceLoad];
				var co2pertkm = co2sum / maxDistance / loading * 1000;

				series.Points.AddXY(missionResult.Key.ToString(), co2pertkm.Value());

				series.Points[0].Label = series.Points[0].YValues[0].ToString("0.0") + " [g/tkm]";
				series.Points[0].Font = new Font("Helvetica", 20);
				series.Points[0].LabelBackColor = Color.White;

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

					var loading = missionResult.Value.Mission.Loadings[pair.Key];

					var point = new DataPoint(avgSpeed, avgCO2km) {
						Label = loading.Value().ToString("0.0") + " t",
						Font = new Font("Helvetica", 16),
						LabelBackColor = Color.White
					};

					if (pair.Key != LoadingType.ReferenceLoad) {
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

			var distance = results.ModData.First().Value.GetValues<SI>(ModalResultField.dist).ToDouble();
			var altitudeValues = results.ModData.First().Value.GetValues<SI>(ModalResultField.altitude).ToDouble();

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
				series.Points.DataBindXY(values.GetValues<SI>(ModalResultField.dist).ToDouble(),
					values.GetValues<SI>(ModalResultField.v_act).ToDouble());
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

			var n = flc.FullLoadEntries.Select(x => x.EngineSpeed).ToDouble();
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
	}
}