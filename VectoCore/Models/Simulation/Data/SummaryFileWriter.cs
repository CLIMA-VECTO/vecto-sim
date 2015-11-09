using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
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
				ANEG, PACC,
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
			row[STATUS] = data.RunStatus;
			row[TIME] = Duration(data);
			row[PPOS] = Ppos(data);
			row[PNEG] = Pneg(data);
			row[FCMAP] = FuelConsumption(data);
			row[FCAUXC] = FuelConsumptionAuxStartStopCorrected(data);
			row[FCWHTCC] = FuelConsumptionWHTCCorrected(data);

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
			row[TIME] = Duration(data);
			row[DISTANCE] = Distance(data);
			row[SPEED] = Speed(data);
			row[PPOS] = Ppos(data);
			row[PNEG] = Pneg(data);
			row[FCMAP] = FuelConsumption(data);
			row[FCMAPKM] = FuelConsumptionPerKilometer(data);
			row[FCAUXC] = FuelConsumptionAuxStartStopCorrected(data);
			row[FCWHTCC] = FuelConsumptionWHTCCorrected(data);
			row[PBRAKE] = PowerBrake(data);
			row[EPOSICE] = WorkEnginePositive(data);
			row[ENEGICE] = WorkEngineNegative(data);
			row[EAIR] = WorkAirResistance(data);
			row[EROLL] = WorkRollingResistance(data);
			row[EGRAD] = WorkRoadGradientResistance(data);
			row[EAUX] = WorkAuxiliaries(data);
			row[EBRAKE] = WorkTotalMechanicalBrake(data);
			row[ETRANSM] = WorkTransmission(data);
			row[ERETARDER] = PowerLossRetarder(data);
			row[EACC] = PowerAccelerations(data);
			row[ALTITUDE] = AltitudeDelta(data);

			WriteAuxiliaries(data, row);

			if (vehicleMass != null) {
				row[MASS] = vehicleMass;
			}

			if (vehicleLoading != null) {
				row[LOADING] = vehicleLoading;
			}

			row[ACCELERATIONS] = AverageAccelerations(data);
			row[APOS] = AverageAccelerationsPositive(data);
			row[ANEG] = AverageAccelerationsNegative(data);
			row[PACC] = PercentAccelerationTime(data);
			row[PDEC] = PercentDecelerationTime(data);
			row[PCRUISE] = PercentCruiseTime(data);
			row[PSTOP] = PercentStopTime(data);

			_table.Rows.Add(row);
		}

		private static object AverageAccelerationsPositive(IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data));
			return acceleration3SecondAverage.Average(x => x > 0.125).DefaultIfNull();
		}

		private static object AverageAccelerationsNegative(IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data));
			return acceleration3SecondAverage.Average(x => x < -0.125).DefaultIfNull();
		}

		private static object PercentAccelerationTime(IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data)).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x > 0.125) / acceleration3SecondAverage.Count;
		}

		private static object PercentDecelerationTime(IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data)).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x < -0.125) / acceleration3SecondAverage.Count;
		}

		private static object PercentCruiseTime(IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data)).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x < 0.125 && x > -0.125) /
					acceleration3SecondAverage.Count;
		}

		private static SI PercentStopTime(IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data));
			if (acceleration3SecondAverage.Any()) {}
			var pStopTime = data.GetValues<SI>(ModalResultField.v_act)
				.Zip(SimulationIntervals(data), (velocity, dt) => new { velocity, dt })
				.Where(x => x.velocity < 0.1)
				.Sum(x => x.dt.Value());
			return 100 * pStopTime / SimulationIntervals(data).Sum();
		}

		private static SI AverageAccelerations(IModalDataWriter data)
		{
			return Accelerations(data).Average();
		}

		private static List<SI> Accelerations(IModalDataWriter data)
		{
			var accValues = data.GetValues<SI>(ModalResultField.acc).Cast<MeterPerSquareSecond>();
			var accelerations = CalculateAverageOverSeconds(SimulationIntervals(data), accValues).ToList();
			return accelerations;
		}

		private static Second[] SimulationIntervals(IModalDataWriter data)
		{
			return data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
		}

		private static SI AltitudeDelta(IModalDataWriter data)
		{
			return data.GetValues<SI>(ModalResultField.altitude).Last() -
					data.GetValues<SI>(ModalResultField.altitude).First();
		}

		private static SI PowerAccelerations(IModalDataWriter data)
		{
			var zero = 0.SI<Watt>();

			var paeng = data.Sum(ModalResultField.PaEng) ?? zero;
			var pagb = data.Sum(ModalResultField.PaGB) ?? zero;

			return paeng + pagb;
		}

		private static SI WorkTransmission(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			var plossdiff = TimeIntegralPower(ModalResultField.PlossGB, data, simulationIntervals);
			var plossgb = TimeIntegralPower(ModalResultField.PlossDiff, data, simulationIntervals);
			return plossdiff + plossgb;
		}

		private static object PowerLossRetarder(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.PlossRetarder, data, simulationIntervals).DefaultIfNull();
		}

		private static SI Duration(IModalDataWriter data)
		{
			return data.Max(ModalResultField.time) - data.Min(ModalResultField.time);
		}

		private static SI Distance(IModalDataWriter data)
		{
			return data.Max(ModalResultField.dist) - data.Min(ModalResultField.dist);
		}


		private static object WorkTotalMechanicalBrake(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Pbrake, data, simulationIntervals).DefaultIfNull();
		}

		private static object WorkAuxiliaries(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Paux, data, simulationIntervals).DefaultIfNull();
		}

		private static object WorkRoadGradientResistance(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Pgrad, data, simulationIntervals).DefaultIfNull();
		}

		private static object WorkRollingResistance(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Proll, data, simulationIntervals).DefaultIfNull();
		}

		private static object WorkAirResistance(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Pair, data, simulationIntervals).DefaultIfNull();
		}

		private static object WorkEngineNegative(IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x < 0).DefaultIfNull();
		}

		private static object WorkEnginePositive(IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x > 0).DefaultIfNull();
		}

		private static object PowerBrake(IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pbrake).DefaultIfNull();
		}

		private static object FuelConsumptionWHTCCorrected(IModalDataWriter data)
		{
			return data.Average(ModalResultField.FCWHTCc).DefaultIfNull();
		}

		private static object FuelConsumptionAuxStartStopCorrected(IModalDataWriter data)
		{
			return data.Average(ModalResultField.FCAUXc).DefaultIfNull();
		}

		private static SI FuelConsumption(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return
				(TimeIntegralFuelConsumption(ModalResultField.FCMap, data, simulationIntervals) / Duration(data)).ConvertTo()
					.Gramm.Per.Hour;
		}

		private static SI FuelConsumptionPerKilometer(IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return
				(TimeIntegralFuelConsumption(ModalResultField.FCMap, data, simulationIntervals) / Distance(data)).ConvertTo()
					.Gramm.Per.Kilo.Meter;
		}

		private static object Pneg(IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x < 0).DefaultIfNull();
		}

		private static object Ppos(IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x > 0).DefaultIfNull();
		}

		private static SI Speed(IModalDataWriter data)
		{
			return (Distance(data) / Duration(data)).ConvertTo().Kilo.Meter.Per.Hour;
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

		protected static SI TimeIntegralPower(ModalResultField field, IModalDataWriter data,
			IEnumerable<Second> simulationIntervals)
		{
			return data.GetValues<Watt>(field)
				.Zip(simulationIntervals, (P, dt) => dt.ConvertTo().Hour * (P ?? 0.SI<Watt>()).ConvertTo().Kilo.Watt)
				.Sum();
		}

		protected static Kilogram TimeIntegralFuelConsumption(ModalResultField field, IModalDataWriter data,
			IEnumerable<Second> simulationIntervals)
		{
			return data.GetValues<KilogramPerSecond>(field)
				.Zip(simulationIntervals, (FC, dt) => dt * (FC ?? 0.SI<KilogramPerSecond>()))
				.Sum().Cast<Kilogram>();
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
		Log.Info("Writing Summary File");
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