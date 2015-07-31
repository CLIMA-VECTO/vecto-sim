using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockTnOutPort : ITnOutPort
	{
		public Second AbsTime { get; set; }
		public Second Dt { get; set; }
		public NewtonMeter Torque { get; set; }
		public PerSecond AngularVelocity { get; set; }

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity)
		{
			AbsTime = absTime;
			Dt = dt;
			Torque = torque;
			AngularVelocity = angularVelocity;
			LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, dt: {1}, torque: {3}, engineSpeed: {4}",
				absTime, dt, torque, angularVelocity);
			return new ResponseSuccess();
		}

		public IResponse Initialize()
		{
			throw new NotImplementedException();
		}
	}

	public class MockDrivingCycleOutPort : IDrivingCycleOutPort
	{
		public Second AbsTime { get; set; }
		public Meter Ds { get; set; }

		public Second Dt { get; set; }
		public MeterPerSecond Velocity { get; set; }
		public Radian Gradient { get; set; }

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			AbsTime = absTime;
			Ds = ds;
			Velocity = targetVelocity;
			Gradient = gradient;
			LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, ds: {1}, velocity: {2}, gradient: {3}",
				absTime, ds, targetVelocity, gradient);
			return new ResponseSuccess();
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			AbsTime = absTime;
			Dt = dt;
			Velocity = targetVelocity;
			Gradient = gradient;
			LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, ds: {1}, velocity: {2}, gradient: {3}",
				absTime, dt, targetVelocity, gradient);
			return new ResponseSuccess();
		}

		public IResponse Initialize()
		{
			throw new NotImplementedException();
		}
	}

	public class MockFvOutPort : IFvOutPort
	{
		public Second AbsTime { get; set; }
		public Second Dt { get; set; }
		public Newton Force { get; set; }
		public MeterPerSecond Velocity { get; set; }


		public IResponse Request(Second absTime, Second dt, Newton force, MeterPerSecond velocity)
		{
			AbsTime = absTime;
			Dt = dt;
			Force = force;
			Velocity = velocity;
			LogManager.GetLogger(GetType())
				.DebugFormat("Request: abstime: {0}, dt: {1}, force: {2}, velocity: {3}", absTime, dt, force, velocity);
			return new ResponseSuccess();
		}

		public IResponse Initialize()
		{
			throw new NotImplementedException();
		}
	}
}