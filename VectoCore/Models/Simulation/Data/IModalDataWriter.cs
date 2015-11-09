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

		public static SI Average(this IEnumerable<SI> self, Func<SI, bool> filter = null)
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

		public static object AverageAccelerationsPositive(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data));
			return acceleration3SecondAverage.Average(x => x > 0.125).DefaultIfNull();
		}

		public static object AverageAccelerationsNegative(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data));
			return acceleration3SecondAverage.Average(x => x < -0.125).DefaultIfNull();
		}

		public static object PercentAccelerationTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data)).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x > 0.125) / acceleration3SecondAverage.Count;
		}

		public static object PercentDecelerationTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data)).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x < -0.125) / acceleration3SecondAverage.Count;
		}

		public static object PercentCruiseTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data)).ToList();
			return 100.SI<Scalar>() * acceleration3SecondAverage.Count(x => x < 0.125 && x > -0.125) /
					acceleration3SecondAverage.Count;
		}

		public static SI PercentStopTime(this IModalDataWriter data)
		{
			var acceleration3SecondAverage = Calculate3SecondAverage(Accelerations(data));
			if (acceleration3SecondAverage.Any()) {}
			var pStopTime = data.GetValues<SI>(ModalResultField.v_act)
				.Zip(SimulationIntervals(data), (velocity, dt) => new { velocity, dt })
				.Where(x => x.velocity < 0.1)
				.Sum(x => x.dt.Value());
			return 100 * pStopTime / SimulationIntervals(data).Sum();
		}

		public static SI AverageAccelerations(this IModalDataWriter data)
		{
			return Accelerations(data).Average();
		}

		public static List<SI> Accelerations(this IModalDataWriter data)
		{
			var accValues = data.GetValues<SI>(ModalResultField.acc).Cast<MeterPerSquareSecond>();
			var accelerations = CalculateAverageOverSeconds(SimulationIntervals(data), accValues).ToList();
			return accelerations;
		}

		public static Second[] SimulationIntervals(this IModalDataWriter data)
		{
			return data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
		}

		public static SI AltitudeDelta(this IModalDataWriter data)
		{
			return data.GetValues<SI>(ModalResultField.altitude).Last() -
					data.GetValues<SI>(ModalResultField.altitude).First();
		}

		public static SI PowerAccelerations(this IModalDataWriter data)
		{
			var zero = 0.SI<Watt>();

			var paeng = data.Sum(ModalResultField.PaEng) ?? zero;
			var pagb = data.Sum(ModalResultField.PaGB) ?? zero;

			return paeng + pagb;
		}

		public static SI WorkTransmission(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			var plossdiff = TimeIntegralPower(ModalResultField.PlossGB, data, simulationIntervals);
			var plossgb = TimeIntegralPower(ModalResultField.PlossDiff, data, simulationIntervals);
			return plossdiff + plossgb;
		}

		public static object PowerLossRetarder(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.PlossRetarder, data, simulationIntervals).DefaultIfNull();
		}

		public static SI Duration(this IModalDataWriter data)
		{
			return data.Max(ModalResultField.time) - data.Min(ModalResultField.time);
		}

		public static SI Distance(this IModalDataWriter data)
		{
			return data.Max(ModalResultField.dist) - data.Min(ModalResultField.dist);
		}


		public static object WorkTotalMechanicalBrake(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Pbrake, data, simulationIntervals).DefaultIfNull();
		}

		public static object WorkAuxiliaries(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Paux, data, simulationIntervals).DefaultIfNull();
		}

		public static object WorkRoadGradientResistance(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Pgrad, data, simulationIntervals).DefaultIfNull();
		}

		public static object WorkRollingResistance(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Proll, data, simulationIntervals).DefaultIfNull();
		}

		public static object WorkAirResistance(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return TimeIntegralPower(ModalResultField.Pair, data, simulationIntervals).DefaultIfNull();
		}

		public static object WorkEngineNegative(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x < 0).DefaultIfNull();
		}

		public static object WorkEnginePositive(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x > 0).DefaultIfNull();
		}

		public static object PowerBrake(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pbrake).DefaultIfNull();
		}

		public static object FuelConsumptionWHTCCorrected(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.FCWHTCc).DefaultIfNull();
		}

		public static object FuelConsumptionAuxStartStopCorrected(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.FCAUXc).DefaultIfNull();
		}

		public static SI FuelConsumption(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return
				(TimeIntegralFuelConsumption(ModalResultField.FCMap, data, simulationIntervals) / Duration(data)).ConvertTo()
					.Gramm.Per.Hour;
		}

		public static SI FuelConsumptionPerKilometer(this IModalDataWriter data)
		{
			var simulationIntervals = data.GetValues<Second>(ModalResultField.simulationInterval).ToArray();
			return
				(TimeIntegralFuelConsumption(ModalResultField.FCMap, data, simulationIntervals) / Distance(data)).ConvertTo()
					.Gramm.Per.Kilo.Meter;
		}

		public static object Pneg(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x < 0).DefaultIfNull();
		}

		public static object Ppos(this IModalDataWriter data)
		{
			return data.Average(ModalResultField.Pe_eng, x => x > 0).DefaultIfNull();
		}

		public static SI Speed(this IModalDataWriter data)
		{
			return (Distance(data) / Duration(data)).ConvertTo().Kilo.Meter.Per.Hour;
		}

		private static SI TimeIntegralPower(ModalResultField field, IModalDataWriter data,
			IEnumerable<Second> simulationIntervals)
		{
			return data.GetValues<Watt>(field)
				.Zip(simulationIntervals, (P, dt) => dt.ConvertTo().Hour * (P ?? 0.SI<Watt>()).ConvertTo().Kilo.Watt)
				.Sum();
		}

		private static Kilogram TimeIntegralFuelConsumption(ModalResultField field, IModalDataWriter data,
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
	}
}