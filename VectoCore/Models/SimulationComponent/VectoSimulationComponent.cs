using System;
using System.Linq;
using System.Reflection;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
    public abstract class VectoSimulationComponent
    {
        [NonSerialized]
        protected ILog Log;

        protected VectoSimulationComponent()
        {
            Log = LogManager.GetLogger(GetType());
        }

        public abstract void CommitSimulationStep(IModalDataWriter writer);

        protected bool IsEqual(VectoSimulationComponent other)
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return fields.All(field => field.GetValue(this) != null
                ? field.GetValue(this).Equals(field.GetValue(other))
                : field.GetValue(other) == null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && IsEqual((VectoSimulationComponent)obj);
        }

        public override int GetHashCode()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return fields.Aggregate(0, (current, field) => (current * fields.Length) ^ (field != null ? field.GetHashCode() : 0));
        }
    }
}
