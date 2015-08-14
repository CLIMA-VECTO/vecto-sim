using System;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	public static class Formulas
	{
		/// <summary>
		///     [Nm], [rad/s] => [W]. Calculates the power from torque and angular velocity.
		/// </summary>
		/// <param name="torque">[Nm]</param>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>power [W]</returns>
		public static Watt TorqueToPower(NewtonMeter torque, PerSecond angularVelocity)
		{
			return torque * angularVelocity;
		}

		/// <summary>
		///     [W], [rad/s] => [Nm]. Calculates the torque from power and angular velocity.
		/// </summary>
		/// <param name="power">[W]</param>
		/// <param name="angularVelocity">[rad/s]</param>
		/// <returns>torque [Nm]</returns>
		public static NewtonMeter PowerToTorque(Watt power, PerSecond angularVelocity)
		{
			if (Math.Abs(angularVelocity.Value()) < 1E-10) {
				throw new VectoSimulationException("Can not compute Torque for 0 angular Velocity!");
			}
			return power / angularVelocity;
		}

		public static Meter DecelerationDistance(MeterPerSecond v1, MeterPerSecond v2,
			MeterPerSquareSecond deceleration)
		{
			if (deceleration >= 0) {
				throw new VectoException("Deceleration must be negative!");
			}
			if (v2 > v1) {
				throw new VectoException("v2 must not be greater than v1");
			}

			return ((v2 - v1) * (v1 + v2) / deceleration / 2.0).Cast<Meter>();
		}
	}
}