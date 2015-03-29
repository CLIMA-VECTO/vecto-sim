using System;
using System.Diagnostics.Contracts;

namespace TUGraz.VectoCore.Utils
{
    public static class Formulas
    {
        /// <summary>
        /// [Nm], [rad/s] => [W].
        /// </summary>
        /// <param name="torque">[Nm]</param>
        /// <param name="angularVelocity">[rad/s]</param>
        /// <returns>power [W]</returns>
        [Pure]
        public static SI TorqueToPower(SI torque, SI angularVelocity)
        {
            Contract.Requires<ArgumentException>(angularVelocity.HasEqualUnit(new SI().Radiant.Per.Second));
            Contract.Requires<ArgumentException>(torque.HasEqualUnit(new SI().Newton.Meter));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Watt));

            return (torque * angularVelocity).SI().Watt;
        }

        /// <summary>
        /// [W], [rad/s] => [Nm].
        /// </summary>
        /// <param name="power">[W]</param>
        /// <param name="angularVelocity">[rad/s]</param>
        /// <returns>torque [Nm]</returns>
        [Pure]
        public static SI PowerToTorque(SI power, SI angularVelocity)
        {
            Contract.Requires<ArgumentException>(angularVelocity.HasEqualUnit(new SI().Radiant.Per.Second));
            Contract.Requires<ArgumentException>(power.HasEqualUnit(new SI().Watt));
            Contract.Ensures(Contract.Result<SI>().HasEqualUnit(new SI().Newton.Meter));

            return (power / angularVelocity).SI().Newton.Meter;
        }
    }
}