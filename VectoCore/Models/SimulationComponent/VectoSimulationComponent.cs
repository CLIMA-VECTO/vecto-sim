namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public abstract class VectoSimulationComponent
	{
	    abstract public void CommitSimulationStep(IModalDataWriter writer);

	}
}
