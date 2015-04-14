using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
	/// <summary>
	/// Represents the Full load curve.
	/// </summary>
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
				throw new VectoException(
					"FullLoadCurve must consist of at least two lines with numeric values (below file header)");
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

			return new FullLoadCurve { _entries = entries };
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
					EngineSpeed = row.ParseDouble(Fields.EngineSpeed).RPMtoRad(),
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
					EngineSpeed = row.ParseDouble(0).RPMtoRad(),
					TorqueFullLoad = row.ParseDouble(1).SI<NewtonMeter>(),
					TorqueDrag = row.ParseDouble(2).SI<NewtonMeter>(),
					PT1 = row.ParseDouble(3).SI<Second>()
				}).ToList();
		}

		/// <summary>
		///     [rad/s] => [Nm]
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[Nm]</returns>
		public NewtonMeter FullLoadStationaryTorque(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate((double) _entries[idx - 1].EngineSpeed, (double) _entries[idx].EngineSpeed,
				(double) _entries[idx - 1].TorqueFullLoad, (double) _entries[idx].TorqueFullLoad,
				(double) angularVelocity).SI<NewtonMeter>();
		}

		/// <summary>
		///     [rad/s] => [W]
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[W]</returns>
		public Watt FullLoadStationaryPower(PerSecond angularVelocity)
		{
			return Formulas.TorqueToPower(FullLoadStationaryTorque(angularVelocity), angularVelocity);
		}

		/// <summary>
		///     [rad/s] => [Nm]
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[Nm]</returns>
		public NewtonMeter DragLoadStationaryTorque(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate((double) _entries[idx - 1].EngineSpeed, (double) _entries[idx].EngineSpeed,
				(double) _entries[idx - 1].TorqueDrag, (double) _entries[idx].TorqueDrag,
				(double) angularVelocity).SI<NewtonMeter>();
		}

		/// <summary>
		///     [rad/s] => [W].
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[W]</returns>
		public Watt DragLoadStationaryPower(PerSecond angularVelocity)
		{
			Contract.Requires(angularVelocity.HasEqualUnit(new SI().Radian.Per.Second));
			Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

			return Formulas.TorqueToPower(DragLoadStationaryTorque(angularVelocity), angularVelocity);
		}

		/// <summary>
		///     [rad/s] => [-]
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[-]</returns>
		public double PT1(PerSecond angularVelocity)
		{
			Contract.Requires(angularVelocity.HasEqualUnit(new SI().Radian.Per.Second));
			Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI()));

			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate(_entries[idx - 1].EngineSpeed.Double(),
				_entries[idx].EngineSpeed.Double(),
				_entries[idx - 1].PT1.Double(), _entries[idx].PT1.Double(),
				angularVelocity.Double());
		}

		/// <summary>
		///     [rad/s] => index. Get item index for angularVelocity.
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>index</returns>
		protected int FindIndex(PerSecond angularVelocity)
		{
			Contract.Requires(angularVelocity.HasEqualUnit(new SI().Radian.Per.Second));

			int idx;
			if (angularVelocity < _entries[0].EngineSpeed) {
				Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}",
					angularVelocity.ConvertTo().Rounds.Per.Minute, _entries[0].EngineSpeed.ConvertTo().Rounds.Per.Minute);
				idx = 1;
			} else {
				idx = _entries.FindIndex(x => x.EngineSpeed > angularVelocity);
			}
			if (idx <= 0) {
				idx = angularVelocity > _entries[0].EngineSpeed ? _entries.Count - 1 : 1;
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
			public PerSecond EngineSpeed { get; set; }

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

		private Tuple<PerSecond, Watt> FindMaxPower(FullLoadCurveEntry p1, FullLoadCurveEntry p2)
		{
			if (p1.EngineSpeed == p2.EngineSpeed) {
				return new Tuple<PerSecond, Watt>(p1.EngineSpeed, Formulas.TorqueToPower(p1.TorqueFullLoad, p1.EngineSpeed));
			}
			if (p2.EngineSpeed < p1.EngineSpeed) {
				var tmp = p1;
				p1 = p2;
				p2 = tmp;
			}
			// y = kx + d
			var k = (p2.TorqueFullLoad - p1.TorqueFullLoad) / (p2.EngineSpeed - p1.EngineSpeed);
			var d = p2.TorqueFullLoad - k * p2.EngineSpeed;
			if (k == 0.0.SI()) {
				return new Tuple<PerSecond, Watt>(p2.EngineSpeed, Formulas.TorqueToPower(p2.TorqueFullLoad, p2.EngineSpeed));
			}
			var engineSpeedMaxPower = (-1 * d / (2 * k)).Cast<PerSecond>();
			if (engineSpeedMaxPower < p1.EngineSpeed || engineSpeedMaxPower > p2.EngineSpeed) {
				if (k > 0) {
					return new Tuple<PerSecond, Watt>(p2.EngineSpeed, Formulas.TorqueToPower(p2.TorqueFullLoad, p2.EngineSpeed));
				}
				return new Tuple<PerSecond, Watt>(p1.EngineSpeed, Formulas.TorqueToPower(p1.TorqueFullLoad, p1.EngineSpeed));
			}
			//return null;
			var engineTorqueMaxPower = FullLoadStationaryTorque(engineSpeedMaxPower);
			return new Tuple<PerSecond, Watt>(engineSpeedMaxPower,
				Formulas.TorqueToPower(engineTorqueMaxPower, engineSpeedMaxPower));
		}

		public PerSecond RatedSpeed()
		{
			var max = new Tuple<PerSecond, Watt>(new PerSecond(), new Watt());
			for (var idx = 1; idx < _entries.Count; idx++) {
				var currentMax = FindMaxPower(_entries[idx - 1], _entries[idx]);
				if (currentMax.Item2 > max.Item2) {
					max = currentMax;
				}
			}

			return max.Item1;
		}
	}
}