using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class FullLoadCurve : SimulationComponentData
	{
		internal List<FullLoadCurveEntry> FullLoadEntries;
		internal LookupData<PerSecond, Second> PT1Data;

		protected PerSecond _ratedSpeed;
		protected Watt _maxPower;

		public static FullLoadCurve ReadFromFile(string fileName, bool declarationMode = false)
		{
			var data = VectoCSVFile.Read(fileName);

			if (data.Columns.Count < 3) {
				throw new VectoException("FullLoadCurve Data File must consist of at least 3 columns.");
			}

			if (data.Rows.Count < 2) {
				throw new VectoException(
					"FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			}

			List<FullLoadCurveEntry> entriesFld;
			if (HeaderIsValid(data.Columns)) {
				entriesFld = CreateFromColumnNames(data);
			} else {
				Logger<FullLoadCurve>().Warn(
					"FullLoadCurve: Header Line is not valid. Expected: '{0}, {1}, {2}', Got: '{3}'. Falling back to column index.",
					Fields.EngineSpeed, Fields.TorqueFullLoad,
					Fields.TorqueDrag,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

				entriesFld = CreateFromColumnIndizes(data);
			}

			LookupData<PerSecond, Second> tmp;
			if (declarationMode) {
				tmp = new PT1();
			} else {
				tmp = PT1Curve.ReadFromFile(fileName);
			}

			return new FullLoadCurve { FullLoadEntries = entriesFld, PT1Data = tmp };
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
		/// Get the rated speed from the given full-load curve (i.e. speed with max. power)
		/// </summary>
		public PerSecond RatedSpeed
		{
			get { return (_ratedSpeed ?? ComputeRatedSpeed().Item1); }
		}

		public Watt MaxPower
		{
			get { return (_maxPower ?? ComputeRatedSpeed().Item2); }
		}


		/// <summary>
		///	Compute the engine's rated speed from the given full-load curve (i.e. engine speed with max. power)
		/// </summary>
		protected Tuple<PerSecond, Watt> ComputeRatedSpeed()
		{
			var max = new Tuple<PerSecond, Watt>(0.SI<PerSecond>(), 0.SI<Watt>());
			for (var idx = 1; idx < FullLoadEntries.Count; idx++) {
				var currentMax = FindMaxPower(FullLoadEntries[idx - 1], FullLoadEntries[idx]);
				if (currentMax.Item2 > max.Item2) {
					max = currentMax;
				}
			}

			_ratedSpeed = max.Item1;
			_maxPower = max.Item2;

			return max;
		}

		private Tuple<PerSecond, Watt> FindMaxPower(FullLoadCurveEntry p1, FullLoadCurveEntry p2)
		{
			if (p1.EngineSpeed.IsEqual(p2.EngineSpeed)) {
				return Tuple.Create(p1.EngineSpeed, p1.TorqueFullLoad * p1.EngineSpeed);
			}

			if (p2.EngineSpeed < p1.EngineSpeed) {
				var tmp = p1;
				p1 = p2;
				p2 = tmp;
			}

			// y = kx + d
			var k = (p2.TorqueFullLoad - p1.TorqueFullLoad) / (p2.EngineSpeed - p1.EngineSpeed);
			var d = p2.TorqueFullLoad - k * p2.EngineSpeed;
			if (k.IsEqual(0)) {
				return Tuple.Create(p2.EngineSpeed, p2.TorqueFullLoad * p2.EngineSpeed);
			}
			var engineSpeedMaxPower = -d / (2 * k);
			if (engineSpeedMaxPower.IsSmaller(p1.EngineSpeed) || engineSpeedMaxPower.IsGreater(p2.EngineSpeed)) {
				if (k.IsGreater(0)) {
					return Tuple.Create(p2.EngineSpeed, p2.TorqueFullLoad * p2.EngineSpeed);
				}
				return Tuple.Create(p1.EngineSpeed, p1.TorqueFullLoad * p1.EngineSpeed);
			}
			var engineTorqueMaxPower = FullLoadStationaryTorque(engineSpeedMaxPower);
			return Tuple.Create(engineSpeedMaxPower, engineTorqueMaxPower * engineSpeedMaxPower);
		}

		public virtual NewtonMeter FullLoadStationaryTorque(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate(FullLoadEntries[idx - 1].EngineSpeed, FullLoadEntries[idx].EngineSpeed,
				FullLoadEntries[idx - 1].TorqueFullLoad, FullLoadEntries[idx].TorqueFullLoad,
				angularVelocity);
		}

		public virtual NewtonMeter DragLoadStationaryTorque(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate(FullLoadEntries[idx - 1].EngineSpeed, FullLoadEntries[idx].EngineSpeed,
				FullLoadEntries[idx - 1].TorqueDrag, FullLoadEntries[idx].TorqueDrag,
				angularVelocity);
		}

		/// <summary>
		///     [rad/s] => index. Get item index for angularVelocity.
		/// </summary>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>index</returns>
		protected int FindIndex(PerSecond angularVelocity)
		{
			int index;
			FullLoadEntries.GetSection(x => x.EngineSpeed < angularVelocity, out index,
				string.Format("requested rpm outside of FLD curve - extrapolating. rpm: {0}",
					angularVelocity.ConvertTo().Rounds.Per.Minute));
			return index + 1;
		}

		internal class FullLoadCurveEntry
		{
			public PerSecond EngineSpeed { get; set; }

			public NewtonMeter TorqueFullLoad { get; set; }

			public NewtonMeter TorqueDrag { get; set; }

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
				if (obj.GetType() != GetType()) {
					return false;
				}
				return Equals((FullLoadCurveEntry)obj);
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
	}
}