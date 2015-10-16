namespace TUGraz.VectoCore.Utils
{
	public class Physics
	{
		public static readonly MeterPerSquareSecond GravityAccelleration = 9.81.SI<MeterPerSquareSecond>();

		public static readonly SI AirDensity = 1.188.SI().Kilo.Gramm.Per.Cubic.Meter;

		public static readonly double RollResistanceExponent = 0.9;

		public static readonly MeterPerSecond BaseWindSpeed = 3.SI<MeterPerSecond>();
		public static readonly double CO2PerFuelGram = 3.16;
	}
}