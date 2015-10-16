using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public enum VehicleClass
	{
		Class0,
		Class1,
		Class2,
		Class3,
		Class4,
		Class5,
		Class6,
		Class7,
		Class8,
		Class9,
		Class10,
		Class11,
		Class12,
		Class13,
		Class14,
		Class15,
		Class16,
		Class17,
		ClassB1,
		ClassB2,
		ClassB3,
		ClassB4,
		ClassB5,
		ClassB6
	}

	public static class VehicleClassHelper
	{
		private const string Prefix = "Class";

		public static VehicleClass Parse(string text)
		{
			return text.Replace(Prefix, "").Parse<VehicleClass>();
		}

		public static string GetClassNumber(this VehicleClass hdvClass)
		{
			return hdvClass.ToString().Substring(Prefix.Length);
		}
	}
}