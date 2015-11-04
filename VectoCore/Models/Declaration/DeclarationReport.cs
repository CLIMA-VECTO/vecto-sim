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
	/// <summary>
	/// Class for creating a declaration report.
	/// </summary>
	public class DeclarationReport
	{
		/// <summary>
		/// Container class for one mission and the modData for the different loadings.
		/// </summary>
		private class ResultContainer
		{
			public Mission Mission;
			public Dictionary<LoadingType, IModalDataWriter> ModData;
		}

		/// <summary>
		/// Data Dictionary for all missions.
		/// </summary>
		private readonly Dictionary<MissionType, ResultContainer> _missions = new Dictionary<MissionType, ResultContainer>();

		/// <summary>
		/// The full load curve.
		/// </summary>
		private readonly FullLoadCurve _flc;

		/// <summary>
		/// The declaration segment from the segment table
		/// </summary>
		private readonly Segment _segment;


		/// <summary>
		/// The creator name for the report.
		/// </summary>
		private readonly string _creator;


		/// <summary>
		/// The engine model string from engine file.
		/// </summary>
		private readonly string _engineModel;

		/// <summary>
		/// The engine description (displacement and max power)
		/// </summary>
		private readonly string _engineStr;

		/// <summary>
		/// The gearbox model string from gearbox file.
		/// </summary>
		private readonly string _gearboxModel;

		/// <summary>
		/// The gearbox description (gear-count and gear type)
		/// </summary>
		private readonly string _gearboxStr;

		/// <summary>
		/// The name of the job file (report name will be the same)
		/// </summary>
		private readonly string _jobFile;

		/// <summary>
		/// The result count determines how many results must be given before the report gets written.
		/// </summary>
		private readonly int _resultCount;

		/// <summary>
		/// The base path of the application
		/// </summary>
		private readonly string _basePath;

		/// <summary>
		/// Initializes a new instance of the <see cref="DeclarationReport"/> class.
		/// </summary>
		/// <param name="flc">The full load curve.</param>
		/// <param name="segment">The segment of the current vehicle from the segment table.</param>
		/// <param name="creator">The creator name.</param>
		/// <param name="engineModel">The engine model.</param>
		/// <param name="engineStr">The engine description string.</param>
		/// <param name="gearboxModel">The gearbox model.</param>
		/// <param name="gearboxStr">The gearbox description string.</param>
		/// <param name="basePath">The base path.</param>
		/// <param name="jobFile">The name of the job file.</param>
		/// <param name="resultCount">The result count which defines after how many finished results the report gets written.</param>
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


		/// <summary>
		/// Adds the result of one run for the specific mission and loading. If all runs finished (given by the resultCount) the report will be written.
		/// </summary>
		/// <param name="loadingType">Type of the loading.</param>
		/// <param name="mission">The mission.</param>
		/// <param name="modData">The mod data.</param>
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


		/// <summary>
		/// Creates the report and writes it to a pdf file.
		/// </summary>
		private void WriteReport()
		{
			var titlePage = CreateTitlePage(_missions);
			var cyclePages = _missions.OrderBy(m => m.Key).Select((m, i) => CreateCyclePage(m.Value, i + 2, _missions.Count + 1));

			MergeDocuments(titlePage, cyclePages, Path.Combine(_basePath, _jobFile + ".pdf"));
		}


		/// <summary>
		/// Creates the title page.
		/// </summary>
		/// <param name="missions">The missions.</param>
		/// <returns>the out stream of the pdf stamper with the title page.</returns>
		private Stream CreateTitlePage(Dictionary<MissionType, ResultContainer> missions)
		{
			var stream = new MemoryStream();

			var resourceName = string.Format("{0}Report.title{1}CyclesTemplate.pdf", RessourceHelper.Namespace, missions.Count);
			var inputStream = RessourceHelper.ReadStream(resourceName);

			var reader = new PdfReader(inputStream);

			var stamper = new PdfStamper(reader, stream);

			var pdfFields = stamper.AcroFields;
			pdfFields.SetField("version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			pdfFields.SetField("Job", _jobFile);
			pdfFields.SetField("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture));
			pdfFields.SetField("Created", _creator);
			pdfFields.SetField("Config",
				string.Format(CultureInfo.InvariantCulture, "{0}t {1} {2}",
					_segment.GrossVehicleMassRating.ConvertTo().Ton.ToOutputFormat(1),
					_segment.AxleConfiguration.GetName(), _segment.VehicleCategory));
			pdfFields.SetField("HDVclass", "HDV Class " + _segment.VehicleClass.GetClassNumber());
			pdfFields.SetField("Engine", _engineStr);
			pdfFields.SetField("EngM", _engineModel);
			pdfFields.SetField("Gearbox", _gearboxStr);
			pdfFields.SetField("GbxM", _gearboxModel);
			pdfFields.SetField("PageNr", string.Format("Page {0} of {1}", 1, missions.Count + 1));


			var i = 1;
			foreach (var results in missions.Values.OrderBy(m => m.Mission.MissionType)) {
				pdfFields.SetField("Mission" + i, results.Mission.MissionType.ToString());

				var m = results.ModData[LoadingType.ReferenceLoad];
				var dt = m.GetValues<Second>(ModalResultField.simulationInterval);
				var maxDistance = m.GetValues<Meter>(ModalResultField.dist).Max();
				var maxTime = m.GetValues<Second>(ModalResultField.time).Max();

				var avgSpeed = maxDistance / maxTime;

				pdfFields.SetField("Loading" + i, results.Mission.RefLoad.ConvertTo().Ton.ToOutputFormat(1) + " t");
				pdfFields.SetField("Speed" + i, avgSpeed.ConvertTo().Kilo.Meter.Per.Hour.ToOutputFormat(1) + " km/h");

				var fc = m.GetValues<KilogramPerSecond>(ModalResultField.FCMap);
				var fcWeight = fc.Zip(dt, (fcVal, dtVal) => fcVal * dtVal).Sum();
				var fcVolume = (fcWeight / Physics.FuelDensity).Cast<CubicMeter>();
				var co2Weight = fcWeight * Physics.CO2PerFuelWeight;

				var fcAvgVolume = fcVolume / maxDistance;
				var co2AvgWeight = co2Weight / maxDistance;

				var loadingTon = results.Mission.RefLoad.ConvertTo().Ton;

				var fcLiterPer100Km = fcAvgVolume.ConvertTo().Cubic.Dezi.Meter * 100.SI().Kilo.Meter;
				var fcLiterPer100Tonkm = fcLiterPer100Km / loadingTon;
				var co2GrammPerKm = co2AvgWeight.ConvertTo().Gramm.Per.Kilo.Meter;
				var co2GrammPerTonKm = co2GrammPerKm / loadingTon;

				pdfFields.SetField("FC" + i, fcLiterPer100Km.ToOutputFormat(1));
				pdfFields.SetField("FCt" + i, fcLiterPer100Tonkm.ToOutputFormat(1));
				pdfFields.SetField("CO2" + i, co2GrammPerKm.ToOutputFormat(1));
				pdfFields.SetField("CO2t" + i, co2GrammPerTonKm.ToOutputFormat(1));
				i++;
			}

			// Add Images
			var content = stamper.GetOverContent(1);
			var img = Image.GetInstance(DrawCo2MissionsChart(missions), BaseColor.WHITE);
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 270);
			content.AddImage(img);

			img = Image.GetInstance(DrawCo2SpeedChart(missions), BaseColor.WHITE);
			img.ScaleAbsolute(440, 195);
			img.SetAbsolutePosition(360, 75);
			content.AddImage(img);

			img = GetVehicleImage(_segment, MissionType.LongHaul);
			img.ScaleAbsolute(180, 50);
			img.SetAbsolutePosition(30, 475);
			content.AddImage(img);

			stamper.FormFlattening = true;
			stamper.Writer.CloseStream = false;
			stamper.Close();

			return stream;
		}


		/// <summary>
		/// Creates the cycle page.
		/// </summary>
		/// <param name="results">The results.</param>
		/// <param name="currentPageNr">The current page nr.</param>
		/// <param name="pageCount">The page count.</param>
		/// <returns>the out stream of the pdfstamper for a single cycle page</returns>
		private Stream CreateCyclePage(ResultContainer results, int currentPageNr, int pageCount)
		{
			var stream = new MemoryStream();

			var reader = new PdfReader(RessourceHelper.ReadStream(RessourceHelper.Namespace + "Report.cyclePageTemplate.pdf"));
			var stamper = new PdfStamper(reader, stream);

			var pdfFields = stamper.AcroFields;
			pdfFields.SetField("version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			pdfFields.SetField("Job", _jobFile);
			pdfFields.SetField("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture));
			pdfFields.SetField("Created", _creator);
			pdfFields.SetField("Config",
				string.Format("{0}t {1} {2}", _segment.GrossVehicleMassRating.ConvertTo().Ton.ToOutputFormat(1),
					_segment.AxleConfiguration.GetName(), _segment.VehicleCategory));
			pdfFields.SetField("HDVclass", "HDV Class " + _segment.VehicleClass.GetClassNumber());
			pdfFields.SetField("PageNr", string.Format("Page {0} of {1}", currentPageNr, pageCount));
			pdfFields.SetField("Mission", results.Mission.MissionType.ToString());


			foreach (var pair in results.ModData) {
				var loadingType = pair.Key;
				var m = pair.Value;

				var dt = m.GetValues<Second>(ModalResultField.simulationInterval);
				var maxDistance = m.GetValues<Meter>(ModalResultField.dist).Max();
				var maxTime = m.GetValues<Second>(ModalResultField.time).Max();
				var avgSpeed = maxDistance / maxTime;

				var fc = m.GetValues<KilogramPerSecond>(ModalResultField.FCMap);
				var fcWeight = fc.Zip(dt, (fcVal, dtVal) => fcVal * dtVal).Sum();
				var fcVolume = (fcWeight / Physics.FuelDensity).Cast<CubicMeter>();
				var co2Weight = fcWeight * Physics.CO2PerFuelWeight;
				var fcAvgVolume = fcVolume / maxDistance;
				var co2AvgWeight = co2Weight / maxDistance;
				var loadingTon = results.Mission.Loadings[loadingType].ConvertTo().Ton;

				var fcLiterPer100Km = fcAvgVolume.ConvertTo().Cubic.Dezi.Meter * 100.SI().Kilo.Meter;
				var co2GrammPerKm = co2AvgWeight.ConvertTo().Gramm.Per.Kilo.Meter;

				var loadString = loadingType.GetShortName();

				pdfFields.SetField("Load" + loadString,
					results.Mission.Loadings[loadingType].ConvertTo().Ton.ToOutputFormat(1) + " t");
				pdfFields.SetField("Speed" + loadString, avgSpeed.ConvertTo().Kilo.Meter.Per.Hour.ToOutputFormat(1));
				pdfFields.SetField("FCkm" + loadString, fcLiterPer100Km.ToOutputFormat(1));
				pdfFields.SetField("FCtkm" + loadString,
					loadingTon.IsEqual(0) ? "-" : (fcLiterPer100Km / loadingTon).ToOutputFormat(1));
				pdfFields.SetField("CO2km" + loadString, co2GrammPerKm.ToOutputFormat(1));
				pdfFields.SetField("CO2tkm" + loadString,
					loadingTon.IsEqual(0) ? "-" : (co2GrammPerKm / loadingTon).ToOutputFormat(1));
			}

			var content = stamper.GetOverContent(1);

			var img = GetVehicleImage(_segment, results.Mission.MissionType);
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

			stamper.FormFlattening = true;

			stamper.Writer.CloseStream = false;
			stamper.Close();
			return stream;
		}

		/// <summary>
		/// Merges the given stream to one document and writes it to a file on disk.
		/// </summary>
		/// <param name="titlePage">The title page.</param>
		/// <param name="cyclePages">The cycle pages.</param>
		/// <param name="outputFileName">Name of the output file.</param>
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

		/// <summary>
		/// Draws the co2 missions chart for the title page.
		/// </summary>
		/// <param name="missions">The missions.</param>
		/// <returns></returns>
		private static Bitmap DrawCo2MissionsChart(Dictionary<MissionType, ResultContainer> missions)
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

			foreach (var missionResult in missions.OrderBy(m => m.Key)) {
				var m = missionResult.Value.ModData[LoadingType.ReferenceLoad];
				var fc = m.GetValues<KilogramPerSecond>(ModalResultField.FCMap);
				var dt = m.GetValues<Second>(ModalResultField.simulationInterval);

				var fcWeight = fc.Zip(dt, (fcValue, dtValue) => fcValue * dtValue).Sum();

				var maxDistance = m.GetValues<Meter>(ModalResultField.dist).Max();
				var loading = missionResult.Value.Mission.Loadings[LoadingType.ReferenceLoad];

				var co2Weight = fcWeight * Physics.CO2PerFuelWeight;

				var co2GPerTonKm = co2Weight.ConvertTo().Gramm / (maxDistance.ConvertTo().Kilo.Meter * loading.ConvertTo().Ton);

				var series = new Series(missionResult.Key + " (Ref. load.)");
				var dataPoint = new DataPoint {
					Name = missionResult.Key.ToString(),
					YValues = new[] { co2GPerTonKm.Value() },
					Label = co2GPerTonKm.ToOutputFormat(1, showUnit: true),
					Font = new Font("Helvetica", 20),
					LabelBackColor = Color.White
				};
				series.Points.Add(dataPoint);
				co2Chart.Series.Add(series);
			}

			co2Chart.Update();

			var chartCo2Tkm = new Bitmap(co2Chart.Width, co2Chart.Height, PixelFormat.Format32bppArgb);
			co2Chart.DrawToBitmap(chartCo2Tkm, new Rectangle(0, 0, chartCo2Tkm.Width, chartCo2Tkm.Height));
			return chartCo2Tkm;
		}

		/// <summary>
		/// Draws the co2 speed chart for the title page.
		/// </summary>
		/// <param name="missions">The missions.</param>
		/// <returns></returns>
		private static Bitmap DrawCo2SpeedChart(Dictionary<MissionType, ResultContainer> missions)
		{
			var co2SpeedChart = new Chart { Width = 1500, Height = 700 };
			co2SpeedChart.Legends.Add(new Legend("main") {
				Font = new Font("Helvetica", 20),
				BorderColor = Color.Black,
				BorderWidth = 3,
			});
			co2SpeedChart.ChartAreas.Add(new ChartArea("main") {
				BorderDashStyle = ChartDashStyle.Solid,
				BorderWidth = 3,
				AxisX = {
					Title = "vehicle speed [km/h]",
					TitleFont = new Font("Helvetica", 20),
					LabelStyle = { Font = new Font("Helvetica", 20) },
					LabelAutoFitStyle = LabelAutoFitStyles.None,
					Minimum = 0,
					Maximum = 80
				},
				AxisY = {
					Title = "CO2 [g/km]",
					TitleFont = new Font("Helvetica", 20),
					LabelStyle = { Font = new Font("Helvetica", 20) },
					LabelAutoFitStyle = LabelAutoFitStyles.None
				}
			});

			foreach (var missionResult in missions) {
				var series = new Series {
					MarkerSize = 15,
					MarkerStyle = MarkerStyle.Circle,
					ChartType = SeriesChartType.Point
				};
				foreach (var pair in missionResult.Value.ModData) {
					var modData = missionResult.Value.ModData[pair.Key];
					var fc = modData.GetValues<KilogramPerSecond>(ModalResultField.FCMap);
					var dt = modData.GetValues<Second>(ModalResultField.simulationInterval).ToList();

					var fcSum = fc.Zip(dt, (fcValue, dtValue) => fcValue * dtValue).Sum();


					var maxDistance = modData.GetValues<Meter>(ModalResultField.dist).Max();
					var maxTime = modData.GetValues<Second>(ModalResultField.time).Max();

					var avgKmh = maxDistance.ConvertTo().Kilo.Meter / maxTime.ConvertTo().Hour;
					var co2GPerKm = fcSum.ConvertTo().Gramm * Physics.CO2PerFuelWeight / maxDistance.ConvertTo().Kilo.Meter;

					var loading = missionResult.Value.Mission.Loadings[pair.Key].ConvertTo().Ton;

					var point = new DataPoint(avgKmh.Value(), co2GPerKm.Value()) {
						Label = string.Format(CultureInfo.InvariantCulture, "{0:0.0} t", loading.Value()),
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
				co2SpeedChart.Series.Add(series);
			}

			co2SpeedChart.Update();
			var chartCo2Speed = new Bitmap(co2SpeedChart.Width, co2SpeedChart.Height, PixelFormat.Format32bppArgb);
			co2SpeedChart.DrawToBitmap(chartCo2Speed, new Rectangle(0, 0, co2SpeedChart.Width, co2SpeedChart.Height));
			return chartCo2Speed;
		}

		/// <summary>
		/// Draws the cycle chart for a cycle page.
		/// </summary>
		/// <param name="results">The results.</param>
		/// <returns></returns>
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
				Position = { X = 0f, Y = 0f, Width = 90, Height = 100 },
			});

			var altitude = new Series {
				ChartType = SeriesChartType.Area,
				Color = Color.Lavender,
				Name = "Altitude",
				YAxisType = AxisType.Secondary
			};

			var m = results.ModData.First().Value;
			var distanceKm = m.GetValues<Meter>(ModalResultField.dist).Select(v => v.ConvertTo().Kilo.Meter).ToDouble();

			altitude.Points.DataBindXY(distanceKm, m.GetValues<Meter>(ModalResultField.altitude).ToDouble());
			missionCycleChart.Series.Add(altitude);

			var targetSpeed = new Series { ChartType = SeriesChartType.FastLine, BorderWidth = 3, Name = "Target speed" };
			targetSpeed.Points.DataBindXY(distanceKm,
				m.GetValues<MeterPerSecond>(ModalResultField.v_targ).Select(v => v.ConvertTo().Kilo.Meter.Per.Hour).ToDouble());
			missionCycleChart.Series.Add(targetSpeed);

			foreach (var result in results.ModData) {
				var name = result.Key.ToString();
				var values = result.Value;
				var series = new Series { ChartType = SeriesChartType.FastLine, Name = name };
				series.Points.DataBindXY(
					values.GetValues<Meter>(ModalResultField.dist).Select(v => v.ConvertTo().Kilo.Meter).ToDouble(),
					values.GetValues<MeterPerSecond>(ModalResultField.v_act).Select(v => v.ConvertTo().Kilo.Meter.Per.Hour).ToDouble());
				missionCycleChart.Series.Add(series);
			}
			missionCycleChart.Update();

			var cycleChart = new Bitmap(missionCycleChart.Width, missionCycleChart.Height, PixelFormat.Format32bppArgb);
			missionCycleChart.DrawToBitmap(cycleChart, new Rectangle(0, 0, missionCycleChart.Width, missionCycleChart.Height));
			return cycleChart;
		}

		/// <summary>
		/// Draws the operating points chart for a cycle page.
		/// </summary>
		/// <param name="modData">The mod data.</param>
		/// <param name="flc">The FLC.</param>
		/// <returns></returns>
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
				}
			});

			var n = flc.FullLoadEntries.Select(x => x.EngineSpeed.ConvertTo().Rounds.Per.Minute).ToDouble();
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
			dataPoints.Points.DataBindXY(
				modData.GetValues<SI>(ModalResultField.n).Select(x => x.ConvertTo().Rounds.Per.Minute).ToDouble(),
				modData.GetValues<SI>(ModalResultField.Tq_eng).ToDouble());
			operatingPointsChart.Series.Add(dataPoints);

			operatingPointsChart.Update();

			var tqnBitmap = new Bitmap(operatingPointsChart.Width, operatingPointsChart.Height, PixelFormat.Format32bppArgb);
			operatingPointsChart.DrawToBitmap(tqnBitmap, new Rectangle(0, 0, tqnBitmap.Width, tqnBitmap.Height));
			return tqnBitmap;
		}

		/// <summary>
		/// Gets the appropriate vehicle image.
		/// </summary>
		/// <param name="segment">The segment.</param>
		/// <param name="missionType">Type of the mission.</param>
		/// <returns></returns>
		private static Image GetVehicleImage(Segment segment, MissionType missionType)
		{
			var name = "Undef.png";
			switch (segment.VehicleClass) {
				case VehicleClass.Class1:
				case VehicleClass.Class2:
				case VehicleClass.Class3:
					name = "4x2r.png";
					break;
				case VehicleClass.Class4:
					name = missionType == MissionType.LongHaul ? "4x2rt.png" : "4x2r.png";
					break;
				case VehicleClass.Class5:
					name = "4x2tt.png";
					break;
				case VehicleClass.Class9:
					name = missionType == MissionType.LongHaul ? "6x2rt.png" : "6x2r.png";
					break;
				case VehicleClass.Class10:
					name = "6x2tt.png";
					break;
			}

			var hdvClassImagePath = RessourceHelper.Namespace + "Report." + name;
			var hdvClassImage = RessourceHelper.ReadStream(hdvClassImagePath);
			return Image.GetInstance(hdvClassImage);
		}
	}
}