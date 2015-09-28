using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	public interface IMileageCounter
	{
		Meter Distance { get; }
	}
}