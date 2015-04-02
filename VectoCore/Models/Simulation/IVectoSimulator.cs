namespace TUGraz.VectoCore.Models.Simulation
{
	public interface IVectoSimulator
	{
		void Run();
		IVehicleContainer GetContainer();
	}
}