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
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	///     Class representing one Distance Based Driving Cycle
	/// </summary>
	public class DistanceBasedDrivingCycle : VectoSimulationComponent, IDrivingCycle,
		ISimulationOutPort, IDrivingCycleInPort, IRoadLookAhead
	{
		protected DrivingCycleData Data;

		internal DrivingCycleState PreviousState = null;
		internal DrivingCycleState CurrentState = new DrivingCycleState();

		internal readonly DrivingCycleEnumerator CycleIntervalIterator;

		private IDrivingCycleOutPort _outPort;

		public DistanceBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
			CycleIntervalIterator = new DrivingCycleEnumerator(Data);
			CycleIntervalIterator.MoveNext();
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

		IResponse ISimulationOutPort.Request(Second absTime, Meter ds)
		{
			var retVal = DoHandleRequest(absTime, ds);

			CurrentState.Response = retVal;

			//switch (retVal.ResponseType) {}
			return retVal;
		}

		private IResponse DoHandleRequest(Second absTime, Meter ds)
		{
			//var currentCycleEntry = Data.Entries[_previousState.CycleIndex];
			//var nextCycleEntry = Data.Entries[_previousState.CycleIndex + 1];

			if (CycleIntervalIterator.LeftSample.Distance.IsEqual(PreviousState.Distance.Value())) {
				// exactly on an entry in the cycle...
				if (!CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(0)
					&& CycleIntervalIterator.LeftSample.StoppingTime > PreviousState.WaitTime) {
					// stop for certain time unless we've already waited long enough...
					if (!CycleIntervalIterator.LeftSample.VehicleTargetSpeed.IsEqual(0)) {
						Log.WarnFormat("Stopping Time requested in cycle but target-velocity not zero. distance: {0}, target speed: {1}",
							CycleIntervalIterator.LeftSample.StoppingTime, CycleIntervalIterator.LeftSample.VehicleTargetSpeed);
						throw new VectoSimulationException("Stopping Time only allowed when target speed is zero!");
					}
					var dt = CycleIntervalIterator.LeftSample.StoppingTime.Value() - PreviousState.WaitTime;
					return DriveTimeInterval(absTime, dt);
				}
			}

			if (PreviousState.Distance + ds > CycleIntervalIterator.RightSample.Distance) {
				// only drive until next sample point in cycle
				// only drive until next sample point in cycle
				return new ResponseDrivingCycleDistanceExceeded() {
					MaxDistance = CycleIntervalIterator.RightSample.Distance - PreviousState.Distance
				};
			}


			return DriveDistance(absTime, ds);
		}

		private IResponse DriveTimeInterval(Second absTime, Second dt)
		{
			CurrentState.AbsTime = PreviousState.AbsTime + dt;
			CurrentState.WaitTime = PreviousState.WaitTime + dt;
			CurrentState.Gradient = ComputeGradient();

			return _outPort.Request(absTime, dt, CycleIntervalIterator.LeftSample.VehicleTargetSpeed, CurrentState.Gradient);
		}

		private IResponse DriveDistance(Second absTime, Meter ds)
		{
			CurrentState.Distance = PreviousState.Distance + ds;
			CurrentState.VehicleTargetSpeed = CycleIntervalIterator.LeftSample.VehicleTargetSpeed;
			CurrentState.Gradient = ComputeGradient();

			return _outPort.Request(absTime, ds, CurrentState.VehicleTargetSpeed, CurrentState.Gradient);
		}

		private Radian ComputeGradient()
		{
			var leftSamplePoint = CycleIntervalIterator.LeftSample;
			var rightSamplePoint = CycleIntervalIterator.RightSample;

			var gradient = leftSamplePoint.RoadGradient;

			if (!leftSamplePoint.Distance.IsEqual(rightSamplePoint.Distance)) {
				CurrentState.Altitude = VectoMath.Interpolate(leftSamplePoint.Distance, rightSamplePoint.Distance,
					leftSamplePoint.Altitude, rightSamplePoint.Altitude, CurrentState.Distance);

				gradient = VectoMath.InclinationToAngle(((CurrentState.Altitude - PreviousState.Altitude) /
														(CurrentState.Distance - PreviousState.Distance)).Value());
			}
			return gradient;
		}

		IResponse ISimulationOutPort.Request(Second absTime, Second dt)
		{
			throw new NotImplementedException();
		}

		IResponse ISimulationOutPort.Initialize()
		{
			var first = Data.Entries.First();
			PreviousState = new DrivingCycleState() {
				AbsTime = 0.SI<Second>(),
				WaitTime = 0.SI<Second>(),
				Distance = first.Distance,
				Altitude = first.Altitude,
			};
			CurrentState = PreviousState.Clone();
			//return new ResponseSuccess();
			return _outPort.Initialize(CycleIntervalIterator.LeftSample.VehicleTargetSpeed,
				CycleIntervalIterator.LeftSample.RoadGradient);
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.v_targ] = CurrentState.VehicleTargetSpeed;
			writer[ModalResultField.grad] = Math.Tan(CurrentState.Gradient.Value()) * 100;
		}

		protected override void DoCommitSimulationStep()
		{
			if (CurrentState.Response.ResponseType != ResponseType.Success) {
				throw new VectoSimulationException("Previous request did not succeed!");
			}

			PreviousState = CurrentState;
			CurrentState = CurrentState.Clone();

			if (!CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(0) &&
				CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(CurrentState.WaitTime)) {
				// we needed to stop at the current interval in the cycle and have already waited enough time, move on..
				CycleIntervalIterator.MoveNext();
			}
			if (CurrentState.Distance >= CycleIntervalIterator.RightSample.Distance) {
				// we have reached the end of the current interval in the cycle, move on...
				CycleIntervalIterator.MoveNext();
			}
		}

		#endregion

		public IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Meter distance)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Second time)
		{
			throw new NotImplementedException();
		}

		public class DrivingCycleEnumerator : IEnumerator<DrivingCycleData.DrivingCycleEntry>
		{
			protected IEnumerator<DrivingCycleData.DrivingCycleEntry> LeftSampleIt;
			protected IEnumerator<DrivingCycleData.DrivingCycleEntry> RightSampleIt;

			//protected uint currentCycleIndex;

			public DrivingCycleEnumerator(DrivingCycleData data)
			{
				LeftSampleIt = data.Entries.GetEnumerator();
				RightSampleIt = data.Entries.GetEnumerator();
				RightSampleIt.MoveNext();
				//currentCycleIndex = 0;
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
					// WaitTime is not cloned on purpose!
					WaitTime = 0.SI<Second>(),
					Response = null
				};
			}

			public Second AbsTime;

			public Meter Distance;

			public Second WaitTime;

			public MeterPerSecond VehicleTargetSpeed;

			public Meter Altitude;

			public Radian Gradient;

			public IResponse Response;
		}
	}
}