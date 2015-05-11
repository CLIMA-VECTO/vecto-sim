﻿using System.Collections.Generic;
using TUGraz.VectoCore.Utils;
using System.Linq;
using System.Data;
using TUGraz.VectoCore.Models.Simulation.Data;

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
		private const string TIME = "Time [s]";
		private const string DISTANCE = "distance [km]";
		private const string SPEED = "speed [km/h]";
		private const string ALTITUDE = "∆altitude [m]";
		private const string PPOS = "Ppos [kw]";
		private const string PNEG = "Pneg [kw]";
		private const string FC = "FC [g/km]";
		private const string FCAUXC = "FC-AUXc [g/km]";
		private const string FCWHTCC = "FC-WHTCc [g/km]";
		private const string PBRAKE = "Pbrake [kw]";
		private const string EPOSICE = "EposICE [kwh]";
		private const string ENEGICE = "EnegICE [kwh]";
		private const string EAIR = "Eair [kwh]";
		private const string EROLL = "Eroll [kwh]";
		private const string EGRAD = "Egrad [kwh]";
		private const string EACC = "Eacc [kwh]";
		private const string EAUX = "Eaux [kwh]";
		private const string EBRAKE = "Ebrake [kwh]";
		private const string ETRANSM = "Etransm [kwh]";
		private const string ERETARDER = "Eretarder [kwh]";
		private const string MASS = "Mass [kg]";
		private const string LOADING = "Loading [kg]";
		private const string A = "a [m/s2]";
		private const string APOS = "a_pos [m/s2]";
		private const string ANEG = "a_neg [m/s2]";
		private const string PACC = "pAcc [%]";
		private const string PDEC = "pDec [%]";
		private const string PCRUISE = "pCruise [%]";
		private const string PSTOP = "pStop [%]";
		private const string ETORQUECONV = "Etorqueconv [kWh]";
		private const string CO2 = "CO2 [g/km]";
		private const string CO2T = "CO2 [g/tkm]";
		private const string FCFINAL = "FC-Final [g/km]";
		private const string FCFINAL_LITER = "FC-Final [l/km]";
		private const string FCFINAL_LITERPER100TKM = "FC-Final [l/tkm]";
		private const string ACCNOISE = "Acc.Noise [m/s^2]";
		// ReSharper restore InconsistentNaming

		private readonly DataTable _table;
		private readonly string _sumFileName;
		private bool _engineOnly = true;

		protected SummaryFileWriter() {}

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
				TIME, DISTANCE, SPEED, ALTITUDE, PPOS, PNEG, FC, FCAUXC, FCWHTCC, PBRAKE, EPOSICE, ENEGICE, EAIR, EROLL, EGRAD,
				EACC, EAUX, EBRAKE, ETRANSM, ERETARDER, MASS, LOADING, A, APOS, ANEG, PACC, PDEC, PCRUISE, PSTOP, ETORQUECONV, CO2,
				CO2T, FCFINAL, FCFINAL_LITER, FCFINAL_LITERPER100TKM, ACCNOISE
			}.Select(x => new DataColumn(x, typeof(double))).ToArray());
		}

		public void WriteEngineOnly(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName)
		{
			var row = _table.NewRow();
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[TIME] = data.Compute("Max(time)", "");
			row[PPOS] = data.Compute("Avg(Pe_eng)", "Pe_eng > 0");
			row[PNEG] = data.Compute("Avg(Pe_eng)", "Pe_eng < 0");
			row[FC] = data.Compute("Avg(FC)", "");
			row[FCAUXC] = data.Compute("Avg([FC-AUXc])", "");
			row[FCWHTCC] = data.Compute("Avg([FC-WHTCc])", "");

			//todo auxiliaries
			//foreach (var auxCol in data.Auxiliaries) {
			//    row["Eaux_" + auxCol.jobName + " [kwh]"] = data.Compute("Sum(aux_" + auxCol.jobName + ")", "");
			//}

			_table.Rows.Add(row);
		}


		public void WriteFullPowertrain(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName,
			double vehicleMass,
			double vehicleLoading)
		{
			_engineOnly = false;

			var row = _table.NewRow();
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[TIME] = data.Compute("Max(time)", "");
			row[DISTANCE] = data.Compute("Max(dist)", "");
			row[SPEED] = data.Compute("Avg(v_act)", "");
			row[PPOS] = data.Compute("Avg(Pe_eng)", "Pe_eng > 0");
			row[PNEG] = data.Compute("Avg(Pe_eng)", "Pe_eng < 0");
			row[FC] = data.Compute("Avg(FC)", "");
			row[FCAUXC] = data.Compute("Avg([FC-AUXc])", "");
			row[FCWHTCC] = data.Compute("Avg([FC-WHTCc])", "");
			row[PBRAKE] = data.Compute("Avg(Pbrake)", "");
			row[EPOSICE] = data.Compute("Avg(Pe_eng)", "Pe_eng > 0");
			row[ENEGICE] = data.Compute("Avg(Pe_eng)", "Pe_eng < 0");
			row[EAIR] = data.Compute("Sum(Pair)", "");
			row[EROLL] = data.Compute("Sum(Proll)", "");
			row[EGRAD] = data.Compute("Sum(Pgrad)", "");
			row[EAUX] = data.Compute("Sum(Paux)", "");
			row[EBRAKE] = data.Compute("Sum(Pbrake)", "");
			row[ETRANSM] = data.Compute("Sum([Ploss Diff]) + Sum([Ploss GB])", "");
			row[ERETARDER] = data.Compute("Sum([Ploss Retarder])", "");
			row[EACC] = data.Compute("Sum(Pa)+Sum([Pa GB])", ""); // TODO +PaEng?

			//todo altitude - calculate when reading the cycle file, add column for altitude
			//row["∆altitude [m]"] = Data.Rows[Data.Rows.Count - 1].Field<double>("altitude") -
			//						Data.Rows[0].Field<double>("altitude");

			//todo auxiliaries
			//foreach (var auxCol in data.Auxiliaries) {
			//    row["Eaux_" + auxCol.jobName + " [kwh]"] = data.Compute("Sum(aux_" + auxCol.jobName + ")", "");
			//}

			//todo get data from vehicle file
			row[MASS] = vehicleMass;
			row[LOADING] = vehicleLoading;

			var dtValues = data.GetValues<double>(ModalResultField.simulationInterval).ToList();
			var accValues = data.GetValues<double?>(ModalResultField.acc);
			var accelerations = CalculateAverageOverSeconds(dtValues, accValues).ToList();
			row[A] = accelerations.Average();

			var acceleration3SecondAverage = Calculate3SecondAverage(accelerations).ToList();

			row[APOS] = acceleration3SecondAverage.Where(x => x > 0.125).DefaultIfEmpty(0).Average();
			row[ANEG] = acceleration3SecondAverage.Where(x => x < -0.125).DefaultIfEmpty(0).Average();
			row[PACC] = 100.0 * acceleration3SecondAverage.Count(x => x > 0.125) / acceleration3SecondAverage.Count;
			row[PDEC] = 100.0 * acceleration3SecondAverage.Count(x => x < -0.125) / acceleration3SecondAverage.Count;
			row[PCRUISE] = 100.0 * acceleration3SecondAverage.Count(x => x < 0.125 && x > -0.125) /
							acceleration3SecondAverage.Count;

			var pStopTime = data.GetValues<double?>(ModalResultField.v_act)
				.Zip(dtValues, (velocity, dt) => new { velocity, dt })
				.Where(x => x.velocity < 0.1)
				.Sum(x => x.dt);
			row[PSTOP] = 100.0 * pStopTime / dtValues.Sum();

			_table.Rows.Add(row);
		}

		private static IEnumerable<double> Calculate3SecondAverage(List<double> accelerations)
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


		private static IEnumerable<double> CalculateAverageOverSeconds(IEnumerable<double> dtValues,
			IEnumerable<double?> accValues)
		{
			var dtSum = 0.0;
			var accSum = 0.0;
			var acceleration = dtValues.Zip(accValues, (dt, acc) => new { dt, acc }).ToList();
			foreach (var x in acceleration.ToList()) {
				var currentX = x;

				while (dtSum + currentX.dt >= 1) {
					var splitX = new { dt = 1 - dtSum, currentX.acc };
					yield return accSum;
					accSum = 0.0;
					dtSum = 0.0;

					currentX = new { dt = currentX.dt - splitX.dt, currentX.acc };
				}
				if (currentX.dt > 0) {
					accSum += currentX.dt * currentX.acc ?? 0.0;
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
			//todo aux
			string[] dataColumns;
			if (_engineOnly) {
				dataColumns = new[] { JOB, INPUTFILE, CYCLE, TIME, PPOS, PNEG, FC, FCAUXC, FCWHTCC };
			} else {
				dataColumns = new[] {
					JOB, INPUTFILE, CYCLE, TIME, PPOS, PNEG, DISTANCE, SPEED, ALTITUDE, PBRAKE, EPOSICE, ENEGICE, EAIR, EROLL, EGRAD,
					EACC, EAUX, EBRAKE, ETRANSM, ERETARDER, ETORQUECONV, MASS, LOADING, FC, FCAUXC, FCWHTCC, CO2, CO2T, FCFINAL,
					FCFINAL_LITER, FCFINAL_LITERPER100TKM, A, APOS, ANEG, ACCNOISE, PACC, PDEC, PCRUISE, PSTOP
				};
			}

			VectoCSVFile.Write(_sumFileName, new DataView(_table).ToTable(false, dataColumns));
		}
	}
}

