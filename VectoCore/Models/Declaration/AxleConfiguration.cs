namespace TUGraz.VectoCore.Models.Declaration
{
	public enum AxleConfiguration
	{
		AxleConfig4x2,
		AxleConfig4x4,
		AxleConfig6x2,
		AxleConfig6x4,
		AxleConfig6x6,
		AxleConfig8x2,
		AxleConfig8x4,
		AxleConfig8x6,
		AxleConfig8x8,
	}

	public static class AxleConfigurationExtensions
	{
		public static string GetName(this AxleConfiguration self)
		{
			return self.ToString().Substring(10);
		}
	}
}