using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

		/// <summary>
		/// Finishes the writing of the DataWriter.
		/// </summary>
		void Finish();

		IEnumerable<T> GetValues<T>(ModalResultField key);

		IEnumerable<T> GetValues<T>(DataColumn col);


		Dictionary<string, DataColumn> Auxiliaries { get; set; }

		void AddAuxiliary(string id);
	}

	public static class ModalDataWriterExtensions
	{
		public static double? Max(this IModalDataWriter data, ModalResultField field)
		{
			var val = data.GetValues<SI>(field).Max();
			if (val != null) {
				return val.Value();
			}
			return null;
		}

		public static double? Average(this IModalDataWriter data, ModalResultField field, Func<SI, bool> filter = null)
		{
			var val = data.GetValues<SI>(field).Where(filter ?? (x => x != null)).ToList();
			if (val.Any()) {
				return val.ToDouble().Average();
			}
			return null;
		}

		public static double? Sum(this IModalDataWriter data, ModalResultField field, Func<SI, bool> filter = null)
		{
			var val = data.GetValues<SI>(field).Where(filter ?? (x => x != null)).ToList();
			if (val.Any()) {
				return val.ToDouble().Sum();
			}
			return null;
		}

		public static double? Sum(this IModalDataWriter data, DataColumn col, Func<SI, bool> filter = null)
		{
			var val = data.GetValues<SI>(col).Where(filter ?? (x => x != null)).ToList();
			if (val.Any()) {
				return val.ToDouble().Sum();
			}
			return null;
		}

		public static double? Average(this IEnumerable<SI> self, Func<SI, bool> filter = null)
		{
			var val = self.Where(filter ?? (x => x != null)).ToList();
			if (val.Any()) {
				return val.ToDouble().Average();
			}
			return null;
		}

		public static object DefaultIfNull(this object self)
		{
			return self ?? DBNull.Value;
		}

		public static T DefaultIfNull<T>(this T self, T defaultValue) where T : class
		{
			return self ?? defaultValue;
		}
	}
}