using System.Diagnostics.Contracts;

namespace TUGraz.VectoCore.Utils
{
	public static class Formulas
	{
		/// <summary>
		///     [Nm], [rad/s] => [W]. Calculates the power from torque and angular velocity.
		/// </summary>
		/// <param name="torque">[Nm]</param>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>power [W]</returns>
		[Pure]
		public static Watt TorqueToPower(NewtonMeter torque, RadianPerSecond angularFrequency)
		{
			return (torque * angularFrequency).To<Watt>();
		}

		/// <summary>
		///     [W], [rad/s] => [Nm]. Calculates the torque from power and angular velocity.
		/// </summary>
		/// <param name="power">[W]</param>
		/// <param name="angularFrequency">[rad/s]</param>
		/// <returns>torque [Nm]</returns>
		[Pure]
		public static NewtonMeter PowerToTorque(SI power, RadianPerSecond angularFrequency)
		{
			return (power / angularFrequency).To<NewtonMeter>();
		}
	}
}