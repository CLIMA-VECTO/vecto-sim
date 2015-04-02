using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
	public class FullLoadCurve : SimulationComponentData
	{
		[JsonProperty] private List<FullLoadCurveEntry> _entries;

		public static FullLoadCurve ReadFromFile(string fileName)
		{
			var data = VectoCSVFile.Read(fileName);

			//todo Contract.Requires<VectoException>(data.Columns.Count != 4, "FullLoadCurve Data File must consist of 4 columns.");
			if (data.Columns.Count != 4) {
				throw new VectoException("FullLoadCurve Data File must consist of 4 columns.");
			}

			//todo Contract.Requires<VectoException>(data.Rows.Count < 2, "FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			if (data.Rows.Count < 2) {
				throw new VectoException("FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			}

			List<FullLoadCurveEntry> entries;
			if (HeaderIsValid(data.Columns)) {
				entries = CreateFromColumnNames(data);
			} else {
				var log = LogManager.GetLogger<FullLoadCurve>();
				log.WarnFormat(
					"FullLoadCurve: Header Line is not valid. Expected: '{0}, {1}, {2}, {3}', Got: '{4}'. Falling back to column index.",
					Fields.EngineSpeed, Fields.TorqueFullLoad, Fields.TorqueDrag, Fields.PT1,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));

				entries = CreateFromColumnIndizes(data);
			}

			return new FullLoadCurve {_entries = entries};
		}

		private static bool HeaderIsValid(DataColumnCollection columns)
		{
			Contract.Requires(columns != null);
			return columns.Contains(Fields.EngineSpeed)
					&& columns.Contains(Fields.TorqueDrag)
					&& columns.Contains(Fields.TorqueFullLoad)
					&& columns.Contains(Fields.PT1);
		}

		private static List<FullLoadCurveEntry> CreateFromColumnNames(DataTable data)
		{
			Contract.Requires(data != null);
			return (from DataRow row in data.Rows
				select new FullLoadCurveEntry {
					EngineSpeed = row.ParseDouble(Fields.EngineSpeed).SI().Rounds.Per.Minute.To<RadianPerSecond>(),
					TorqueFullLoad = row.ParseDouble(Fields.TorqueFullLoad).SI<NewtonMeter>(),
					TorqueDrag = row.ParseDouble(Fields.TorqueDrag).SI<NewtonMeter>(),
					PT1 = row.ParseDouble(Fields.PT1).SI<Second>()
				}).ToList();
		}

		private static List<FullLoadCurveEntry> CreateFromColumnIndizes(DataTable data)
		{
			Contract.Requires(data != null);
			return (from DataRow row in data.Rows
				select new FullLoadCurveEntry {
					EngineSpeed = row.ParseDouble(0).SI().Rounds.Per.Minute.To<RadianPerSecond>(),
					TorqueFullLoad = row.ParseDouble(1).SI<NewtonMeter>(),
					TorqueDrag = row.ParseDouble(2).SI<NewtonMeter>(),
					PT1 = row.ParseDouble(3).SI<Second>()
				}).ToList();
		}

		/// <summary>
		///     [rad/s] => [Nm]
		/// </summary>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>[Nm]</returns>
		public NewtonMeter FullLoadStationaryTorque(RadianPerSecond angularFrequency)
		{
			var idx = FindIndex(angularFrequency);
			return VectoMath.Interpolate((double) _entries[idx - 1].EngineSpeed, (double) _entries[idx].EngineSpeed,
				(double) _entries[idx - 1].TorqueFullLoad, (double) _entries[idx].TorqueFullLoad,
				(double) angularFrequency).SI<NewtonMeter>();
		}

		/// <summary>
		///     [rad/s] => [W]
		/// </summary>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>[W]</returns>
		public Watt FullLoadStationaryPower(RadianPerSecond angularFrequency)
		{
			return Formulas.TorqueToPower(FullLoadStationaryTorque(angularFrequency), angularFrequency);
		}

		/// <summary>
		///     [rad/s] => [Nm]
		/// </summary>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>[Nm]</returns>
		public NewtonMeter DragLoadStationaryTorque(RadianPerSecond angularFrequency)
		{
			var idx = FindIndex(angularFrequency);
			return VectoMath.Interpolate((double) _entries[idx - 1].EngineSpeed, (double) _entries[idx].EngineSpeed,
				(double) _entries[idx - 1].TorqueDrag, (double) _entries[idx].TorqueDrag,
				(double) angularFrequency).SI<NewtonMeter>();
		}

		/// <summary>
		///     [rad/s] => [W].
		/// </summary>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>[W]</returns>
		public Watt DragLoadStationaryPower(RadianPerSecond angularFrequency)
		{
			Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
			Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

			return Formulas.TorqueToPower(DragLoadStationaryTorque(angularFrequency), angularFrequency);
		}

		/// <summary>
		///     [rad/s] => [-]
		/// </summary>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>[-]</returns>
		public SI PT1(SI angularFrequency)
		{
			Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
			Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI()));

			var idx = FindIndex(angularFrequency);
			return VectoMath.Interpolate((double) _entries[idx - 1].EngineSpeed, (double) _entries[idx].EngineSpeed,
				(double) _entries[idx - 1].PT1, (double) _entries[idx].PT1,
				(double) angularFrequency).SI();
		}

		/// <summary>
		///     [rad/s] => index. Get item index for engineSpeed.
		/// </summary>
		/// <param name="engineSpeed">[rad/s]</param>
		/// <returns>index</returns>
		protected int FindIndex(SI engineSpeed)
		{
			Contract.Requires(engineSpeed.HasEqualUnit(new SI().Radian.Per.Second));

			int idx;
			if (engineSpeed < _entries[0].EngineSpeed) {
				Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}",
					engineSpeed.To().Rounds.Per.Minute, _entries[0].EngineSpeed.To().Rounds.Per.Minute);
				idx = 1;
			} else {
				idx = _entries.FindIndex(x => x.EngineSpeed > engineSpeed);
			}
			if (idx <= 0) {
				idx = engineSpeed > _entries[0].EngineSpeed ? _entries.Count - 1 : 1;
			}
			return idx;
		}

		private static class Fields
		{
			/// <summary>
			///     [rpm] engine speed
			/// </summary>
			public const string EngineSpeed = "engine speed";

			/// <summary>
			///     [Nm] full load torque
			/// </summary>
			public const string TorqueFullLoad = "full load torque";

			/// <summary>
			///     [Nm] motoring torque
			/// </summary>
			public const string TorqueDrag = "motoring torque";

			/// <summary>
			///     [s] time constant
			/// </summary>
			public const string PT1 = "PT1";
		}

		private class FullLoadCurveEntry
		{
			/// <summary>
			///     [rad/s] engine speed
			/// </summary>
			public RadianPerSecond EngineSpeed { get; set; }

			/// <summary>
			///     [Nm] full load torque
			/// </summary>
			public NewtonMeter TorqueFullLoad { get; set; }

			/// <summary>
			///     [Nm] motoring torque
			/// </summary>
			public NewtonMeter TorqueDrag { get; set; }

			/// <summary>
			///     [s] PT1 time constant
			/// </summary>
			public Second PT1 { get; set; }

			#region Equality members

			protected bool Equals(FullLoadCurveEntry other)
			{
				Contract.Requires(other != null);
				return EngineSpeed.Equals(other.EngineSpeed)
						&& TorqueFullLoad.Equals(other.TorqueFullLoad)
						&& TorqueDrag.Equals(other.TorqueDrag)
						&& PT1.Equals(other.PT1);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				if (ReferenceEquals(this, obj)) {
					return true;
				}
				return obj.GetType() == GetType() && Equals((FullLoadCurveEntry) obj);
			}

			public override int GetHashCode()
			{
				var hashCode = EngineSpeed.GetHashCode();
				hashCode = (hashCode * 397) ^ TorqueFullLoad.GetHashCode();
				hashCode = (hashCode * 397) ^ TorqueDrag.GetHashCode();
				hashCode = (hashCode * 397) ^ PT1.GetHashCode();
				return hashCode;
			}

			#endregion
		}

		#region Equality members

		protected bool Equals(FullLoadCurve other)
		{
			return _entries.SequenceEqual(other._entries);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			return obj.GetType() == GetType() && Equals((FullLoadCurve) obj);
		}

		public override int GetHashCode()
		{
			return (_entries != null ? _entries.GetHashCode() : 0);
		}

		#endregion
	}
}