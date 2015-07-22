using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class DistanceCycleDataTest
	{
//		public readonly string CycleFile = @"TestData\Cycles\";
		private const string ResourceNamespace = "TUGraz.VectoCore.Resources.Declaration.";


		[TestMethod]
		public void CycleAltitudeTest()
		{
			var missionType = "LongHaul";
			var stream = RessourceHelper.ReadStream(ResourceNamespace + "MissionCycles." + missionType + ".vdri");

			var cycleData = DrivingCycleDataReader.ReadFromStream(stream, DrivingCycleData.CycleType.DistanceBased);
			foreach (var entry in cycleData.Entries) {
				var tmp = entry.Altitude;
			}
		}
	}
}