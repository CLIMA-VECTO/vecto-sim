using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockDriver : VectoSimulationComponent, IDriver, IDrivingCycleOutPort, IDriverDemandInPort
	{
		private IDriverDemandOutPort _next;

		public RequestData LastRequest;

		public MockDriver(IVehicleContainer container) : base(container) {}

		public override void CommitSimulationStep(IModalDataWriter writer) {}


		public IDrivingCycleOutPort OutPort()
		{
			return this;
		}

		public IDriverDemandInPort InPort()
		{
			return this;
		}

		public IResponse Request(TimeSpan absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			LastRequest = new RequestData() { AbsTime = absTime, ds = ds, Gradient = gradient, TargetVelocity = targetVelocity };
			var acc = 0.SI<MeterPerSquareSecond>();
			return new ResponseSuccess(); //_next.Request(absTime, TimeSpan.FromSeconds(0), acc, 0.SI<Radian>());
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			LastRequest = new RequestData() { AbsTime = absTime, dt = dt, Gradient = gradient, TargetVelocity = targetVelocity };
			var acc = 0.SI<MeterPerSquareSecond>();
			return new ResponseSuccess(); //_next.Request(absTime, dt, acc, gradient);
		}

		public void Connect(IDriverDemandOutPort other)
		{
			_next = other;
		}

		public class RequestData
		{
			public TimeSpan AbsTime;
			public Meter ds;
			public TimeSpan dt;
			public MeterPerSecond TargetVelocity;
			public Radian Gradient;
		}
	}
}