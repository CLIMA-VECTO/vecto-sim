using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public enum ClutchState
	{
		ClutchClosed,
		ClutchOpened,
		ClutchSlipping
	}

	public interface IClutch : IPowerTrainComponent
	{
		ITnOutPort IdleControlPort { get; }
	}
}