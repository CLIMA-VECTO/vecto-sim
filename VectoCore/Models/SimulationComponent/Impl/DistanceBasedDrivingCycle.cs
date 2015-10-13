using System;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.Configuration;
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
		protected const double LookaheadTimeSafetyMargin = 1.5;
		protected DrivingCycleData Data;

		internal DrivingCycleState PreviousState = null;
		internal DrivingCycleState CurrentState = new DrivingCycleState();

		internal readonly DrivingCycleEnumerator CycleIntervalIterator;

		protected IDrivingCycleOutPort NextComponent;

		public DistanceBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
			CycleIntervalIterator = new DrivingCycleEnumerator(Data);
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
			NextComponent = other;
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

		/// <summary>
		/// Does the handle request.
		/// </summary>
		/// <param name="absTime">The abs time.</param>
		/// <param name="ds">The ds.</param>
		/// <returns></returns>
		/// <exception cref="VectoSimulationException">Stopping Time only allowed when target speed is zero!</exception>
		private IResponse DoHandleRequest(Second absTime, Meter ds)
		{
			if (CycleIntervalIterator.LeftSample.Distance.IsEqual(PreviousState.Distance.Value())) {
				// exactly on an entry in the cycle...
				if (!CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(0)
					&& CycleIntervalIterator.LeftSample.StoppingTime > PreviousState.WaitTime) {
					// stop for certain time unless we've already waited long enough...
					if (!CycleIntervalIterator.LeftSample.VehicleTargetSpeed.IsEqual(0)) {
						Log.Warn("Stopping Time requested in cycle but target-velocity not zero. distance: {0}, target speed: {1}",
							CycleIntervalIterator.LeftSample.StoppingTime, CycleIntervalIterator.LeftSample.VehicleTargetSpeed);
						throw new VectoSimulationException("Stopping Time only allowed when target speed is zero!");
					}
					var dt = CycleIntervalIterator.LeftSample.StoppingTime - PreviousState.WaitTime;
					if (CycleIntervalIterator.LeftSample.StoppingTime.IsGreater(3 * Constants.SimulationSettings.TargetTimeInterval)) {
						// split into 3 parts
						if (PreviousState.WaitTime.IsEqual(0)) {
							dt = Constants.SimulationSettings.TargetTimeInterval;
						} else {
							if (dt > Constants.SimulationSettings.TargetTimeInterval) {
								dt -= Constants.SimulationSettings.TargetTimeInterval;
							}
						}
					}
					return DriveTimeInterval(absTime, dt);
				}
			}
			if (CycleIntervalIterator.LastEntry && PreviousState.Distance.IsEqual(CycleIntervalIterator.RightSample.Distance)) {
				return new ResponseCycleFinished();
			}

			if ((PreviousState.Distance + ds).IsGreater(CycleIntervalIterator.RightSample.Distance)) {
				// only drive until next sample point in cycle
				Log.Debug("Limiting distance to next sample point {0}",
					CycleIntervalIterator.RightSample.Distance - PreviousState.Distance);
				return new ResponseDrivingCycleDistanceExceeded {
					Source = this,
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

			return NextComponent.Request(absTime, dt, CycleIntervalIterator.LeftSample.VehicleTargetSpeed, CurrentState.Gradient);
		}

		private IResponse DriveDistance(Second absTime, Meter ds)
		{
			if (!CurrentState.RequestToNextSamplePointDone &&
				(CycleIntervalIterator.RightSample.Distance - PreviousState.Distance) <
				Constants.SimulationSettings.BrakeNextTargetDistance) {
				CurrentState.RequestToNextSamplePointDone = true;
				Log.Debug("current distance is close to the next speed change: {0}",
					CycleIntervalIterator.RightSample.Distance - PreviousState.Distance);
				return new ResponseDrivingCycleDistanceExceeded {
					Source = this,
					MaxDistance = Constants.SimulationSettings.BrakeNextTargetDistance
				};
			}
			CurrentState.Distance = PreviousState.Distance + ds;
			CurrentState.VehicleTargetSpeed = CycleIntervalIterator.LeftSample.VehicleTargetSpeed;
			CurrentState.Gradient = ComputeGradient();

			return NextComponent.Request(absTime, ds, CurrentState.VehicleTargetSpeed, CurrentState.Gradient);
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
			//return 0.SI<Radian>();
			return gradient;
		}

		IResponse ISimulationOutPort.Request(Second absTime, Second dt)
		{
			throw new NotImplementedException();
		}

		IResponse ISimulationOutPort.Initialize()
		{
			var first = Data.Entries.First();
			PreviousState = new DrivingCycleState {
				AbsTime = 0.SI<Second>(),
				WaitTime = 0.SI<Second>(),
				Distance = first.Distance,
				Altitude = first.Altitude,
			};
			CurrentState = PreviousState.Clone();

			if (CycleIntervalIterator.LeftSample.VehicleTargetSpeed.IsEqual(0)) {
				var retVal = NextComponent.Initialize(DataBus.StartSpeed,
					CycleIntervalIterator.LeftSample.RoadGradient, DataBus.StartAcceleration);
				if (!(retVal is ResponseSuccess)) {
					throw new UnexpectedResponseException("Couldn't find start gear.", retVal);
				}
			}

			return NextComponent.Initialize(CycleIntervalIterator.LeftSample.VehicleTargetSpeed,
				CycleIntervalIterator.LeftSample.RoadGradient);
		}

		#endregion

		protected IResponse ProcessResponse(IResponse response)
		{
			throw new NotImplementedException();
		}

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.dist] = CurrentState.Distance;
			writer[ModalResultField.v_targ] = CurrentState.VehicleTargetSpeed;
			writer[ModalResultField.grad] = (Math.Tan(CurrentState.Gradient.Value()) * 100).SI<Scalar>();
		}

		protected override void DoCommitSimulationStep()
		{
			if (!(CurrentState.Response is ResponseSuccess)) {
				throw new VectoSimulationException("Previous request did not succeed!");
			}

			PreviousState = CurrentState;
			CurrentState = CurrentState.Clone();

			if (!CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(0) &&
				CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(PreviousState.WaitTime)) {
				// we needed to stop at the current interval in the cycle and have already waited enough time, move on..
				CycleIntervalIterator.MoveNext();
			}

			// separately test for equality and greater than to have tolerance for equality comparison
			if (CycleIntervalIterator.LeftSample.StoppingTime.IsEqual(0) &&
				CurrentState.Distance.IsGreaterOrEqual(CycleIntervalIterator.RightSample.Distance)) {
				// we have reached the end of the current interval in the cycle, move on...
				CycleIntervalIterator.MoveNext();
			}
		}

		#endregion

		public IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Meter lookaheadDistance)
		{
			var retVal = new List<DrivingCycleData.DrivingCycleEntry>();

			var cycleIterator = CycleIntervalIterator.Clone();
			var velocity = cycleIterator.LeftSample.VehicleTargetSpeed;

			do {
				if (cycleIterator.RightSample.VehicleTargetSpeed.IsEqual(velocity)) {
					continue;
				}
				retVal.Add(cycleIterator.RightSample);
				velocity = cycleIterator.RightSample.VehicleTargetSpeed;
			} while (cycleIterator.MoveNext() && cycleIterator.RightSample.Distance < PreviousState.Distance + lookaheadDistance);
			return retVal;
		}

		public IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Second time)
		{
			return LookAhead((LookaheadTimeSafetyMargin * DataBus.VehicleSpeed * time).Cast<Meter>());
		}

		public CycleData CycleData()
		{
			return new CycleData {
				AbsTime = CurrentState.AbsTime,
				AbsDistance = CurrentState.Distance,
				LeftSample = CycleIntervalIterator.LeftSample,
				RightSample = CycleIntervalIterator.RightSample
			};
		}

		public class DrivingCycleEnumerator : IEnumerator<DrivingCycleData.DrivingCycleEntry>
		{
			protected int CurrentCycleIndex;
			protected DrivingCycleData Data;

			public DrivingCycleEnumerator(DrivingCycleData data)
			{
				CurrentCycleIndex = 0;
				Data = data;
				LastEntry = false;
			}

			public DrivingCycleEnumerator Clone()
			{
				return new DrivingCycleEnumerator(Data) {
					CurrentCycleIndex = CurrentCycleIndex,
					LastEntry = LastEntry
				};
			}

			public DrivingCycleData.DrivingCycleEntry Current
			{
				get { return LeftSample; }
			}

			public DrivingCycleData.DrivingCycleEntry Next
			{
				get { return RightSample; }
			}

			public DrivingCycleData.DrivingCycleEntry LeftSample
			{
				get { return Data.Entries[CurrentCycleIndex]; }
			}

			public DrivingCycleData.DrivingCycleEntry RightSample
			{
				get { return CurrentCycleIndex + 1 >= Data.Entries.Count ? null : Data.Entries[CurrentCycleIndex + 1]; }
			}

			public bool LastEntry { get; protected set; }

			public void Dispose() {}

			object System.Collections.IEnumerator.Current
			{
				get { return LeftSample; }
			}

			public bool MoveNext()
			{
				// cycleIndex has to be max. next to last (so that rightSample is still valid.
				if (CurrentCycleIndex >= Data.Entries.Count - 2) {
					LastEntry = true;
					return false;
				}
				CurrentCycleIndex++;
				if (CurrentCycleIndex == Data.Entries.Count - 2) {
					LastEntry = true;
				}
				return true;
			}

			public void Reset()
			{
				CurrentCycleIndex = 0;
			}
		}

		public class DrivingCycleState
		{
			public DrivingCycleState Clone()
			{
				return new DrivingCycleState {
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

			public bool RequestToNextSamplePointDone = false;
		}
	}
}