using TUGraz.VectoCore.Models.Simulation.Data;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class TestSumWriter : SummaryFileWriter, ISummaryDataWriter
	{
		public void Write(IModalDataWriter data, double vehicleMass = 0, double vehicleLoading = 0) {}

		public override void Finish() {}
	}
}