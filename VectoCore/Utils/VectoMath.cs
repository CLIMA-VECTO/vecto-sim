using System;

namespace TUGraz.VectoCore.Utils
{
	public class VectoMath
	{
		public static double Interpolate(double x1, double x2, double y1, double y2, double xint)
		{
			return (xint - x1)*(y2 - y1)/(x2 - x1) + y1;
		}

		/// <summary>
		/// Convert revolutions from rounds per minute into angular velocity
		/// </summary>
		/// <param name="rpm">[1/min]</param>
		/// <returns>[rad/s]</returns>
		public static double RpmTpAngularVelocity(double rpm)
		{
			return (2.0 * Math.PI / 60.0) * rpm;
		}

		/// <summary>
		/// Convert revolutions and torque to power
		/// </summary>
		/// <param name="rpm">revolutions per minute [1/min]</param>
		/// <param name="torque">torque [Nm]</param>
		/// <returns>the power equivalent to the given rpm and torque [W]</returns>
		public static double ConvertRpmToPower(double rpm, double torque)
		{
			return (2.0 * Math.PI / 60.0) * torque * rpm
		}
	}
}