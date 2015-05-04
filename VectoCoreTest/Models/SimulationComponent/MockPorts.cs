using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	public class MockTnOutPort : ITnOutPort
	{
		public TimeSpan AbsTime { get; set; }
		public TimeSpan Dt { get; set; }
		public NewtonMeter Torque { get; set; }
		public PerSecond AngularVelocity { get; set; }

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond angularVelocity)
		{
			AbsTime = absTime;
			Dt = dt;
			Torque = torque;
			AngularVelocity = angularVelocity;
			LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, dt: {1}, torque: {3}, engineSpeed: {4}",
				absTime, dt, torque, angularVelocity);
			return new ResponseSuccess();
		}
	}

	public class MockDrivingCycleDemandOutPort : IDrivingCycleDemandOutPort
	{
		public TimeSpan AbsTime { get; set; }
		public TimeSpan Dt { get; set; }
		public MeterPerSecond Velocity { get; set; }
		public Radian Gradient { get; set; }

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, Radian gradient)
		{
			AbsTime = absTime;
			Dt = dt;
			Velocity = velocity;
			Gradient = gradient;
			LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, dt: {1}, velocity: {2}, gradient: {3}",
				absTime, dt, velocity, gradient);
			return new ResponseSuccess();
		}
	}

	public class MockFvOutPort : IFvOutPort
	{
		public TimeSpan AbsTime { get; set; }
		public TimeSpan Dt { get; set; }
		public Newton Force { get; set; }
		public MeterPerSecond Velocity { get; set; }


		public IResponse Request(TimeSpan absTime, TimeSpan dt, Newton force, MeterPerSecond velocity)
		{
			AbsTime = absTime;
			Dt = dt;
			Force = force;
			Velocity = velocity;
			LogManager.GetLogger(GetType())
				.DebugFormat("Request: abstime: {0}, dt: {1}, force: {2}, velocity: {3}", absTime, dt, force, velocity);
			return new ResponseSuccess();
		}
	}
}