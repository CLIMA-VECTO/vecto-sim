using System.Security.Cryptography.X509Certificates;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	public interface IClutchInfo
	{
		bool ClutchClosed(Second absTime);
	}
}