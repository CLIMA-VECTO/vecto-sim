using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	/// <summary>
	/// Class for the sum file in vecto.
	/// </summary>
	public class SummaryFileWriter
	{
		// ReSharper disable InconsistentNaming
		private const string JOB = "Job [-]";
		private const string INPUTFILE = "Input File [-]";
		private const string CYCLE = "Cycle [-]";
		private const string TIME = "time [s]";
		private const string DISTANCE = "distance [km]";
		private const string SPEED = "speed [km/h]";
		private const string ALTITUDE = "∆altitude [m]";
		private const string PPOS = "Ppos [kW]";
		private const string PNEG = "Pneg [kW]";
		private const string FCMAP = "FC-Map [g/h]";
		private const string FCMAPKM = "FC-Map [g/km]";
		private const string FCAUXC = "FC-AUXc [g/h]";
		private const string FCAUXCKM = "FC-AUXc [g/km]";
		private const string FCWHTCC = "FC-WHTCc [g/h]";
		private const string FCWHTCCKM = "FC-WHTCc [g/km]";
		private const string PWHEELPOS = "PwheelPos [kW]";
		private const string PBRAKE = "Pbrake [kW]";
		private const string EPOSICE = "EposICE [kWh]";
		private const string ENEGICE = "EnegICE [kWh]";
		private const string EAIR = "Eair [kWh]";
		private const string EROLL = "Eroll [kWh]";
		private const string EGRAD = "Egrad [kWh]";
		private const string EACC = "Eacc [kWh]";
		private const string EAUX = "Eaux [kWh]";
		private const string EBRAKE = "Ebrake [kWh]";
		private const string ETRANSM = "Etransm [kWh]";
		private const string ERETARDER = "Eretarder [kWh]";
		private const string MASS = "Mass [kg]";
		private const string LOADING = "Loading [kg]";
		private const string A = "a [m/s^2]";
		private const string APOS = "a_pos [m/s^2]";
		private const string ANEG = "a_neg [m/s^2]";
		private const string PACC = "pAcc [%]";
		private const string PDEC = "pDec [%]";
		private const string PCRUISE = "pCruise [%]";
		private const string PSTOP = "pStop [%]";
		private const string ETORQUECONV = "Etorqueconv [kWh]";
		private const string CO2 = "CO2 [g/km]";
		private const string CO2T = "CO2 [g/tkm]";
		private const string FCFINAL = "FC-Final [g/km]";
		private const string FCFINAL_LITER = "FC-Final [l/100km]";
		private const string FCFINAL_LITERPER100TKM = "FC-Final [l/100tkm]";
		private const string ACCNOISE = "Acc.Noise [m/s^2]";
		// ReSharper restore InconsistentNaming

		private readonly DataTable _table;
		private readonly string _sumFileName;
		private bool _engineOnly = true;

		protected SummaryFileWriter() {}

		private IList<string> _auxColumns = new List<string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryFileWriter"/> class.
		/// </summary>
		/// <param name="sumFileName">Name of the sum file.</param>
		public SummaryFileWriter(string sumFileName)
		{
			_sumFileName = sumFileName;

			_table = new DataTable();
			_table.Columns.Add(JOB, typeof(string));
			_table.Columns.Add(INPUTFILE, typeof(string));
			_table.Columns.Add(CYCLE, typeof(string));

			_table.Columns.AddRange(new[] {
				TIME, DISTANCE, SPEED, ALTITUDE, PPOS, PNEG, FCMAP, FCMAPKM, FCAUXC, FCAUXCKM, FCWHTCC, FCWHTCCKM, PWHEELPOS, PBRAKE,
				EPOSICE, ENEGICE, EAIR, EROLL, EGRAD, EACC, EAUX, EBRAKE, ETRANSM, ERETARDER, MASS, LOADING, A, APOS, ANEG, PACC,
				PDEC, PCRUISE, PSTOP, ETORQUECONV, CO2, CO2T, FCFINAL, FCFINAL_LITER, FCFINAL_LITERPER100TKM, ACCNOISE
			}.Select(x => new DataColumn(x, typeof(SI))).ToArray());
		}

		protected internal void WriteEngineOnly(IModalDataWriter data, string jobFileName, string jobName,
			string cycleFileName)
		{
			var row = _table.NewRow();
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[TIME] = data.GetValues<SI>(ModalResultField.time).Max();
			row[PPOS] = data.GetValues<SI>(ModalResultField.Pe_eng).Where(x => x > 0).Average();
			row[PNEG] = data.GetValues<SI>(ModalResultField.Pe_eng).Where(x => x < 0).Average();
			row[FCMAP] = data.GetValues<SI>(ModalResultField.FCMap).Average();
			row[FCAUXC] = data.GetValues<SI>(ModalResultField.FCAUXc).Average();
			row[FCWHTCC] = data.GetValues<SI>(ModalResultField.FCWHTCc).Average();

			WriteAuxiliaries(data, row);

			_table.Rows.Add(row);
		}

		private void WriteAuxiliaries(IModalDataWriter data, DataRow row)
		{
			_auxColumns = _auxColumns.Union(data.Auxiliaries.Select(kv => "Eaux_" + kv.Key + " [kWh]")).ToList();

			var sum = 0.SI<Watt>();
			foreach (var aux in data.Auxiliaries) {
				var colName = "Eaux_" + aux.Key + " [kWh]";
				try {
					_table.Columns.Add(colName, typeof(SI));
				} catch (DuplicateNameException) {}


				var currentSum = data.Sum(aux.Value);
				row[colName] = currentSum;
				sum += currentSum;
			}
			row[EAUX] = sum;
		}


		protected internal void WriteFullPowertrain(IModalDataWriter data, string jobFileName, string jobName,
			string cycleFileName,
			Kilogram vehicleMass, Kilogram vehicleLoading)
		{
			_engineOnly = false;

			var row = _table.NewRow();
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[TIME] = data.Max(ModalResultField.time).DefaultIfNull();
			row[DISTANCE] = data.Max(ModalResultField.dist).DefaultIfNull();
			row[SPEED] = data.Average(ModalResultField.v_act).DefaultIfNull();
			row[PPOS] = data.Average(ModalResultField.Pe_eng, x => x > 0).DefaultIfNull();
			row[PNEG] = data.Average(ModalResultField.Pe_eng, x => x < 0).DefaultIfNull();
			row[FCMAP] = data.Average(ModalResultField.FCMap).DefaultIfNull();
			row[FCAUXC] = data.Average(ModalResultField.FCAUXc).DefaultIfNull();
			row[FCWHTCC] = data.Average(ModalResultField.FCWHTCc).DefaultIfNull();
			row[PBRAKE] = data.Average(ModalResultField.Pbrake).DefaultIfNull();
			row[EPOSICE] = data.Average(ModalResultField.Pe_eng, x => x > 0).DefaultIfNull();
			row[ENEGICE] = data.Average(ModalResultField.Pe_eng, x => x < 0).DefaultIfNull();
			row[EAIR] = data.Sum(ModalResultField.Pair).DefaultIfNull();
			row[EROLL] = data.Sum(ModalResultField.Proll).DefaultIfNull();
			row[EGRAD] = data.Sum(ModalResultField.Pgrad).DefaultIfNull();
			row[EAUX] = data.Sum(ModalResultField.Paux).DefaultIfNull();
			row[EBRAKE] = data.Sum(ModalResultField.Pbrake).DefaultIfNull();

			var plossdiff = data.Sum(ModalResultField.PlossDiff);
			var plossgb = data.Sum(ModalResultField.PlossGB);
			if ((plossdiff ?? plossgb) != null) {
				row[ETRANSM] = plossdiff ?? 0.SI() + plossgb ?? 0.SI();
			}
			row[ERETARDER] = data.Sum(ModalResultField.PlossRetarder).DefaultIfNull();

			var paeng = data.Sum(ModalResultField.PaEng);
			var pagb = data.Sum(ModalResultField.PaGB);
			if ((paeng ?? pagb) != null) {
				row[EACC] = paeng ?? 0.SI() + pagb ?? 0.SI();
			}

			//todo altitude - calculate when reading the cycle file, add column for altitude
			//row["∆altitude [m]"] = Data.Rows[Data.Rows.Count - 1].Field<double>("altitude") -
			//						Data.Rows[0].Field<double>("altitude");

			WriteAuxiliaries(data, row);

			//todo get data from vehicle file
			if (vehicleMass != null) {
				row[MASS] = vehicleMass;
			}

			if (vehicleLoading != null) {
				row[LOADING] = vehicleLoading;
			}

			var dtValues = data.GetValues<SI>(ModalResultField.simulationInterval).Cast<Second>().ToList();
			var accValues = data.GetValues<SI>(ModalResultField.acc).Cast<MeterPerSquareSecond>();
			var accelerations = CalculateAverageOverSeconds(dtValues, accValues).ToList();
			if (accelerations.Any()) {
				row[A] = accelerations.Average();
			}

			var acceleration3SecondAverage = Calculate3SecondAverage(accelerations).ToList();
			if (acceleration3SecondAverage.Any()) {
				row[APOS] = acceleration3SecondAverage.Average(x => x > 0.125).DefaultIfNull();
				row[ANEG] = acceleration3SecondAverage.Average(x => x < -0.125).DefaultIfNull();
				row[PACC] = 100.SI() * acceleration3SecondAverage.Count(x => x > 0.125) / acceleration3SecondAverage.Count;
				row[PDEC] = 100.SI() * acceleration3SecondAverage.Count(x => x < -0.125) / acceleration3SecondAverage.Count;
				row[PCRUISE] = 100.SI() * acceleration3SecondAverage.Count(x => x < 0.125 && x > -0.125) /
								acceleration3SecondAverage.Count;
			}
			var pStopTime = data.GetValues<SI>(ModalResultField.v_act)
				.Zip(dtValues, (velocity, dt) => new { velocity, dt })
				.Where(x => x.velocity < 0.1)
				.Sum(x => x.dt.Value());
			row[PSTOP] = 100 * pStopTime / dtValues.Sum();

			_table.Rows.Add(row);
		}

		private static IEnumerable<SI> Calculate3SecondAverage(IReadOnlyList<SI> accelerations)
		{
			if (accelerations.Count >= 3) {
				var runningAverage = (accelerations[0] + accelerations[1] + accelerations[2]) / 3.0;
				for (var i = 2; i < accelerations.Count() - 1; i++) {
					runningAverage -= accelerations[i - 2] / 3.0;
					runningAverage += accelerations[i + 1] / 3.0;
					yield return runningAverage;
				}
			}
		}


		private static IEnumerable<SI> CalculateAverageOverSeconds(IEnumerable<Second> dtValues,
			IEnumerable<MeterPerSquareSecond> accValues)
		{
			var dtSum = 0.SI().Second;
			var accSum = 0.SI().Meter.Per.Second;
			var acceleration = dtValues.Zip(accValues, (dt, acc) => new { dt, acc }).ToList();
			foreach (var x in acceleration.ToList()) {
				var currentX = x;

				while (dtSum + currentX.dt >= 1) {
					var splitX = new { dt = 1.SI<Second>() - dtSum, currentX.acc };
					yield return accSum;
					dtSum = 0.SI<Second>();
					accSum = 0.SI<MeterPerSecond>();

					currentX = new { dt = currentX.dt - splitX.dt, currentX.acc };
				}
				if (currentX.dt > 0) {
					accSum += currentX.dt * currentX.acc ?? 0.SI();
					dtSum += currentX.dt;
				}
			}

			// return remaining data. acts like extrapolation to next whole second.
			if (dtSum > 0) {
				yield return accSum;
			}
		}

		public virtual void Finish()
		{
			var dataColumns = new List<string>();

			if (_engineOnly) {
				dataColumns.AddRange(new[] { JOB, INPUTFILE, CYCLE, TIME, PPOS, PNEG, FCMAP, FCAUXC, FCWHTCC });
			} else {
				dataColumns.AddRange(new[] { JOB, INPUTFILE, CYCLE, TIME, DISTANCE, SPEED, ALTITUDE });

				dataColumns.AddRange(_auxColumns);

				dataColumns.AddRange(new[] {
					PPOS, PNEG, FCMAP, FCMAPKM, FCAUXC, FCAUXCKM, FCWHTCC, FCWHTCCKM, CO2, CO2T, FCFINAL, FCFINAL_LITERPER100TKM,
					FCFINAL_LITER, PWHEELPOS, PBRAKE, EPOSICE, ENEGICE, EAIR, EROLL, EGRAD, EACC, EAUX, EBRAKE, ETRANSM,
					ERETARDER, ETORQUECONV, MASS, LOADING, A, APOS, ANEG, ACCNOISE, PACC, PDEC, PCRUISE, PSTOP
				});
			}

			VectoCSVFile.Write(_sumFileName, new DataView(_table).ToTable(false, dataColumns.ToArray()));
		}
	}
}

