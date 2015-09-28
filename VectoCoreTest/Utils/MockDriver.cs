using System;
using System.Diagnostics;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockDriver : VectoSimulationComponent, IDriver, IDrivingCycleOutPort, IDriverDemandInPort, IDriverInfo
	{
		private IDriverDemandOutPort _next;

		public RequestData LastRequest;

		public MockDriver(IVehicleContainer container) : base(container) {}

		protected override void DoWriteModalResults(IModalDataWriter writer) {}

		protected override void DoCommitSimulationStep() {}


		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		public IDriverDemandInPort InPort()
		{
			return this;
		}

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			LastRequest = new RequestData { AbsTime = absTime, ds = ds, Gradient = gradient, TargetVelocity = targetVelocity };
			var acc = 0.SI<MeterPerSquareSecond>();
			var dt = 1.SI<Second>();
			return new ResponseSuccess { SimulationInterval = dt, Source = this };
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			LastRequest = new RequestData { AbsTime = absTime, dt = dt, Gradient = gradient, TargetVelocity = targetVelocity };
			var acc = 0.SI<MeterPerSquareSecond>();
			return new ResponseSuccess { SimulationInterval = dt, Source = this };
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			if (_next != null) {
				return _next.Initialize(vehicleSpeed, roadGradient);
			}

			return new ResponseSuccess { Source = this };
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient, MeterPerSquareSecond startAcceleration)
		{
			if (_next != null) {
				return _next.Initialize(vehicleSpeed, startAcceleration, roadGradient);
			}

			return new ResponseSuccess { Source = this };
		}

		public void Connect(IDriverDemandOutPort other)
		{
			_next = other;
		}

		public class RequestData
		{
			public Second AbsTime;
			public Meter ds;
			public Second dt;
			public MeterPerSecond TargetVelocity;
			public Radian Gradient;
		}

		public bool VehicleStopped { get; set; }
	}
}