using System;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	public static class Formulas
	{
		/// <summary>
		/// Calculates the power from torque and angular velocity.
		/// </summary>
		public static Watt TorqueToPower(NewtonMeter torque, PerSecond angularVelocity)
		{
			return torque * angularVelocity;
		}

		/// <summary>
		/// Calculates the torque from power and angular velocity.
		/// </summary>
		public static NewtonMeter PowerToTorque(Watt power, PerSecond angularVelocity)
		{
			if (Math.Abs(angularVelocity.Value()) < 1E-10) {
				throw new VectoException("Can not compute Torque for 0 angular Velocity!");
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

		/// <summary>
		/// Calculates power loss caused by inertia.
		/// https://en.wikipedia.org/wiki/Angular_acceleration
		/// alpha = delta_omega / dt
		/// torque = I * alpha
		/// </summary>
		public static Watt InertiaPower(PerSecond currentOmega, PerSecond previousOmega, KilogramSquareMeter inertia,
			Second dt)
		{
			var deltaOmega = previousOmega - currentOmega;
			var avgOmega = (currentOmega + previousOmega) / 2;

			var alpha = deltaOmega / dt;
			var torque = inertia * alpha;
			return (torque * avgOmega).Cast<Watt>();
		}
	}
}