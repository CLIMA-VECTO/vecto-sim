using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IBrakes
	{
		Watt BrakePower { get; set; }
	}
}