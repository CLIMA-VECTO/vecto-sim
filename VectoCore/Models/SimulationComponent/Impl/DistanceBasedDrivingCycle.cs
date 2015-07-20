using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one Distance Based Driving Cycle
	/// </summary>
	public class DistanceBasedDrivingCycle : VectoSimulationComponent, IDrivingCycle,
		ISimulationOutPort,
		IDrivingCycleInPort
	{
		protected DrivingCycleData Data;

		private DrivingCycleState _previousState = null;
		private DrivingCycleState _currentState = new DrivingCycleState();

		private readonly DrivingCycleEnumerator _cycleIntervalIterator;

		private IDrivingCycleOutPort _outPort;

		public DistanceBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
			_cycleIntervalIterator = new DrivingCycleEnumerator(Data);
			_cycleIntervalIterator.MoveNext();
		}

		#region IDrivingCycleInProvider

		public IDrivingCycleInPort InPort()
		{
			return this;
		}

		#endregion

		#region ISimulationOutProvider

		public ISimulationOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IDrivingCycleInPort

		void IDrivingCycleInPort.Connect(IDrivingCycleOutPort other)
		{
			_outPort = other;
		}

		#endregion

		#region ISimulationOutPort

		IResponse ISimulationOutPort.Request(TimeSpan absTime, Meter ds)
		{
			var retVal = DoHandleRequest(absTime, ds);

			//var success = retVal as ResponseSuccess;
			//switch (retVal.ResponseType) {}
			return retVal;
		}

		private IResponse DoHandleRequest(TimeSpan absTime, Meter ds)
		{
			//var currentCycleEntry = Data.Entries[_previousState.CycleIndex];
			//var nextCycleEntry = Data.Entries[_previousState.CycleIndex + 1];

			if (_cycleIntervalIterator.LeftSample.Distance.IsEqual(_previousState.Distance.Value())) {
				// exactly on an entry in the cycle...
				if (!_cycleIntervalIterator.LeftSample.StoppingTime.IsEqual(0)
					&& _cycleIntervalIterator.LeftSample.StoppingTime.Value() > _previousState.WaitTime.TotalSeconds) {
					// stop for certain time unless we've already waited long enough...
					if (!_cycleIntervalIterator.LeftSample.VehicleTargetSpeed.IsEqual(0)) {
						Log.WarnFormat("Stopping Time requested in cycle but target-velocity not zero. distance: {0}, target speed: {1}",
							_cycleIntervalIterator.LeftSample.StoppingTime, _cycleIntervalIterator.LeftSample.VehicleTargetSpeed);
						throw new VectoSimulationException("Stopping Time only allowed when target speed is zero!");
					}
					var dt = TimeSpan.FromSeconds(_cycleIntervalIterator.LeftSample.StoppingTime.Value()) -
							_previousState.WaitTime;
					return DriveTimeInterval(absTime, dt);
				}
			}

			if (_previousState.Distance + ds > _cycleIntervalIterator.RightSample.Distance) {
				// only drive until next sample point in cycle
				// only drive until next sample point in cycle
				return new ResponseDrivingCycleDistanceExceeded() {
					MaxDistance = _cycleIntervalIterator.RightSample.Distance - _previousState.Distance
				};
			}


			return DriveDistance(absTime, ds);
		}

		private IResponse DriveTimeInterval(TimeSpan absTime, TimeSpan dt)
		{
			_currentState.AbsTime += dt;
			_currentState.WaitTime = _previousState.WaitTime + dt;

			return _outPort.Request(absTime, dt,
				_cycleIntervalIterator.LeftSample.VehicleTargetSpeed, ComputeGradient());
		}

		private IResponse DriveDistance(TimeSpan absTime, Meter ds)
		{
			_currentState.Distance += ds;
			while (_currentState.Distance >= _cycleIntervalIterator.RightSample.Distance) {
				//Data.Entries[_currentState.CycleIndex + 1].Distance) {
				_cycleIntervalIterator.MoveNext();
			}

			_currentState.VehicleTargetSpeed = _cycleIntervalIterator.LeftSample.VehicleTargetSpeed;

			return _outPort.Request(absTime, ds, _currentState.VehicleTargetSpeed, ComputeGradient());
		}

		private Radian ComputeGradient()
		{
			var leftSamplePoint = _cycleIntervalIterator.LeftSample;
			var rightSamplePoint = _cycleIntervalIterator.RightSample;

			var gradient = leftSamplePoint.RoadGradient;

			if (!leftSamplePoint.Distance.IsEqual(rightSamplePoint.Distance)) {
				_currentState.Altitude = VectoMath.Interpolate(leftSamplePoint.Distance, rightSamplePoint.Distance,
					leftSamplePoint.Altitude, rightSamplePoint.Altitude, _currentState.Distance);

				gradient = VectoMath.InclinationToAngle(((_currentState.Altitude - _previousState.Altitude) /
															(_currentState.Distance - _previousState.Distance)).Value());
			}
			return gradient;
		}

		IResponse ISimulationOutPort.Request(TimeSpan absTime, TimeSpan dt)
		{
			throw new NotImplementedException();
		}

		IResponse ISimulationOutPort.Initialize()
		{
			var first = Data.Entries.First();
			_previousState = new DrivingCycleState() {
				AbsTime = TimeSpan.FromSeconds(0),
				WaitTime = TimeSpan.FromSeconds(0),
				Distance = first.Distance,
				Altitude = first.Altitude,
				//CycleIndex = 0,
				//CycleInterval = new DrivingCycleEnumerator(Data)
			};
			_currentState = _previousState.Clone();
			return new ResponseSuccess();
			//TODO: return _outPort.Initialize();
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer) {}

		protected override void DoCommitSimulationStep()
		{
			_previousState = _currentState;
			_currentState = _currentState.Clone();
			_cycleIntervalIterator.MoveNext();
		}

		#endregion

		protected void LookupCycle(Meter ds) {}


		public class DrivingCycleEnumerator : IEnumerator<DrivingCycleData.DrivingCycleEntry>
		{
			protected IEnumerator<DrivingCycleData.DrivingCycleEntry> LeftSampleIt;
			protected IEnumerator<DrivingCycleData.DrivingCycleEntry> RightSampleIt;

			public DrivingCycleEnumerator(DrivingCycleData data)
			{
				LeftSampleIt = data.Entries.GetEnumerator();
				RightSampleIt = data.Entries.GetEnumerator();
				RightSampleIt.MoveNext();
			}

			public DrivingCycleData.DrivingCycleEntry Current
			{
				get { return LeftSampleIt.Current; }
			}

			public DrivingCycleData.DrivingCycleEntry Next
			{
				get { return RightSampleIt.Current; }
			}

			public DrivingCycleData.DrivingCycleEntry LeftSample
			{
				get { return LeftSampleIt.Current; }
			}

			public DrivingCycleData.DrivingCycleEntry RightSample
			{
				get { return RightSampleIt.Current; }
			}

			public void Dispose()
			{
				LeftSampleIt.Dispose();
				RightSampleIt.Dispose();
			}

			object System.Collections.IEnumerator.Current
			{
				get { return LeftSampleIt.Current; }
			}

			public bool MoveNext()
			{
				return LeftSampleIt.MoveNext() && RightSampleIt.MoveNext();
			}

			public void Reset()
			{
				LeftSampleIt.Reset();
				RightSampleIt.Reset();
				RightSampleIt.MoveNext();
			}
		}

		public class DrivingCycleState
		{
			public DrivingCycleState() {}

			public DrivingCycleState Clone()
			{
				return new DrivingCycleState() {
					AbsTime = AbsTime,
					Distance = Distance,
					VehicleTargetSpeed = VehicleTargetSpeed,
					Altitude = Altitude,
					//CycleIndex = CycleIndex,
					//CycleInterval = CycleInterval,
					// WaitTime is not cloned on purpose!
					WaitTime = TimeSpan.FromSeconds(0)
				};
			}

			public TimeSpan AbsTime;

			public Meter Distance;

			public TimeSpan WaitTime;

			public MeterPerSecond VehicleTargetSpeed;

			public Meter Altitude;

			//public int CycleIndex;
		}
	}
}