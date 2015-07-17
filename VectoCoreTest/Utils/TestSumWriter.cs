using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class TestSumWriter : SummaryFileWriter, ISummaryDataWriter
	{
		public void Write(IModalDataWriter data, Kilogram vehicleMass = null, Kilogram vehicleLoading = null) {}

		public override void Finish() {}
	}
}