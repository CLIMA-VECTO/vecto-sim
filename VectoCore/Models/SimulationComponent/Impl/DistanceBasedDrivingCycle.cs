using System;
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

		private IDrivingCycleOutPort _outPort;

		public DistanceBasedDrivingCycle(IVehicleContainer container, DrivingCycleData cycle) : base(container)
		{
			Data = cycle;
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
			var currentCycleEntry = Data.Entries[_currentState.CycleIndex];
			var nextCycleEntry = Data.Entries[_currentState.CycleIndex + 1];

			IResponse retVal = null;
			if (currentCycleEntry.Distance.IsEqual(_previousState.Distance.Value())) {
				// exactly on an entry in the cycle...
				if (!currentCycleEntry.StoppingTime.IsEqual(0)) {
					// stop for certain time...
					if (!currentCycleEntry.VehicleTargetSpeed.IsEqual(0)) {
						Log.WarnFormat("Stopping Time requested in cycle but target-velocity not zero. distance: {0}, target speed: {1}",
							currentCycleEntry.StoppingTime, currentCycleEntry.VehicleTargetSpeed);
						throw new VectoSimulationException("Stopping Time only allowed when target speed is zero!");
					}
					retVal = _outPort.Request(absTime, TimeSpan.FromSeconds(currentCycleEntry.StoppingTime.Value()),
						currentCycleEntry.VehicleTargetSpeed, currentCycleEntry.RoadGradient);
					//retVal = OutPort().Request(absTime, TimeSpan.FromSeconds(currentCycleEntry.StoppingTime.Value()));
				}
			}

			if (_previousState.Distance + ds > nextCycleEntry.Distance) {
				// only drive until next sample point in cycle
				return new ResponseDrivingCycleDistanceExceeded() {
					MaxDistance = nextCycleEntry.Distance - _previousState.Distance
				};
			}


			retVal = DriveDistance(absTime, ds);
			//throw new NotImplementedException("Distance based Cycle is not yet implemented.");
		}

		private IResponse DriveDistance(TimeSpan absTime, Meter ds)
		{
			_currentState = _previousState.Clone();
			_currentState.Distance += ds;
			while (_currentState.Distance < Data.Entries[_currentState.CycleIndex + 1].Distance) {
				_currentState.CycleIndex++;
			}

			var leftSamplePoint = Data.Entries[_currentState.CycleIndex];
			var rightSamplePoint = Data.Entries[_currentState.CycleIndex + 1];
			_currentState.Altitude = VectoMath.Interpolate(leftSamplePoint.Distance, rightSamplePoint.Distance,
				leftSamplePoint.Altitude, rightSamplePoint.Altitude, _currentState.Distance);

			_currentState.VehicleTargetSpeed = Data.Entries[_currentState.CycleIndex].VehicleTargetSpeed;

			var gradient = ((_currentState.Altitude - _previousState.Altitude) /
							(_currentState.Distance - _previousState.Distance)).GradientPercent.Cast<Radian>();


			return _outPort.Request(absTime, ds, _currentState.VehicleTargetSpeed, gradient);
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
				Distance = first.Distance,
				Altitude = first.Altitude,
				CycleIndex = 0,
			};
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
		}

		#endregion

		protected void LookupCycle(Meter ds) {}


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
					CycleIndex = CycleIndex
				};
			}

			public TimeSpan AbsTime;

			public Meter Distance;

			public MeterPerSecond VehicleTargetSpeed;

			public Meter Altitude;

			public int CycleIndex;
		}
	}
}