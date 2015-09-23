using System;
using System.Diagnostics;
using NLog;
using TUGraz.VectoCore.Models;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockTnOutPort : ITnOutPort, IEngineInfo
	{
		protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public Second AbsTime;
		public Second Dt;
		public NewtonMeter Torque;
		public PerSecond AngularVelocity;

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			AbsTime = absTime;
			Dt = dt;
			Torque = torque;
			AngularVelocity = angularVelocity;
			Log.Debug("Request: absTime: {0}, dt: {1}, torque: {3}, engineSpeed: {4}", absTime, dt, torque, angularVelocity);

			if (dryRun) {
				return new ResponseDryRun {
					Source = this,
					GearboxPowerRequest = torque * angularVelocity,
					EnginePowerRequest = torque * angularVelocity,
					DeltaFullLoad = (torque - 2300.SI<NewtonMeter>()) * angularVelocity,
					DeltaDragLoad = (torque - -100.SI<NewtonMeter>()) * angularVelocity
				};
			}

			return new ResponseSuccess {
				Source = this,
				GearboxPowerRequest = torque * angularVelocity,
				EnginePowerRequest = torque * angularVelocity,
			};
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			return new ResponseSuccess { Source = this, EnginePowerRequest = torque * angularVelocity, };
		}

		public void DoCommitSimulationStep()
		{
			AbsTime = null;
			Dt = null;
			Torque = null;
			AngularVelocity = null;
		}

		public PerSecond EngineSpeed
		{
			get { return AngularVelocity; }
		}

		public Watt EngineStationaryFullPower(PerSecond angularSpeed)
		{
			return 2300.SI<NewtonMeter>() * angularSpeed;
		}

		public PerSecond EngineIdleSpeed
		{
			get { return 560.RPMtoRad(); }
		}
	}

	public class MockDrivingCycleOutPort : LoggingObject, IDrivingCycleOutPort
	{
		public Second AbsTime;
		public Meter Ds;
		public Second Dt;
		public MeterPerSecond Velocity;
		public Radian Gradient;

		public IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient)
		{
			AbsTime = absTime;
			Ds = ds;
			Velocity = targetVelocity;
			Gradient = gradient;
			Log.Debug("Request: absTime: {0}, ds: {1}, velocity: {2}, gradient: {3}", absTime, ds, targetVelocity, gradient);
			return new ResponseSuccess();
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient)
		{
			AbsTime = absTime;
			Dt = dt;
			Velocity = targetVelocity;
			Gradient = gradient;
			Log.Debug("Request: absTime: {0}, ds: {1}, velocity: {2}, gradient: {3}", absTime, dt, targetVelocity, gradient);
			return new ResponseSuccess();
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			throw new NotImplementedException();
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient, MeterPerSquareSecond startAcceleration)
		{
			throw new NotImplementedException();
		}
	}

	public class MockFvOutPort : LoggingObject, IFvOutPort
	{
		public Second AbsTime { get; set; }
		public Second Dt { get; set; }
		public Newton Force { get; set; }
		public MeterPerSecond Velocity { get; set; }


		public IResponse Request(Second absTime, Second dt, Newton force, MeterPerSecond velocity, bool dryRun = false)
		{
			AbsTime = absTime;
			Dt = dt;
			Force = force;
			Velocity = velocity;
			Log.Debug("Request: abstime: {0}, dt: {1}, force: {2}, velocity: {3}", absTime, dt, force, velocity);
			return new ResponseSuccess();
		}

		public IResponse Initialize(Newton vehicleForce, MeterPerSecond vehicleSpeed)
		{
			return new ResponseSuccess();
		}
	}
}