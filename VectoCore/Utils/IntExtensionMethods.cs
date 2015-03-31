using System;
using System.Diagnostics.Contracts;

namespace TUGraz.VectoCore.Utils
{
    public static class IntExtensionMethods
    {
        public static SI SI(this int i)
        {
            return new SI(i);
        }

        [Pure]
        public static T SI<T>(this int d) where T : SI
        {
            return (T)Activator.CreateInstance(typeof(T), d);
        }
    }
}