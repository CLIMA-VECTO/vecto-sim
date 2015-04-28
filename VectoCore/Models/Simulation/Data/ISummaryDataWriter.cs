namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public interface ISummaryDataWriter
	{
		void Write(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName);
		void Finish();
	}
}