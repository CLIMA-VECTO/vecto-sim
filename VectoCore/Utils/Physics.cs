namespace TUGraz.VectoCore.Utils
{
	public class Physics
	{
		public static readonly MeterPerSquareSecond GravityAccelleration = 9.81.SI<MeterPerSquareSecond>();

		/// <summary>
		/// Density of air.
		/// </summary>
		public static readonly SI AirDensity = 1.188.SI().Kilo.Gramm.Per.Cubic.Meter;

		/// <summary>
		/// Density of fuel.
		/// </summary>
		public static readonly SI FuelDensity = 0.832.SI().Kilo.Gramm.Per.Cubic.Dezi.Meter;

		public static readonly double RollResistanceExponent = 0.9;

		public static readonly MeterPerSecond BaseWindSpeed = 3.SI<MeterPerSecond>();


		/// <summary>
		/// fuel[kg] => co2[kg]. Factor to convert from fuel weight to co2 weight.
		/// </summary>
		public static readonly double CO2PerFuelWeight = 3.16;
	}
}