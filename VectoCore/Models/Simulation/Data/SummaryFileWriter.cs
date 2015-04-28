using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public class SummaryFileWriter : ISummaryDataWriter
	{
		private readonly DataTable _table;
		private readonly string _sumFileName;

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


		public void Write(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName)
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
			row["a [m/s2]"] = data.Compute("Avg(acc)", ""); // todo dynamic time steps!


			//todo altitude - calculate when reading the cycle file, add column for altitude
			//row["∆altitude [m]"] = Data.Rows[Data.Rows.Count - 1].Field<double>("altitude") -
			//						Data.Rows[0].Field<double>("altitude");

			//todo auxiliaries
			//foreach (var auxCol in data.Auxiliaries) {
			//    row["Eaux_" + auxCol.jobName + " [kwh]"] = data.Compute("Sum(aux_" + auxCol.jobName + ")", "");
			//}

			//todo get data from vehicle file
			//row["Mass [kg]"] = Container.VehicleMass();
			//row["Loading [kg]"] = Container.LoadingMass();

			var acceleration = data.GetValues<double?>(ModalResultField.acc).ToList();
			var simInterval = data.GetValues<double>(ModalResultField.simulationInterval).ToList();

			//todo dynamic time steps!!!
			var runningAverage = (acceleration[0] + acceleration[1] + acceleration[2]) / 3;
			var accelerationAvg = new List<double?>();

			for (var i = 2; i < acceleration.Count() - 1; i++) {
				runningAverage -= acceleration[i - 2] / 3;
				runningAverage += acceleration[i + 1] / 3;
				accelerationAvg.Add(runningAverage);
			}


			var apos = accelerationAvg.Where(x => x > 0.125).Average();
			if (apos.HasValue) {
				row["a_pos [m/s2]"] = apos;
			}

			var aneg = accelerationAvg.Where(x => x < -0.125).Average();
			if (aneg.HasValue) {
				row["a_neg [m/s2]"] = aneg;
			}


			row["pAcc [%]"] = 100.0 * accelerationAvg.Count(x => x > 0.125) / accelerationAvg.Count;
			row["pDec [%]"] = 100.0 * accelerationAvg.Count(x => x < -0.125) / accelerationAvg.Count;
			row["pCruise [%]"] = 100.0 * accelerationAvg.Count(x => x < 0.125 && x > -0.125) / accelerationAvg.Count;


			var velocity = data.GetValues<double?>(ModalResultField.v_act).ToList();
			var timeSum = 0.0;
			for (var i = 0; i < velocity.Count; i++) {
				if (velocity[i] < 0.1) {
					timeSum += simInterval[i];
				}
			}


			row["pStop [%]"] = 100.0 * timeSum / (double)data.Compute("Max(time)", "");

			_table.ImportRow(row);
		}

		public void Finish()
		{
			VectoCSVFile.Write(_sumFileName, _table);
		}
	}
}