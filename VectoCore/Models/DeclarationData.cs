using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models
{
	public class DeclarationData
	{
		private static DeclarationData _instance;

		public Wheels Wheels { get; private set; }

		public Rims Rims { get; private set; }

		private DeclarationData()
		{
			Wheels = new Wheels();
			Rims = new Rims();
		}

		public static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}

		public static Segment GetSegment(VehicleCategory vehicleCategory, AxleConfiguration axleConfiguration,
			Kilogram grossVehicleMassRating)
		{
			throw new System.NotImplementedException();
		}
	}

	public class Segment
	{
		public string HDVClass { get; internal set; }
		public string VACC { get; internal set; }
		public Mission[] Missions { get; internal set; }
	}

	public class Mission
	{
		public string VCDV { get; set; }
		public string Name { get; set; }
		public double[] AxleWeightDistribution { get; set; }
		public double MassExtra { get; set; }
		public Kilogram RefLoad { get; set; }
		public string CycleFile { get; set; }
		public Kilogram[] Loadings { get; set; }
	}
}