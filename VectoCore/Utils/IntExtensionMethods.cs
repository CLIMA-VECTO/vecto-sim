namespace TUGraz.VectoCore.Utils
{
    public static class IntExtensionMethods
    {
        public static SI SI(this int i)
        {
            return new SI(i);
        }
    }
}