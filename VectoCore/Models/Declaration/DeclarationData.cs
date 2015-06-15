namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationData
	{
		private static DeclarationData _instance;
		private readonly DeclarationSegments _segments;
		private readonly DeclarationRims _rims;
		private readonly DeclarationWheels _wheels;

		public static DeclarationWheels Wheels
		{
			get { return Instance()._wheels; }
		}

		public static DeclarationRims Rims
		{
			get { return Instance()._rims; }
		}

		public static DeclarationSegments Segments
		{
			get { return Instance()._segments; }
		}

		private DeclarationData()
		{
			_wheels = new DeclarationWheels();
			_rims = new DeclarationRims();
			_segments = new DeclarationSegments();
		}

		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}
}