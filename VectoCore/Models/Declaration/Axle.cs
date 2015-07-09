using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Axle
	{
		public KilogramSquareMeter Inertia { get; internal set; }

		public double RollResistanceCoefficient { get; internal set; }

		public Newton TyreTestLoad { get; internal set; }

		public double AxleWeightShare { get; internal set; }

		public bool TwinTyres { get; internal set; }
	}
}