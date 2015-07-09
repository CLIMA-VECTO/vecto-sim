using System;

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

	public static class AxleConfigurationExtensions
	{
		public static string GetName(this AxleConfiguration self)
		{
			return self.ToString().Substring(11);
		}
	}

	public static class EnumHelper
	{
		public static AxleConfiguration ParseAxleConfigurationType(string typeString)
		{
			return (AxleConfiguration) Enum.Parse(typeof (AxleConfiguration), "AxleConfig_" + typeString, true);
		}
	}
}