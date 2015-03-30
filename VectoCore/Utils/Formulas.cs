using System.Diagnostics.Contracts;

namespace TUGraz.VectoCore.Utils
{
    public static class Formulas
    {
        /// <summary>
        /// [Nm], [rad/s] => [W]. Calculates the power from torque and angular velocity.
        /// </summary>
        /// <param name="torque">[Nm]</param>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>power [W]</returns>
        [Pure]
        public static SI TorqueToPower(SI torque, SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Requires(torque.HasEqualUnit(new SI().Newton.Meter));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

            return torque * angularFrequency;
        }

        /// <summary>
        /// [W], [rad/s] => [Nm]. Calculates the torque from power and angular velocity.
        /// </summary>
        /// <param name="power">[W]</param>
        /// <param name="angularFrequency">[rad/s]</param>
        /// <returns>torque [Nm]</returns>
        [Pure]
        public static SI PowerToTorque(SI power, SI angularFrequency)
        {
            Contract.Requires(angularFrequency.HasEqualUnit(new SI().Radian.Per.Second));
            Contract.Requires(power.HasEqualUnit(new SI().Watt));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Newton.Meter));

            return power / angularFrequency;
        }
    }
}