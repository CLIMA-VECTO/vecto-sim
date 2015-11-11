using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public interface IModalDataWriter
	{
		/// <summary>
		/// Indexer for fields of the DataWriter. Accesses the data of the current step.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[ModalResultField key] { get; set; }

		/// <summary>
		/// Indexer for auxiliary fields of the DataWriter.
		/// </summary>
		/// <param name="auxId"></param>
		/// <returns></returns>
		object this[string auxId] { get; set; }

		bool HasTorqueConverter { get; set; }

		/// <summary>
		/// Commits the data of the current simulation step.
		/// </summary>
		void CommitSimulationStep();

		VectoRun.Status RunStatus { get; }


		/// <summary>
		/// Finishes the writing of the DataWriter.
		/// </summary>
		void Finish(VectoRun.Status runStatus);

		bool WriteModalResults { get; set; }

		IEnumerable<T> GetValues<T>(ModalResultField key);

		IEnumerable<T> GetValues<T>(DataColumn col);


		Dictionary<string, DataColumn> Auxiliaries { get; set; }

		void AddAuxiliary(string id);
	}

	public static class ModalDataWriterExtensions
	{
		public static SI Max(this IModalDataWriter data, ModalResultField field)
		{
			return data.GetValues<SI>(field).Max();
		}

		public static SI Min(this IModalDataWriter data, ModalResultField field)
		{
			return data.GetValues<SI>(field).Min();
		}

		public static SI Average(this IModalDataWriter data, ModalResultField field, Func<SI, bool> filter = null)
		{
			return data.GetValues<SI>(field).Average(filter);
		}

		public static SI Sum(this IModalDataWriter data, ModalResultField field, Func<SI, bool> filter = null)
		{
			return data.GetValues<SI>(field).Where(filter ?? (x => x != null)).Sum();
		}

		public static SI Sum(this IModalDataWriter data, DataColumn col, Func<SI, bool> filter = null)
		{
			return data.GetValues<SI>(col).Where(filter ?? (x => x != null)).Sum();
		}

		public static SI Average(this IEnumerable<SI> self, Func<SI, bool> filter)
		{
			var values = self.Where(filter ?? (x => x != null && !double.IsNaN(x.Value()))).ToList();
			return values.Any() ? values.Sum() / values.Count : null;
		}

		public static object DefaultIfNull(this object self)
		{
			return self ?? DBNull.Value;
		}

		public static T DefaultIfNull<T>(this T self, T defaultValue) where T : class
		{
			return self ?? defaultValue;
		}

		public static MeterPerSquareSecond AccelerationsPositive3SecondAverage(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = AccelerationPer3Seconds(data);
			return acceleration3SecondAverage.Where(x => x > 0.125).Average();
		}

		public static MeterPerSquareSecond AccelerationNoise(this IModalDataWriter data)
		{
			var avg = data.AccelerationAverage();
			var accelerationAverages = AccelerationPerSecond(data).ToList();
			var sqareAvg = accelerationAverages.Select(x => (x - avg) * (x - avg)).Sum() / accelerationAverages.Count;
			return sqareAvg.Sqrt().Cast<MeterPerSquareSecond>();
		}

		public static MeterPerSquareSecond AverageAccelerations3SecondNegative(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = AccelerationPer3Seconds(data);
			return acceleration3SecondAverage.Where(x => x < -0.125).Average();
		}

		public static Scalar PercentAccelerationTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = AccelerationPer3Seconds(data).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x > 0.125) / acceleration3SecondAverage.Count;
		}

		public static Scalar PercentDecelerationTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = AccelerationPer3Seconds(data).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x < -0.125) / acceleration3SecondAverage.Count;
		}

		public static Scalar PercentCruiseTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = AccelerationPer3Seconds(data).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x.IsBetween(-0.125, -0.125)) /
					acceleration3SecondAverage.Count;
		}

		public static Scalar PercentStopTime(this IModalDataWriter data)
		{
			var stopTime = data.GetValues<MeterPerSecond>(ModalResultField.v_act)
				.Zip(data.SimulationIntervals(), (v, dt) => new { v, dt })
				.Where(x => x.v < 0.1).Select(x => x.dt).Sum();

			return 100 * (stopTime / data.Duration()).Cast<Scalar>();
		}

		public static MeterPerSquareSecond AccelerationAverage(this IModalDataWriter data)
		{
			return data.TimeIntegral<MeterPerSecond>(ModalResultField.acc) / data.Duration();
		}

		public static Second[] SimulationIntervals(this IModalDataWriter data)
		{
			return data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
		}

		public static Meter AltitudeDelta(this IModalDataWriter data)
		{
			return data.GetValues<Meter>(ModalResultField.altitude).Last() -
					data.GetValues<Meter>(ModalResultField.altitude).First();
		}

		public static WattSecond PowerAccelerations(this IModalDataWriter data)
		{
			var paEngine = data.TimeIntegral<WattSecond>(ModalResultField.PaEng);
			var paGearbox = data.TimeIntegral<WattSecond>(ModalResultField.PaGB);
			return paEngine + paGearbox;
		}

		public static WattSecond WorkTransmission(this IModalDataWriter data)
		{
			var plossdiff = data.TimeIntegral<WattSecond>(ModalResultField.PlossGB);
			var plossgb = data.TimeIntegral<WattSecond>(ModalResultField.PlossDiff);
			return plossdiff + plossgb;
		}

		public static WattSecond WorkRetarder(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.PlossRetarder);
		}

		public static WattSecond WorkTorqueConverter(this IModalDataWriter data)
		{
			//TODO (MK, 2015-11-10): return torque converter work - this was currently not possible because torque converter is not implemented.
			return 0.SI<WattSecond>();
		}

		public static Second Duration(this IModalDataWriter data)
		{
			return (data.Max(ModalResultField.time) - data.Min(ModalResultField.time)).Cast<Second>();
		}

		public static Meter Distance(this IModalDataWriter data)
		{
			return (data.Max(ModalResultField.dist) - data.Min(ModalResultField.dist)).Cast<Meter>();
		}

		public static WattSecond WorkTotalMechanicalBrake(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pbrake);
		}

		public static WattSecond WorkAuxiliaries(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Paux);
		}

		public static WattSecond WorkRoadGradientResistance(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pgrad);
		}

		public static WattSecond WorkRollingResistance(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Proll);
		}

		public static WattSecond WorkAirResistance(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pair);
		}

		public static WattSecond EngineWorkPositive(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pe_eng, x => x > 0);
		}

		public static WattSecond EngineWorkNegative(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pe_eng, x => x < 0);
		}

		public static Watt PowerBrake(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pbrake) / data.Duration();
		}

		public static Watt PowerWheelPositive(this IModalDataWriter data)
		{
			return data.TimeIntegral<WattSecond>(ModalResultField.Pwheel, x => x > 0) / data.Duration();
		}

		public static KilogramPerMeter FuelConsumptionWHTCCorrected(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCWHTCc) / data.Distance();
		}

		public static KilogramPerSecond FuelConsumptionWHTCCorrectedPerSecond(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCWHTCc) / data.Duration();
		}

		public static KilogramPerMeter FuelConsumptionAuxStartStopCorrected(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCAUXc) / data.Distance();
		}

		public static KilogramPerSecond FuelConsumptionAuxStartStopCorrectedPerSecond(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCAUXc) / data.Duration();
		}

		public static KilogramPerMeter FuelConsumptionFinal(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCWHTCc) / data.Distance();
		}

		public static SI FuelConsumptionFinalLiterPer100Kilometer(this IModalDataWriter data)
		{
			var fcVolumePerMeter = data.FuelConsumptionFinal() / Physics.FuelDensity;
			return fcVolumePerMeter.ConvertTo().Cubic.Dezi.Meter * 100.SI().Kilo.Meter;
		}

		public static KilogramPerMeter CO2PerMeter(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCMap) * Physics.CO2PerFuelWeight / data.Distance();
		}

		public static SI FuelConsumptionLiterPer100Kilometer(this IModalDataWriter data)
		{
			var fcVolumePerMeter = data.FuelConsumptionPerMeter() / Physics.FuelDensity;
			return fcVolumePerMeter.ConvertTo().Cubic.Dezi.Meter * 100.SI().Kilo.Meter;
		}

		public static KilogramPerSecond FuelConsumptionPerSecond(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCMap) / data.Duration();
		}

		public static KilogramPerMeter FuelConsumptionPerMeter(this IModalDataWriter data)
		{
			return data.TimeIntegral<Kilogram>(ModalResultField.FCMap) / data.Distance();
		}

		public static Watt EnginePowerNegativeAverage(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval);
			var values = data.GetValues<Watt>(ModalResultField.Pe_eng)
				.Zip(simulationIntervals, (value, dt) => new { Dt = dt, Value = value * dt })
				.Where(v => v.Value < 0).ToList();
			if (values.Any()) {
				return values.Select(v => v.Value).Sum() / values.Select(v => v.Dt).Sum();
			}
			return 0.SI<Watt>();
		}

		public static Watt EnginePowerPositiveAverage(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval);
			var values = data.GetValues<Watt>(ModalResultField.Pe_eng)
				.Zip(simulationIntervals, (value, dt) => new { Dt = dt, Value = value * dt })
				.Where(v => v.Value > 0).ToList();
			if (values.Any()) {
				return values.Select(v => v.Value).Sum() / values.Select(v => v.Dt).Sum();
			}
			return 0.SI<Watt>();
		}

		public static MeterPerSecond Speed(this IModalDataWriter data)
		{
			return Distance(data) / Duration(data);
		}

		public static WattSecond AuxiliaryWork(this IModalDataWriter data, DataColumn auxCol)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval);
			return data.GetValues<Watt>(auxCol).Zip(simulationIntervals, (value, dt) => value * dt).Sum().Cast<WattSecond>();
		}


		private static T TimeIntegral<T>(this IModalDataWriter data, ModalResultField field, Func<SI, bool> filter = null)
			where T : SIBase<T>
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval);
			var filteredList = data.GetValues<SI>(field)
				.Zip(simulationIntervals, (value, dt) => new { value, dt })
				.Where(x => filter == null || filter(x.value)).ToList();

			return filteredList.Any()
				? filteredList.Select(x => (x.value == null ? SIBase<T>.Create(0) : x.value * x.dt)).Sum().Cast<T>()
				: SIBase<T>.Create(0);
		}

		private static IEnumerable<MeterPerSquareSecond> AccelerationPer3Seconds(IModalDataWriter data)
		{
			var accelerationAverages = AccelerationPerSecond(data).ToList();
			var runningAverage = (accelerationAverages[0] + accelerationAverages[1] + accelerationAverages[2]) / 3.0;
			yield return runningAverage;

			for (var i = 2; i < accelerationAverages.Count() - 1; i++) {
				runningAverage -= accelerationAverages[i - 2] / 3.0;
				runningAverage += accelerationAverages[i + 1] / 3.0;
				yield return runningAverage;
			}
		}

		/// <summary>
		/// Calculates the average acceleration for whole seconds.
		/// </summary>
		private static IEnumerable<MeterPerSquareSecond> AccelerationPerSecond(IModalDataWriter data)
		{
			var dtSum = 0.SI<Second>();
			var accAvg = 0.SI<MeterPerSecond>();

			var accValues = data.GetValues<MeterPerSquareSecond>(ModalResultField.acc);

			foreach (var value in accValues.Zip(SimulationIntervals(data), (acc, dt) => new { acc, dt })) {
				var dt = value.dt;
				var acc = value.acc;

				while (dtSum + dt >= 1) {
					var diffDt = 1.SI<Second>() - dtSum;
					yield return (accAvg + acc * diffDt) / 1.SI<Second>();
					dt -= diffDt;
					dtSum = 0.SI<Second>();
					accAvg = 0.SI<MeterPerSecond>();
				}
				if (dt > 0) {
					accAvg += acc * dt;
					dtSum += dt;
				}
			}

			// return remaining data. acts like extrapolation to next whole second.
			if (dtSum > 0) {
				yield return accAvg / 1.SI<Second>();
			}
		}
	}
}