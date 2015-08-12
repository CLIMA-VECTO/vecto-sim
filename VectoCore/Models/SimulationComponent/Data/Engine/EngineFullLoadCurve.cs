using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
	/// <summary>
	/// Represents the Full load curve.
	/// </summary>
	public class EngineFullLoadCurve : FullLoadCurve
	{
		private Watt _maxPower;

		private PerSecond _ratedSpeed;
		private PerSecond _preferredSpeed;
		private PerSecond _engineSpeedLo; // 55% of Pmax
		private PerSecond _engineSpeedHi; // 70% of Pmax
		private PerSecond _n95hSpeed; // 95% of Pmax

		public new static EngineFullLoadCurve ReadFromFile(string fileName, bool declarationMode = false)
		{
			var curve = FullLoadCurve.ReadFromFile(fileName, declarationMode);
			return new EngineFullLoadCurve { FullLoadEntries = curve.FullLoadEntries, PT1Data = curve.PT1Data };
		}


		public CombustionEngineData EngineData { get; internal set; }


		public NewtonMeter FullLoadStationaryTorque(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate(FullLoadEntries[idx - 1].EngineSpeed, FullLoadEntries[idx].EngineSpeed,
				FullLoadEntries[idx - 1].TorqueFullLoad, FullLoadEntries[idx].TorqueFullLoad,
				angularVelocity);
		}

		public Watt FullLoadStationaryPower(PerSecond angularVelocity)
		{
			return Formulas.TorqueToPower(FullLoadStationaryTorque(angularVelocity), angularVelocity);
		}

		public NewtonMeter DragLoadStationaryTorque(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate(FullLoadEntries[idx - 1].EngineSpeed, FullLoadEntries[idx].EngineSpeed,
				FullLoadEntries[idx - 1].TorqueDrag, FullLoadEntries[idx].TorqueDrag,
				angularVelocity);
		}

		public Watt DragLoadStationaryPower(PerSecond angularVelocity)
		{
			Contract.Requires(angularVelocity.HasEqualUnit(new SI().Radian.Per.Second));
			Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

			return Formulas.TorqueToPower(DragLoadStationaryTorque(angularVelocity), angularVelocity);
		}

		public Second PT1(PerSecond angularVelocity)
		{
			return PT1Data.Lookup(angularVelocity);
		}


		/// <summary>
		/// Get the engine's rated speed from the given full-load curve (i.e. engine speed with max. power)
		/// </summary>
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
		///	Get the engine's preferred speed from the given full-load curve (i.e. Speed at 51% torque/speed-integral between idling and N95h.)
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
			get { return FullLoadEntries.Max(x => x.TorqueFullLoad); }
		}

		public NewtonMeter MaxDragTorque
		{
			get { return FullLoadEntries.Min(x => x.TorqueDrag); }
		}

		/// <summary>
		///	Compute the engine's rated speed from the given full-load curve (i.e. engine speed with max. power)
		/// </summary>
		private void ComputeRatedSpeed()
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
		}


		private void ComputePreferredSpeed()
		{
			var maxArea = ComputeArea(EngineData.IdleSpeed, N95hSpeed);

			var area = 0.SI<Watt>();
			var idx = 0;
			while (++idx < FullLoadEntries.Count) {
				var additionalArea = ComputeArea(FullLoadEntries[idx - 1].EngineSpeed, FullLoadEntries[idx].EngineSpeed);
				if (area + additionalArea > 0.51 * maxArea) {
					var deltaArea = 0.51 * maxArea - area;
					_preferredSpeed = ComputeEngineSpeedForSegmentArea(FullLoadEntries[idx - 1], FullLoadEntries[idx], deltaArea);
					return;
				}
				area += additionalArea;
			}
			Log.WarnFormat("Could not compute preferred speed, check FullLoadCurve! N95h: {0}, maxArea: {1}", N95hSpeed, maxArea);
		}

		private PerSecond ComputeEngineSpeedForSegmentArea(FullLoadCurveEntry p1, FullLoadCurveEntry p2, Watt area)
		{
			var k = (p2.TorqueFullLoad - p1.TorqueFullLoad) / (p2.EngineSpeed - p1.EngineSpeed);
			var d = p2.TorqueFullLoad - k * p2.EngineSpeed;

			if (k.IsEqual(0.0)) {
				// rectangle
				// area = M * n
				return p1.EngineSpeed + area / d;
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
			if (angularVelocity < FullLoadEntries[0].EngineSpeed) {
				Log.ErrorFormat("requested rpm below minimum rpm in FLD curve - extrapolating. n: {0}, rpm_min: {1}",
					angularVelocity.ConvertTo().Rounds.Per.Minute, FullLoadEntries[0].EngineSpeed.ConvertTo().Rounds.Per.Minute);
				idx = 1;
			} else {
				idx = FullLoadEntries.FindIndex(x => x.EngineSpeed > angularVelocity);
			}
			if (idx <= 0) {
				idx = angularVelocity > FullLoadEntries[0].EngineSpeed ? FullLoadEntries.Count - 1 : 1;
			}
			return idx;
		}

		private List<PerSecond> FindEngineSpeedForPower(Watt power)
		{
			var retVal = new List<PerSecond>();
			for (var idx = 1; idx < FullLoadEntries.Count; idx++) {
				var solutions = FindEngineSpeedForPower(FullLoadEntries[idx - 1], FullLoadEntries[idx], power);
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

		private Watt ComputeArea(PerSecond lowEngineSpeed, PerSecond highEngineSpeed)
		{
			var startSegment = FindIndex(lowEngineSpeed);
			var endSegment = FindIndex(highEngineSpeed);

			var area = 0.SI<Watt>();
			if (lowEngineSpeed < FullLoadEntries[startSegment].EngineSpeed) {
				// add part of the first segment
				area += (FullLoadEntries[startSegment].EngineSpeed - lowEngineSpeed) *
						(FullLoadStationaryTorque(lowEngineSpeed) + FullLoadEntries[startSegment].TorqueFullLoad) / 2.0;
			}
			for (var i = startSegment + 1; i <= endSegment; i++) {
				var speedHigh = FullLoadEntries[i].EngineSpeed;
				var torqueHigh = FullLoadEntries[i].TorqueFullLoad;
				if (highEngineSpeed < FullLoadEntries[i].EngineSpeed) {
					// add part of the last segment
					speedHigh = highEngineSpeed;
					torqueHigh = FullLoadStationaryTorque(highEngineSpeed);
				}
				area += (speedHigh - FullLoadEntries[i - 1].EngineSpeed) * (torqueHigh + FullLoadEntries[i - 1].TorqueFullLoad) /
						2.0;
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

		#region Equality members

		protected bool Equals(EngineFullLoadCurve other)
		{
			return Equals(FullLoadEntries, other.FullLoadEntries) && Equals(PT1Data, other.PT1Data);
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
				return ((FullLoadEntries != null ? FullLoadEntries.GetHashCode() : 0) * 397) ^
						(PT1Data != null ? PT1Data.GetHashCode() : 0);
			}
		}

		#endregion
	}
}