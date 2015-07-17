using TUGraz.VectoCore.Utils;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.Declaration
{
	public enum AxleConfiguration
	{
		AxleConfig_4x2,
		AxleConfig_4x4,
		AxleConfig_6x2,
		AxleConfig_6x4,
		AxleConfig_6x6,
		AxleConfig_8x2,
		AxleConfig_8x4,
		AxleConfig_8x6,
		AxleConfig_8x8,
	}

	public static class AxleConfigurationHelper
	{
		private const string Prefix = "AxleConfig_";

		public static string GetName(this AxleConfiguration self)
		{
			return self.ToString().Replace(Prefix, "");
		}

		public static AxleConfiguration Parse(string typeString)
		{
			return EnumHelper.Parse<AxleConfiguration>(Prefix + typeString);
		}

		public static DriverData.DriverMode ParseDriverMode(string driverString)
		{
			return (DriverData.DriverMode)Enum.Parse(typeof(DriverData.DriverMode), driverString.Replace("-", ""), true);
		}
	}
}