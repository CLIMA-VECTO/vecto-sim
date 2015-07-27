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
	public class EngineFullLoadCurve : SimulationComponentData
	{
		private List<FullLoadCurveEntry> _fullLoadEntries;
		private LookupData<PerSecond, Second> _pt1Data;

		private Watt _maxPower;

		private PerSecond _ratedSpeed;
		private PerSecond _preferredSpeed;
		private PerSecond _engineSpeedLo; // 55% of Pmax
		private PerSecond _engineSpeedHi; // 70% of Pmax
		private PerSecond _n95hSpeed; // 95% of Pmax

		public static EngineFullLoadCurve ReadFromFile(string fileName, bool declarationMode = false)
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
				var log = LogManager.GetLogger<EngineFullLoadCurve>();
				log.WarnFormat(
					"FullLoadCurve: Header Line is not valid. Expected: '{0}, {1}, {2}', Got: '{3}'. Falling back to column index.",
					Fields.EngineSpeed, Fields.TorqueFullLoad, Fields.TorqueDrag,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));

				entriesFld = CreateFromColumnIndizes(data);
			}

			LookupData<PerSecond, Second> tmp;
			if (declarationMode) {
				tmp = new PT1();
			} else {
				tmp = PT1Curve.ReadFromFile(fileName);
			}

			return new EngineFullLoadCurve { _fullLoadEntries = entriesFld, _pt1Data = tmp };
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

		public CombustionEngineData EngineData { get; internal set; }

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
		public PerSecond RatedSpeed
		{
			get
			{
				if (_ratedSpeed == null) {
					ComputeRatedSpeed();
				}
				return _ratedSpeed;
			}
		}

		public Watt MaxPower
		{
			get
			{
				if (_maxPower == null) {
					ComputeRatedSpeed();
				}
				return _maxPower;
			}
		}

		/// <summary>
		///		Get the engine's preferred speed from the given full-load curve (i.e. Speed at 51% torque/speed-integral between idling and N95h.)
		/// </summary>
		public PerSecond PreferredSpeed
		{
			get
			{
				if (_preferredSpeed == null) {
					ComputePreferredSpeed();
				}
				return _preferredSpeed;
			}
		}

		public PerSecond N95hSpeed
		{
			get { return _n95hSpeed ?? (_n95hSpeed = FindEngineSpeedForPower(0.95 * MaxPower).Last()); }
		}


		public PerSecond LoSpeed
		{
			get { return _engineSpeedLo ?? (_engineSpeedLo = FindEngineSpeedForPower(0.55 * MaxPower).First()); }
		}

		public PerSecond HiSpeed
		{
			get { return _engineSpeedHi ?? (_engineSpeedHi = FindEngineSpeedForPower(0.7 * MaxPower).Last()); }
		}


		public NewtonMeter MaxLoadTorque
		{
			get { return _fullLoadEntries.Max(x => x.TorqueFullLoad); }
		}

		public NewtonMeter MaxDragTorque
		{
			get { return _fullLoadEntries.Min(x => x.TorqueDrag); }
		}

		/// <summary>
		///		Compute the engine's rated speed from the given full-load curve (i.e. engine speed with max. power)
		/// </summary>
		/// <returns>[1/s]</returns>
		private void ComputeRatedSpeed()
		{
			var max = new Tuple<PerSecond, Watt>(0.SI<PerSecond>(), 0.SI<Watt>());
			for (var idx = 1; idx < _fullLoadEntries.Count; idx++) {
				var currentMax = FindMaxPower(_fullLoadEntries[idx - 1], _fullLoadEntries[idx]);
				if (currentMax.Item2 > max.Item2) {
					max = currentMax;
				}
			}

			_ratedSpeed = max.Item1;
			_maxPower = max.Item2;
		}


		private void ComputePreferredSpeed()
		{
			var maxArea = ComputeArea(EngineData.IdleSpeed, N95hSpeed);

			var area = 0.0;
			var idx = 0;
			while (++idx < _fullLoadEntries.Count) {
				var additionalArea = ComputeArea(_fullLoadEntries[idx - 1].EngineSpeed, _fullLoadEntries[idx].EngineSpeed);
				if (area + additionalArea > 0.51 * maxArea) {
					var deltaArea = 0.51 * maxArea - area;
					_preferredSpeed = ComputeEngineSpeedForSegmentArea(_fullLoadEntries[idx - 1], _fullLoadEntries[idx], deltaArea);
					return;
				}
				area += additionalArea;
			}
			Log.WarnFormat("Could not compute preferred speed, check FullLoadCurve! N95h: {0}, maxArea: {1}", N95hSpeed, maxArea);
		}

		private PerSecond ComputeEngineSpeedForSegmentArea(FullLoadCurveEntry p1, FullLoadCurveEntry p2, double area)
		{
			var k = (p2.TorqueFullLoad - p1.TorqueFullLoad) / (p2.EngineSpeed - p1.EngineSpeed);
			var d = p2.TorqueFullLoad - k * p2.EngineSpeed;

			if (k.IsEqual(0.0)) {
				// rectangle
				// area = M * n
				return (p1.EngineSpeed + (area / d.Value()));
			}

			// non-constant torque, M(n) = k * n + d
			// area = M(n1) * (n2 - n1) + (M(n1) + M(n2))/2 * (n2 - n1) => solve for n2
			var retVal = VectoMath.QuadraticEquationSolver(k.Value() / 2.0, d.Value(),
				(k * p1.EngineSpeed * p1.EngineSpeed + 2 * p1.EngineSpeed * d).Value());
			if (retVal.Count == 0) {
				Log.InfoFormat("No real solution found for requested area: P: {0}, p1: {1}, p2: {2}", area, p1, p2);
			}
			return retVal.First(x => x >= p1.EngineSpeed.Value() && x <= p2.EngineSpeed.Value()).SI<PerSecond>();
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

		private List<PerSecond> FindEngineSpeedForPower(Watt power)
		{
			var retVal = new List<PerSecond>();
			for (var idx = 1; idx < _fullLoadEntries.Count; idx++) {
				var solutions = FindEngineSpeedForPower(_fullLoadEntries[idx - 1], _fullLoadEntries[idx], power);
				retVal.AddRange(solutions);
			}
			retVal.Sort();
			return retVal;
		}

		private List<PerSecond> FindEngineSpeedForPower(FullLoadCurveEntry p1, FullLoadCurveEntry p2, Watt power)
		{
			var k = (p2.TorqueFullLoad - p1.TorqueFullLoad) / (p2.EngineSpeed - p1.EngineSpeed);
			var d = p2.TorqueFullLoad - k * p2.EngineSpeed;

			var retVal = new List<PerSecond>();
			if (k.IsEqual(0, 0.0001)) {
				// constant torque, solve linear equation
				// power = M * n
				retVal.Add((power.Value() / d.Value()).SI<PerSecond>());
			} else {
				// non-constant torque, solve quadratic equation for engine speed (n)
				// power = M(n) * n = (k * n + d) * n =  k * n^2 + d * n
				retVal = VectoMath.QuadraticEquationSolver(k.Value(), d.Value(), -power.Value()).SI<PerSecond>().ToList();
				if (retVal.Count == 0) {
					Log.InfoFormat("No real solution found for requested power demand: P: {0}, p1: {1}, p2: {2}", power, p1, p2);
				}
			}
			retVal = retVal.Where(x => x >= p1.EngineSpeed && x <= p2.EngineSpeed).ToList();
			return retVal;
		}

		private double ComputeArea(PerSecond lowEngineSpeed, PerSecond highEngineSpeed)
		{
			var startSegment = FindIndex(lowEngineSpeed);
			var endSegment = FindIndex(highEngineSpeed);

			var area = 0.0;
			if (lowEngineSpeed < _fullLoadEntries[startSegment].EngineSpeed) {
				// add part of the first segment
				area += ((_fullLoadEntries[startSegment].EngineSpeed - lowEngineSpeed) *
						(FullLoadStationaryTorque(lowEngineSpeed) + _fullLoadEntries[startSegment].TorqueFullLoad) / 2.0).Value();
			}
			for (var i = startSegment + 1; i <= endSegment; i++) {
				var speedHigh = _fullLoadEntries[i].EngineSpeed;
				var torqueHigh = _fullLoadEntries[i].TorqueFullLoad;
				if (highEngineSpeed < _fullLoadEntries[i].EngineSpeed) {
					// add part of the last segment
					speedHigh = highEngineSpeed;
					torqueHigh = FullLoadStationaryTorque(highEngineSpeed);
				}
				area += ((speedHigh - _fullLoadEntries[i - 1].EngineSpeed) *
						(torqueHigh + _fullLoadEntries[i - 1].TorqueFullLoad) / 2.0).Value();
			}
			return area;
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

		#region Equality members

		protected bool Equals(EngineFullLoadCurve other)
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
			return Equals((EngineFullLoadCurve)obj);
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