using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
	/// <summary>
	/// Represents the Full load curve.
	/// </summary>
	public class FullLoadCurve : SimulationComponentData
	{
		private List<FullLoadCurveEntry> _fullLoadEntries;
		private LookupData<PerSecond, Second> _pt1Data;

		public static FullLoadCurve ReadFromFile(string fileName, bool declarationMode = false)
		{
			var data = VectoCSVFile.Read(fileName);

			//todo Contract.Requires<VectoException>(data.Columns.Count != 4, "FullLoadCurve Data File must consist of 4 columns.");
			if (data.Columns.Count < 3) {
				throw new VectoException("FullLoadCurve Data File must consist of at least 3 columns.");
			}

			//todo Contract.Requires<VectoException>(data.Rows.Count < 2, "FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			if (data.Rows.Count < 2) {
				throw new VectoException(
					"FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			}

			List<FullLoadCurveEntry> entriesFld;
			if (HeaderIsValid(data.Columns)) {
				entriesFld = CreateFromColumnNames(data);
			} else {
				var log = LogManager.GetLogger<FullLoadCurve>();
				log.WarnFormat(
					"FullLoadCurve: Header Line is not valid. Expected: '{0}, {1}, {2}', Got: '{3}'. Falling back to column index.",
					Fields.EngineSpeed, Fields.TorqueFullLoad, Fields.TorqueDrag,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));

				entriesFld = CreateFromColumnIndizes(data);
			}

			LookupData<PerSecond, Second> tmp;
			if (declarationMode) {
				tmp = new DeclarationPT1();
			} else {
				tmp = PT1Curve.ReadFromFile(fileName);
			}

			return new FullLoadCurve { _fullLoadEntries = entriesFld, _pt1Data = tmp };
		}

		private static bool HeaderIsValid(DataColumnCollection columns)
		{
			Contract.Requires(columns != null);
			return columns.Contains(Fields.EngineSpeed)
					&& columns.Contains(Fields.TorqueDrag)
					&& columns.Contains(Fields.TorqueFullLoad);
			//&& columns.Contains(Fields.PT1);
		}

		private static List<FullLoadCurveEntry> CreateFromColumnNames(DataTable data)
		{
			Contract.Requires(data != null);
			return (from DataRow row in data.Rows
				select new FullLoadCurveEntry {
					EngineSpeed = row.ParseDouble(Fields.EngineSpeed).RPMtoRad(),
					TorqueFullLoad = row.ParseDouble(Fields.TorqueFullLoad).SI<NewtonMeter>(),
					TorqueDrag = row.ParseDouble(Fields.TorqueDrag).SI<NewtonMeter>(),
					//PT1 = row.ParseDouble(Fields.PT1).SI<Second>()
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
					//PT1 = row.ParseDouble(3).SI<Second>()
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
			return VectoMath.Interpolate(_fullLoadEntries[idx - 1].EngineSpeed, _fullLoadEntries[idx].EngineSpeed,
				_fullLoadEntries[idx - 1].TorqueFullLoad, _fullLoadEntries[idx].TorqueFullLoad,
				angularVelocity);
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
			return VectoMath.Interpolate(_fullLoadEntries[idx - 1].EngineSpeed, _fullLoadEntries[idx].EngineSpeed,
				_fullLoadEntries[idx - 1].TorqueDrag, _fullLoadEntries[idx].TorqueDrag,
				angularVelocity);
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
		///     [rad/s] => [s]
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>[s]</returns>
		public Second PT1(PerSecond angularVelocity)
		{
			return _pt1Data.Lookup(angularVelocity);
		}

		/// <summary>
		///		Get the engine's rated speed from the given full-load curve (i.e. engine speed with max. power)
		/// </summary>
		/// <returns>[1/s]</returns>
		public PerSecond RatedSpeed()
		{
			var max = new Tuple<PerSecond, Watt>(0.SI<PerSecond>(), 0.SI<Watt>());
			for (var idx = 1; idx < _fullLoadEntries.Count; idx++) {
				var currentMax = FindMaxPower(_fullLoadEntries[idx - 1], _fullLoadEntries[idx]);
				if (currentMax.Item2 > max.Item2) {
					max = currentMax;
				}
			}

			return max.Item1;
		}


		/// <summary>
		///     [rad/s] => index. Get item index for angularVelocity.
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>index</returns>
		protected int FindIndex(PerSecond angularVelocity)
		{
			int idx;
			if (angularVelocity < _fullLoadEntries[0].EngineSpeed) {
				Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}",
					angularVelocity.ConvertTo().Rounds.Per.Minute, _fullLoadEntries[0].EngineSpeed.ConvertTo().Rounds.Per.Minute);
				idx = 1;
			} else {
				idx = _fullLoadEntries.FindIndex(x => x.EngineSpeed > angularVelocity);
			}
			if (idx <= 0) {
				idx = angularVelocity > _fullLoadEntries[0].EngineSpeed ? _fullLoadEntries.Count - 1 : 1;
			}
			return idx;
		}

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
			if (k == 0.SI()) {
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

			///// <summary>
			/////     [s] PT1 time constant
			///// </summary>
			//public Second PT1 { get; set; }

			#region Equality members

			protected bool Equals(FullLoadCurveEntry other)
			{
				return Equals(EngineSpeed, other.EngineSpeed) && Equals(TorqueFullLoad, other.TorqueFullLoad) &&
						Equals(TorqueDrag, other.TorqueDrag);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				if (ReferenceEquals(this, obj)) {
					return true;
				}
				if (obj.GetType() != this.GetType()) {
					return false;
				}
				return Equals((FullLoadCurveEntry) obj);
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = (EngineSpeed != null ? EngineSpeed.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (TorqueFullLoad != null ? TorqueFullLoad.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (TorqueDrag != null ? TorqueDrag.GetHashCode() : 0);
					return hashCode;
				}
			}

			#endregion
		}

		#region Equality members

		protected bool Equals(FullLoadCurve other)
		{
			return Equals(_fullLoadEntries, other._fullLoadEntries) && Equals(_pt1Data, other._pt1Data);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != this.GetType()) {
				return false;
			}
			return Equals((FullLoadCurve) obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				return ((_fullLoadEntries != null ? _fullLoadEntries.GetHashCode() : 0) * 397) ^
						(_pt1Data != null ? _pt1Data.GetHashCode() : 0);
			}
		}

		#endregion
	}
}