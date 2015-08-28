namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public enum ClutchState
	{
		ClutchClosed,
		ClutchOpened,
		ClutchSlipping
	}

	public interface IClutch : IPowerTrainComponent {}
}