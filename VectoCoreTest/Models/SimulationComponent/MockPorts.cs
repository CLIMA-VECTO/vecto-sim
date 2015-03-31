using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
    public class MockTnOutPort : ITnOutPort
    {
        public TimeSpan AbsTime { get; set; }
        public TimeSpan Dt { get; set; }
        public NewtonMeter Torque { get; set; }
        public RadianPerSecond AngularFrequency { get; set; }

        public void Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond angularFrequency)
        {
            AbsTime = absTime;
            Dt = dt;
            Torque = torque;
            AngularFrequency = angularFrequency;
            LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, dt: {1}, torque: {3}, engineSpeed: {4}",
                                                        absTime, dt, torque, angularFrequency);
        }
    }

    public class MockDriverDemandOutPort : IDriverDemandOutPort
    {
        public TimeSpan AbsTime { get; set; }

        public TimeSpan Dt { get; set; }
        public MeterPerSecond Velocity { get; set; }
        public double Gradient { get; set; }
        public void Request(TimeSpan absTime, TimeSpan dt, MeterPerSecond velocity, double gradient)
        {
            AbsTime = absTime;
            Dt = dt;
            Velocity = velocity;
            Gradient = gradient;
            LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, dt: {1}, velocity: {3}, gradient: {4}",
                                                        absTime, dt, velocity, gradient);
        }
    }
}