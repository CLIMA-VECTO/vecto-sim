using TUGraz.VectoCore.Models.Declaration;

namespace TUGraz.VectoCore.Models
{
	public class DeclarationData
	{
		private static DeclarationData _instance;

		public readonly Wheels Wheels;

		public readonly Rims Rims;

		private DeclarationData()
		{
			Wheels = new Wheels();
			Rims = new Rims();
		}

		public static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}
}