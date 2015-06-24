using System.IO;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Segment
	{
		public VehicleCategory VehicleCategory { get; set; }


		public AxleConfiguration AxleConfiguration { get; set; }

		public Kilogram GrossVehicleWeightMin { get; set; }

		public Kilogram GrossVehicleWeightMax { get; set; }

		public string HDVClass { get; internal set; }

		public Stream AccelerationFile { get; internal set; }

		public Mission[] Missions { get; internal set; }
	}
}