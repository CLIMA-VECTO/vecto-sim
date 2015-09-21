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
		/// tau = I * alpha
		/// </summary>
		/// <param name="currentOmega">The current omega (new angularSpeed).</param>
		/// <param name="previousOmega">The previous omega (old angularSpeed).</param>
		/// <param name="inertia">The inertia parameter.</param>
		/// <param name="dt">The dt.</param>
		/// <returns></returns>
		public static Watt InertiaPower(PerSecond currentOmega, PerSecond previousOmega, KilogramSquareMeter inertia,
			Second dt)
		{
			var deltaOmega = currentOmega - previousOmega;
			var avgOmega = (currentOmega + previousOmega) / 2;

			var alpha = deltaOmega / dt;
			var torque = inertia * alpha;
			return (torque * avgOmega).Cast<Watt>();
		}


		public static Watt InertiaPower(PerSecond omega, PerSquareSecond alpha, KilogramSquareMeter inertia, Second dt)
		{
			var torque = inertia * alpha;
			var power = torque * (omega + alpha / 2 * dt);
			return power;
		}
	}
}