/// <summary>
/// Decorator for FullPowertrain which adds some data for later use in the SummaryFileWriter.
/// </summary>
public class SumWriterDecoratorFullPowertrain : SummaryFileWriter, ISummaryDataWriter
{
	protected SummaryFileWriter _writer;
	protected string _jobFileName;
	protected string _jobName;
	protected string _cycleFileName;

	public SumWriterDecoratorFullPowertrain(SummaryFileWriter writer, string jobFileName, string jobName,
		string cycleFileName)
	{
		_writer = writer;
		_jobFileName = jobFileName;
		_jobName = jobName;
		_cycleFileName = cycleFileName;
	}

	public void Write(IModalDataWriter data, double vehicleMass = 0, double vehicleLoading = 0)
	{
		_writer.WriteFullPowertrain(data, _jobFileName, _jobName, _cycleFileName, vehicleMass, vehicleLoading);
	}
}


/// <summary>
/// Decorator for EngineOnly Mode which adds some data for later use in the SummaryFileWriter.
/// </summary>
public class SumWriterDecoratorEngineOnly : SummaryFileWriter, ISummaryDataWriter
{
	protected SummaryFileWriter _writer;
	protected string _jobFileName;
	protected string _jobName;
	protected string _cycleFileName;

	public SumWriterDecoratorEngineOnly(SummaryFileWriter writer, string jobFileName, string jobName, string cycleFileName)
	{
		_writer = writer;
		_jobFileName = jobFileName;
		_jobName = jobName;
		_cycleFileName = cycleFileName;
	}

	public void Write(IModalDataWriter data, double vehicleMass = 0, double vehicleLoading = 0)
	{
		_writer.WriteEngineOnly(data, _jobFileName, _jobName, _cycleFileName);
	}
}