/// <summary>
/// Decorator for FullPowertrain which adds some data for later use in the SummaryFileWriter.
/// </summary>
public class SumWriterDecoratorFullPowertrain : SummaryFileWriter, ISummaryDataWriter
{
	private readonly SummaryFileWriter _writer;
	private readonly string _jobFileName;
	private readonly string _jobName;
	private readonly string _cycleFileName;

	public SumWriterDecoratorFullPowertrain(SummaryFileWriter writer, string jobFileName, string jobName,
		string cycleFileName)
	{
		_writer = writer;
		_jobFileName = jobFileName;
		_jobName = jobName;
		_cycleFileName = cycleFileName;
	}

	public void Write(IModalDataWriter data, Kilogram vehicleMass = null, Kilogram vehicleLoading = null)
	{
		_writer.WriteFullPowertrain(data, _jobFileName, _jobName, _cycleFileName, vehicleMass, vehicleLoading);
	}
}


/// <summary>
/// Decorator for EngineOnly Mode which adds some data for later use in the SummaryFileWriter.
/// </summary>
public class SumWriterDecoratorEngineOnly : SummaryFileWriter, ISummaryDataWriter
{
	private readonly SummaryFileWriter _writer;
	private readonly string _jobFileName;
	private readonly string _jobName;
	private readonly string _cycleFileName;

	public SumWriterDecoratorEngineOnly(SummaryFileWriter writer, string jobFileName, string jobName, string cycleFileName)
	{
		_writer = writer;
		_jobFileName = jobFileName;
		_jobName = jobName;
		_cycleFileName = cycleFileName;
	}

	public void Write(IModalDataWriter data, Kilogram vehicleMass = null, Kilogram vehicleLoading = null)
	{
		_writer.WriteEngineOnly(data, _jobFileName, _jobName, _cycleFileName);
	}
}