using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public class ModalDataWriter : IModalDataWriter
	{
		private string JobName { get; set; }
		private SummaryFileWriter SumWriter { get; set; }
		private string CycleFileName { get; set; }
		private string JobFileName { get; set; }
		private ModalResults Data { get; set; }
		private DataRow CurrentRow { get; set; }
		private string FileName { get; set; }


		public ModalDataWriter(string fileName, string jobName, string cycleFileName, SummaryFileWriter sumWriter)
		{
			FileName = fileName;
			Data = new ModalResults();
			CurrentRow = Data.NewRow();
			SumWriter = sumWriter;
			JobName = jobName;
			CycleFileName = cycleFileName;
		}

		public void CommitSimulationStep()
		{
			Data.Rows.Add(CurrentRow);
			CurrentRow = Data.NewRow();
		}

		public void Finish()
		{
			VectoCSVFile.Write(FileName, Data);
			SumWriter.Write(this, jobName, cycleFileName);
		}

		public object Compute(string expression, string filter)
		{
			return Data.Compute(expression, filter);
		}

		public IEnumerable<object> GetValues(ModalResultField key)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x[(int)key]);
		}


		public object this[ModalResultField key]
		{
			get { return CurrentRow[(int)key]; }
			set { CurrentRow[(int)key] = value; }
		}
	}


	/*
	jobName	Unit	Description
	Job	[-]	Job number. Format is "x-y" with x = file number and y = cycle number
	Input File	[-]	jobName of the input file
	Cycle	[-]	jobName of the cycle file
	time	[s]	Total simulation time
	distance	[km]	Total travelled distance
	speed	[km/h]	Average vehicle speed
	∆altitude	[m]	Altitude difference between start and end of cycle
	Ppos	[kW]	Average positive engine power
	Pneg	[kW]	Average negative engine power
	FC	[g/km]	Average fuel consumption
	FC-AUXc	[g/km]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
	FC-WHTCc	[g/km]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)
	Pbrake	[kW]	Average brake power (not including engine drag)
	EposICE	[kWh]	Total positive engine work
	EnegICE	[kWh]	Total negative engine work (engine brake)
	Eair	[kWh]	Total work of air resistance
	Eroll	[kWh]	Total work of rolling resistance
	Egrad	[kWh]	Total work of gradient resistance
	Eacc	[kWh]	Total work from accelerations (<0) / decelerations (>0) 
	Eaux	[kWh]	Total energy demand of auxiliaries
	Eaux_xxx	[kWh]	Energy demand of auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
	Ebrake	[kWh]	Total work dissipated in mechanical braking (sum of service brakes, retader and additional engine exhaust brakes)
	Etransm	[kWh]	Total work of transmission losses
	Eretarder	[kWh]	Total retarder losses
	Mass	[kg]	Vehicle mass (equals Curb Weight Vehicle plus Curb Weight Extra Trailer/Body, see Vehicle Editor)
	Loading	[kg]	Vehicle loading (see Vehicle Editor)
	a	[m/s2]	Average acceleration
	a_pos	[m/s2]	Average acceleration in acceleration phases*
	a_neg	[m/s2]	Average deceleration in deceleration phases*
	Acc.Noise	[m/s2]	Acceleration noise
	pAcc	[%]	Time share of acceleration phases*
	pDec	[%]	Time share of deceleration phases*
	pCruise	[%]	Time share of cruise phases*
	pStop	[%]	Time share of stop phases*
	*/

	public class SummaryFileWriter
	{
		private readonly DataTable _table;
		private readonly string _jobFileName;
		private readonly string _sumFileName;

		public SummaryFileWriter(string sumFileName, string jobFileName)
		{
			_sumFileName = sumFileName;
			_jobFileName = jobFileName;

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


		public void Write(IModalDataWriter data, string jobName, string cycleFileName)
		{
			var row = _table.NewRow();
			row["Job [-]"] = jobName;
			row["Input File [-]"] = _jobFileName;
			row["Cycle [-]"] = cycleFileName;
			row["time [s]"] = data.Compute("Max(time)", "");
			row["distance [km]"] = data.Compute("Max(dist)", "");
			row["speed [km/h]"] = data.Compute("Avg(v_act)", "");


			row["Ppos [kw]"] = data.Compute("Avg(Peng)", "Pe_eng > 0");
			row["Pneg [kw]"] = data.Compute("Avg(Peng)", "Pe_eng < 0");
			row["FC [g/km]"] = data.Compute("Avg(FC)", "");
			row["FC-AUXc [g/km]"] = data.Compute("Avg(FC-AUXc)", "");
			row["FC-WHTCc [g/km]"] = data.Compute("Avg(FC-WHTCc)", "");
			row["Pbrake [kw]"] = data.Compute("Avg(Pbrake)", "");
			row["EposICE [kwh]"] = data.Compute("Avg(pos)", "pos > 0");
			row["EnegICE [kwh]"] = data.Compute("Avg(pos)", "pos < 0");
			row["Eair [kwh]"] = data.Compute("Sum(Pair)", "");
			row["Eroll [kwh]"] = data.Compute("Sum(Proll)", "");
			row["Egrad [kwh]"] = data.Compute("Sum(Pgrad)", "");
			row["Eaux [kwh]"] = data.Compute("Sum(Paux)", "");
			row["Ebrake [kwh]"] = data.Compute("Sum(brake)", "");
			row["Etransm [kwh]"] = data.Compute("Sum(transm)", "");
			row["Eretarder [kwh]"] = data.Compute("Sum(retarder)", "");
			row["Eacc [kwh]"] = data.Compute("Sum(Pa+PaGB+PaEng)", "");
			row["a [m/s2]"] = data.Compute("Avg(a)", "");


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

			var acceleration = data.GetValues(ModalResultField.acc).Cast<double>().ToList();
			var simInterval = data.GetValues(ModalResultField.simulationInterval).Cast<double>().ToList();

			//todo dynamic time steps!!!
			var runningAverage = (acceleration[0] + acceleration[1] + acceleration[2]) / 3;
			var accelerationAvg = new List<double>();

			for (var i = 2; i < acceleration.Count() - 1; i++) {
				runningAverage -= acceleration[i - 2] / 3;
				runningAverage += acceleration[i + 1] / 3;
				accelerationAvg.Add(runningAverage);
			}

			row["a_pos [m/s2]"] = accelerationAvg.Where(x => x > 0.125).Average();
			row["a_neg [m/s2]"] = accelerationAvg.Where(x => x < -0.125).Average();


			row["pAcc [%]"] = 100.0 * accelerationAvg.Count(x => x > 0.125) / accelerationAvg.Count;
			row["pDec [%]"] = 100.0 * accelerationAvg.Count(x => x < -0.125) / accelerationAvg.Count;
			row["pCruise [%]"] = 100.0 * accelerationAvg.Count(x => x < 0.125 && x > -0.125) / accelerationAvg.Count;


			var velocity = data.GetValues(ModalResultField.v_act).Cast<double>().ToList();
			var timeSum = 0.0;
			for (var i = 0; i < velocity.Count; i++) {
				if (velocity[i] < 0.1) {
					timeSum += simInterval[i];
				}
			}
			row["pStop [%]"] = 100.0 * timeSum / (double)data.Compute("Max(v_act)", "");

			_table.ImportRow(row);
		}

		public void Finish()
		{
			VectoCSVFile.Write(_sumFileName, _table);
		}
	}
}