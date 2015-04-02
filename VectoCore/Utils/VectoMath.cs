namespace TUGraz.VectoCore.Utils
{
	public class VectoMath
	{
		public static double Interpolate(double x1, double x2, double y1, double y2, double xint)
		{
			return (xint - x1) * (y2 - y1) / (x2 - x1) + y1;
		}
	}
}