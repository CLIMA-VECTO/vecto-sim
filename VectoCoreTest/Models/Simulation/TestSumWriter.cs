using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
	public class TestSumWriter : ISummaryDataWriter
	{
		public void Write(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName, double vehicleMass,
			double vehicleLoading) {}

		public void Finish() {}
	}
}