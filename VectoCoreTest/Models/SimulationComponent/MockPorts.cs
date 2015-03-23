using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
    public class MockTnOutPort : ITnOutPort
    {
        public TimeSpan AbsTime { get; set; }
        public TimeSpan Dt { get; set; }
        public double Torque { get; set; }
        public double EngineSpeed { get; set; }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            AbsTime = absTime;
            Dt = dt;
            Torque = torque;
            EngineSpeed = engineSpeed;
            LogManager.GetLogger(GetType()).DebugFormat("Request: absTime: {0}, dt: {1}, torque: {3}, engineSpeed: {4}",
                                                        absTime, dt, torque, engineSpeed);
        }
    }

    public class MockDriverDemandOutPort : IDriverDemandOutPort
    {
        public TimeSpan AbsTime { get; set; }

        public TimeSpan Dt { get; set; }
        public double Velocity { get; set; }
        public double Gradient { get; set; }
        public void Request(TimeSpan absTime, TimeSpan dt, double velocity, double gradient)
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