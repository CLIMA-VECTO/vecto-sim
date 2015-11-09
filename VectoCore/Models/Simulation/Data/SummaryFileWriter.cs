using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public delegate void WriteSumData(IModalDataWriter data, Kilogram vehicleMass, Kilogram loading);


	/// <summary>
	/// Class for the sum file in vecto.
	/// </summary>
	public class SummaryFileWriter : LoggingObject
	{
		// ReSharper disable InconsistentNaming
		private const string JOB = "Job [-]";
		private const string INPUTFILE = "Input File [-]";
		private const string CYCLE = "Cycle [-]";
		private const string STATUS = "Status";
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
		private const string ACCELERATIONS = "a [m/s^2]";
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
			_table.Columns.Add(STATUS, typeof(string));

			_table.Columns.AddRange(new[] {
				TIME, DISTANCE, SPEED, ALTITUDE, PPOS, PNEG, FCMAP, FCMAPKM, FCAUXC, FCAUXCKM, FCWHTCC, FCWHTCCKM, PWHEELPOS, PBRAKE,
				EPOSICE, ENEGICE, EAIR, EROLL, EGRAD, EACC, EAUX, EBRAKE, ETRANSM, ERETARDER, MASS, LOADING, ACCELERATIONS, APOS,
				ANEG, PACC, PDEC, PCRUISE, PSTOP, ETORQUECONV, CO2, CO2T, FCFINAL, FCFINAL_LITER, FCFINAL_LITERPER100TKM, ACCNOISE
			}.Select(x => new DataColumn(x, typeof(SI))).ToArray());
		}

		public virtual void Write(bool isEngineOnly, IModalDataWriter data, string jobFileName, string jobName, string cycleFileName,
			Kilogram vehicleMass, Kilogram vehicleLoading)
		{
			if (isEngineOnly) {
				WriteEngineOnly(data, jobFileName, jobName, cycleFileName);
			} else {
				WriteFullPowertrain(data, jobFileName, jobName, cycleFileName, vehicleMass, vehicleLoading);
			}
		}


		protected internal void WriteEngineOnly(IModalDataWriter data, string jobFileName, string jobName,
			string cycleFileName)
		{
			var row = _table.NewRow();
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[STATUS] = data.RunStatus;
			row[TIME] = data.Duration();
			row[PPOS] = data.Ppos();
			row[PNEG] = data.Pneg();
			row[FCMAP] = data.FuelConsumption();
			row[FCAUXC] = data.FuelConsumptionAuxStartStopCorrected();
			row[FCWHTCC] = data.FuelConsumptionWHTCCorrected();

			WriteAuxiliaries(data, row);

			_table.Rows.Add(row);
		}


		protected internal void WriteFullPowertrain(IModalDataWriter data, string jobFileName, string jobName,
			string cycleFileName, Kilogram vehicleMass, Kilogram vehicleLoading)
		{
			_engineOnly = false;

			var row = _table.NewRow();
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[STATUS] = data.RunStatus;
			row[TIME] = data.Duration();
			row[DISTANCE] = data.Distance();
			row[SPEED] = data.Speed();
			row[PPOS] = data.Ppos();
			row[PNEG] = data.Pneg();
			row[FCMAP] = data.FuelConsumption();
			row[FCMAPKM] = data.FuelConsumptionPerKilometer();
			row[FCAUXC] = data.FuelConsumptionAuxStartStopCorrected();
			row[FCWHTCC] = data.FuelConsumptionWHTCCorrected();
			row[PBRAKE] = data.PowerBrake();
			row[EPOSICE] = data.WorkEnginePositive();
			row[ENEGICE] = data.WorkEngineNegative();
			row[EAIR] = data.WorkAirResistance();
			row[EROLL] = data.WorkRollingResistance();
			row[EGRAD] = data.WorkRoadGradientResistance();
			row[EAUX] = data.WorkAuxiliaries();
			row[EBRAKE] = data.WorkTotalMechanicalBrake();
			row[ETRANSM] = data.WorkTransmission();
			row[ERETARDER] = data.PowerLossRetarder();
			row[EACC] = data.PowerAccelerations();
			row[ALTITUDE] = data.AltitudeDelta();

			WriteAuxiliaries(data, row);

			if (vehicleMass != null) {
				row[MASS] = vehicleMass;
			}

			if (vehicleLoading != null) {
				row[LOADING] = vehicleLoading;
			}

			row[ACCELERATIONS] = data.AverageAccelerations();
			row[APOS] = data.AverageAccelerationsPositive();
			row[ANEG] = data.AverageAccelerationsNegative();
			row[PACC] = data.PercentAccelerationTime();
			row[PDEC] = data.PercentDecelerationTime();
			row[PCRUISE] = data.PercentCruiseTime();
			row[PSTOP] = data.PercentStopTime();

			_table.Rows.Add(row);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void WriteAuxiliaries(IModalDataWriter data, DataRow row)
		{
			var simulationInterval = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();

			_auxColumns = _auxColumns.Union(data.Auxiliaries.Select(kv => "Eaux_" + kv.Key + " [kWh]")).ToList();

			var sum = 0.SI().Kilo.Watt.Hour.ConvertTo().Kilo.Watt.Hour;
			foreach (var aux in data.Auxiliaries) {
				var colName = "Eaux_" + aux.Key + " [kWh]";
				if (!_table.Columns.Contains(colName)) {
					_table.Columns.Add(colName, typeof(SI));
				}

				var currentSum = data.GetValues<Watt>(aux.Value)
					.Zip(simulationInterval, (P, dt) => P.ConvertTo().Kilo.Watt * dt.ConvertTo().Hour)
					.Sum();
				row[colName] = currentSum;
				sum += currentSum;
			}
			row[EAUX] = sum;
		}

		public virtual void Finish()
		{
			var dataColumns = new List<string>();

			if (_engineOnly) {
				dataColumns.AddRange(new[] { JOB, INPUTFILE, CYCLE, STATUS, TIME, PPOS, PNEG, FCMAP, FCAUXC, FCWHTCC });
			} else {
				dataColumns.AddRange(new[] { JOB, INPUTFILE, CYCLE, STATUS, TIME, DISTANCE, SPEED, ALTITUDE });

				dataColumns.AddRange(_auxColumns);

				dataColumns.AddRange(new[] {
					PPOS, PNEG, FCMAP, FCMAPKM, FCAUXC, FCAUXCKM, FCWHTCC, FCWHTCCKM, CO2, CO2T, FCFINAL, FCFINAL_LITERPER100TKM,
					FCFINAL_LITER, PWHEELPOS, PBRAKE, EPOSICE, ENEGICE, EAIR, EROLL, EGRAD, EACC, EAUX, EBRAKE, ETRANSM,
					ERETARDER, ETORQUECONV, MASS, LOADING, ACCELERATIONS, APOS, ANEG, ACCNOISE, PACC, PDEC, PCRUISE, PSTOP
				});
			}

			VectoCSVFile.Write(_sumFileName, new DataView(_table).ToTable(false, dataColumns.ToArray()));
		}
	}
}