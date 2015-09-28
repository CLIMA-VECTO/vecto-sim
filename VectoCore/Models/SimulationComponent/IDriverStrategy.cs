using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IDriverStrategy
	{
		IDriverActions Driver { get; set; }

		IResponse Request(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient);

		IResponse Request(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient);
	}
}