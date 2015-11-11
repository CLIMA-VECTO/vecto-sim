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
		private const string CO2KM = "CO2 [g/km]";
		private const string CO2TKM = "CO2 [g/tkm]";
		private const string FCFINAL = "FC-Final [g/km]";
		private const string FCFINAL_LITERPER100KM = "FC-Final [l/100km]";
		private const string FCFINAL_LITERPER100TKM = "FC-Final [l/100tkm]";
		private const string ACCNOISE = "Acc.Noise [m/s^2]";
		// ReSharper restore InconsistentNaming

		private readonly DataTable _table;
		private readonly string _sumFileName;
		private bool _engineOnly = true;

		protected SummaryFileWriter() {}

		private readonly IList<string> _auxColumns = new List<string>();

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
				ANEG, PACC, PDEC, PCRUISE, PSTOP, ETORQUECONV, CO2KM, CO2TKM, FCFINAL, FCFINAL_LITERPER100KM, FCFINAL_LITERPER100TKM,
				ACCNOISE
			}.Select(x => new DataColumn(x, typeof(SI))).ToArray());
		}

		public virtual void Write(bool isEngineOnly, IModalDataWriter data, string jobFileName, string jobName,
			string cycleFileName,
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
			row[PPOS] = data.EnginePowerPositiveAverage().ConvertTo().Kilo.Watt;
			row[PNEG] = data.EnginePowerNegativeAverage().ConvertTo().Kilo.Watt;
			row[FCMAP] = data.FuelConsumptionPerSecond().ConvertTo().Gramm.Per.Hour;
			row[FCAUXC] = data.FuelConsumptionAuxStartStopCorrectedPerSecond().ConvertTo().Gramm.Per.Hour;
			row[FCWHTCC] = data.FuelConsumptionWHTCCorrectedPerSecond().ConvertTo().Gramm.Per.Hour;
			WriteAuxiliaries(data, row);

			_table.Rows.Add(row);
		}


		protected internal void WriteFullPowertrain(IModalDataWriter data, string jobFileName, string jobName,
			string cycleFileName, Kilogram vehicleMass, Kilogram vehicleLoading)
		{
			_engineOnly = false;

			var row = _table.NewRow();
			_table.Rows.Add(row);
			row[JOB] = jobName;
			row[INPUTFILE] = jobFileName;
			row[CYCLE] = cycleFileName;
			row[STATUS] = data.RunStatus;
			row[TIME] = data.Duration();
			row[DISTANCE] = data.Distance().ConvertTo().Kilo.Meter;
			row[SPEED] = data.Speed().ConvertTo().Kilo.Meter.Per.Hour;
			row[ALTITUDE] = data.AltitudeDelta();
			row[PPOS] = data.EnginePowerPositiveAverage().ConvertTo().Kilo.Watt;
			row[PNEG] = data.EnginePowerNegativeAverage().ConvertTo().Kilo.Watt;
			row[FCFINAL] = data.FuelConsumptionFinal().ConvertTo().Gramm.Per.Kilo.Meter;
			row[FCFINAL_LITERPER100KM] = data.FuelConsumptionFinalLiterPer100Kilometer();
			if (!vehicleLoading.IsEqual(0)) {
				row[FCFINAL_LITERPER100TKM] = data.FuelConsumptionFinalLiterPer100Kilometer() / vehicleLoading.ConvertTo().Ton;
			}
			row[FCMAP] = data.FuelConsumptionPerSecond().ConvertTo().Gramm.Per.Hour;
			row[FCMAPKM] = data.FuelConsumptionPerMeter().ConvertTo().Gramm.Per.Kilo.Meter;
			row[FCAUXC] = data.FuelConsumptionAuxStartStopCorrectedPerSecond().ConvertTo().Gramm.Per.Hour;
			row[FCAUXCKM] = data.FuelConsumptionAuxStartStopCorrected().ConvertTo().Gramm.Per.Kilo.Meter;
			row[FCWHTCC] = data.FuelConsumptionWHTCCorrectedPerSecond().ConvertTo().Gramm.Per.Hour;
			row[FCWHTCCKM] = data.FuelConsumptionWHTCCorrected().ConvertTo().Gramm.Per.Kilo.Meter;
			row[CO2KM] = data.CO2PerMeter().ConvertTo().Gramm.Per.Kilo.Meter;
			if (!vehicleLoading.IsEqual(0)) {
				row[CO2TKM] = data.CO2PerMeter().ConvertTo().Gramm.Per.Kilo.Meter / vehicleLoading.ConvertTo().Ton;
			}
			row[PWHEELPOS] = data.PowerWheelPositive().ConvertTo().Kilo.Watt;
			row[PBRAKE] = data.PowerBrake().ConvertTo().Kilo.Watt;
			row[EPOSICE] = data.EngineWorkPositive().ConvertTo().Kilo.Watt.Hour;
			row[ENEGICE] = data.EngineWorkNegative().ConvertTo().Kilo.Watt.Hour;
			row[EAIR] = data.WorkAirResistance().ConvertTo().Kilo.Watt.Hour;
			row[EROLL] = data.WorkRollingResistance().ConvertTo().Kilo.Watt.Hour;
			row[EGRAD] = data.WorkRoadGradientResistance().ConvertTo().Kilo.Watt.Hour;
			row[EACC] = data.PowerAccelerations().ConvertTo().Kilo.Watt.Hour;
			row[EAUX] = data.WorkAuxiliaries().ConvertTo().Kilo.Watt.Hour;
			WriteAuxiliaries(data, row);
			row[EBRAKE] = data.WorkTotalMechanicalBrake().ConvertTo().Kilo.Watt.Hour;
			row[ETRANSM] = data.WorkTransmission().ConvertTo().Kilo.Watt.Hour;
			row[ERETARDER] = data.WorkRetarder().ConvertTo().Kilo.Watt.Hour;
			row[ETORQUECONV] = data.WorkTorqueConverter().ConvertTo().Kilo.Watt.Hour;
			row[MASS] = vehicleMass;
			row[LOADING] = vehicleLoading;
			row[ACCELERATIONS] = data.AccelerationAverage();
			row[APOS] = data.AccelerationsPositive3SecondAverage();
			row[ANEG] = data.AverageAccelerations3SecondNegative();
			row[ACCNOISE] = data.AccelerationNoise();
			row[PACC] = data.PercentAccelerationTime();
			row[PDEC] = data.PercentDecelerationTime();
			row[PCRUISE] = data.PercentCruiseTime();
			row[PSTOP] = data.PercentStopTime();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void WriteAuxiliaries(IModalDataWriter data, DataRow row)
		{
			foreach (var aux in data.Auxiliaries) {
				var colName = "Eaux_" + aux.Key + " [kWh]";
				if (!_table.Columns.Contains(colName)) {
					_table.Columns.Add(colName, typeof(SI));
					_auxColumns.Add(colName);
				}

				row[colName] = data.AuxiliaryWork(aux.Value).ConvertTo().Kilo.Watt.Hour;
			}
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
					PPOS, PNEG, FCMAP, FCMAPKM, FCAUXC, FCAUXCKM, FCWHTCC, FCWHTCCKM, CO2KM, CO2TKM, FCFINAL, FCFINAL_LITERPER100KM,
					FCFINAL_LITERPER100TKM, PWHEELPOS, PBRAKE, EPOSICE, ENEGICE, EAIR, EROLL, EGRAD, EACC, EAUX, EBRAKE, ETRANSM,
					ERETARDER, ETORQUECONV, MASS, LOADING, ACCELERATIONS, APOS, ANEG, ACCNOISE, PACC, PDEC, PCRUISE, PSTOP
				});
			}

			var sortedAndFilteredTable = new DataView(_table, "", JOB, DataViewRowState.CurrentRows).ToTable(false,
				dataColumns.ToArray());

			VectoCSVFile.Write(_sumFileName, sortedAndFilteredTable);
		}
	}
}