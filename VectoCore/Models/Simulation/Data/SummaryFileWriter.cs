using System.Collections.Generic;
using TUGraz.VectoCore.Utils;
using System.Linq;
using System.Data;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public class SummaryFileWriter : ISummaryDataWriter
	{
		private readonly DataTable _table;
		private readonly string _sumFileName;

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryFileWriter"/> class.
		/// </summary>
		/// <param name="sumFileName">Name of the sum file.</param>
		public SummaryFileWriter(string sumFileName)
		{
			_sumFileName = sumFileName;

			_table = new DataTable();
			_table.Columns.Add("Job [-]", typeof(string));
			_table.Columns.Add("Input File [-]", typeof(string));
			_table.Columns.Add("Cycle [-]", typeof(string));
			_table.Columns.Add("Time [s]", typeof(double));
			_table.Columns.Add("distance [km]", typeof(double));
			_table.Columns.Add("speed [km/h]", typeof(double));
			_table.Columns.Add("∆altitude [m]", typeof(double));
			_table.Columns.Add("Ppos [kw]", typeof(double));
			_table.Columns.Add("Pneg [kw]", typeof(double));
			_table.Columns.Add("FC [g/km]", typeof(double));
			_table.Columns.Add("FC-AUXc [g/km]", typeof(double));
			_table.Columns.Add("FC-WHTCc [g/km]", typeof(double));
			_table.Columns.Add("Pbrake [kw]", typeof(double));
			_table.Columns.Add("EposICE [kwh]", typeof(double));
			_table.Columns.Add("EnegICE [kwh]", typeof(double));
			_table.Columns.Add("Eair [kwh]", typeof(double));
			_table.Columns.Add("Eroll [kwh]", typeof(double));
			_table.Columns.Add("Egrad [kwh]", typeof(double));
			_table.Columns.Add("Eacc [kwh]", typeof(double));
			_table.Columns.Add("Eaux [kwh]", typeof(double));
			_table.Columns.Add("Eaux_xxx [kwh]", typeof(double));
			_table.Columns.Add("Ebrake [kwh]", typeof(double));
			_table.Columns.Add("Etransm [kwh]", typeof(double));
			_table.Columns.Add("Eretarder [kwh]", typeof(double));
			_table.Columns.Add("Mass [kg]", typeof(double));
			_table.Columns.Add("Loading [kg]", typeof(double));
			_table.Columns.Add("a [m/s2]", typeof(double));
			_table.Columns.Add("a_pos [m/s2]", typeof(double));
			_table.Columns.Add("a_neg [m/s2]", typeof(double));
			_table.Columns.Add("pAcc [%]", typeof(double));
			_table.Columns.Add("pDec [%]", typeof(double));
			_table.Columns.Add("pCruise [%]", typeof(double));
			_table.Columns.Add("pStop [%]", typeof(double));
		}

		public void Write(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName, double vehicleMass,
			double vehicleLoading)
		{
			var row = _table.NewRow();
			row["Job [-]"] = jobName;
			row["Input File [-]"] = jobFileName;
			row["Cycle [-]"] = cycleFileName;
			row["time [s]"] = data.Compute("Max(time)", "");
			row["distance [km]"] = data.Compute("Max(dist)", "");
			row["speed [km/h]"] = data.Compute("Avg(v_act)", "");
			row["Ppos [kw]"] = data.Compute("Avg(Pe_eng)", "Pe_eng > 0");
			row["Pneg [kw]"] = data.Compute("Avg(Pe_eng)", "Pe_eng < 0");
			row["FC [g/km]"] = data.Compute("Avg(FC)", "");
			row["FC-AUXc [g/km]"] = data.Compute("Avg([FC-AUXc])", "");
			row["FC-WHTCc [g/km]"] = data.Compute("Avg([FC-WHTCc])", "");
			row["Pbrake [kw]"] = data.Compute("Avg(Pbrake)", "");
			row["EposICE [kwh]"] = data.Compute("Avg(Pe_eng)", "Pe_eng > 0");
			row["EnegICE [kwh]"] = data.Compute("Avg(Pe_eng)", "Pe_eng < 0");
			row["Eair [kwh]"] = data.Compute("Sum(Pair)", "");
			row["Eroll [kwh]"] = data.Compute("Sum(Proll)", "");
			row["Egrad [kwh]"] = data.Compute("Sum(Pgrad)", "");
			row["Eaux [kwh]"] = data.Compute("Sum(Paux)", "");
			row["Ebrake [kwh]"] = data.Compute("Sum(Pbrake)", "");
			row["Etransm [kwh]"] = data.Compute("Sum([Ploss Diff]) + Sum([Ploss GB])", "");
			row["Eretarder [kwh]"] = data.Compute("Sum([Ploss Retarder])", "");
			row["Eacc [kwh]"] = data.Compute("Sum(Pa)+Sum([Pa GB])", ""); // TODO +PaEng?

			//todo altitude - calculate when reading the cycle file, add column for altitude
			//row["∆altitude [m]"] = Data.Rows[Data.Rows.Count - 1].Field<double>("altitude") -
			//						Data.Rows[0].Field<double>("altitude");

			//todo auxiliaries
			//foreach (var auxCol in data.Auxiliaries) {
			//    row["Eaux_" + auxCol.jobName + " [kwh]"] = data.Compute("Sum(aux_" + auxCol.jobName + ")", "");
			//}

			//todo get data from vehicle file
			row["Mass [kg]"] = vehicleMass;
			row["Loading [kg]"] = vehicleLoading;

			var dtValues = data.GetValues<double>(ModalResultField.simulationInterval).ToList();
			var accValues = data.GetValues<double?>(ModalResultField.acc);
			var accelerations = CalculateAverageOverSeconds(dtValues, accValues).ToList();
			row["a [m/s2]"] = accelerations.Average();

			var acceleration3SecondAverage = Calculate3SecondAverage(accelerations).ToList();

			row["a_pos [m/s2]"] = acceleration3SecondAverage.Where(x => x > 0.125).DefaultIfEmpty(0).Average();
			row["a_neg [m/s2]"] = acceleration3SecondAverage.Where(x => x < -0.125).DefaultIfEmpty(0).Average();
			row["pAcc [%]"] = 100.0 * acceleration3SecondAverage.Count(x => x > 0.125) / acceleration3SecondAverage.Count;
			row["pDec [%]"] = 100.0 * acceleration3SecondAverage.Count(x => x < -0.125) / acceleration3SecondAverage.Count;
			row["pCruise [%]"] = 100.0 * acceleration3SecondAverage.Count(x => x < 0.125 && x > -0.125) /
								acceleration3SecondAverage.Count;

			var pStopTime = data.GetValues<double?>(ModalResultField.v_act)
				.Zip(dtValues, (velocity, dt) => new { velocity, dt })
				.Where(x => x.velocity < 0.1)
				.Sum(x => x.dt);
			row["pStop [%]"] = 100.0 * pStopTime / dtValues.Sum();

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

		public void Finish()
		{
			VectoCSVFile.Write(_sumFileName, _table);
		}
	}
}