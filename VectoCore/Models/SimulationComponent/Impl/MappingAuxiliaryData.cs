using System.Data;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class MappingAuxiliaryData
	{
		public double EfficiencyToSupply { get; set; }
		public double TransitionRatio { get; set; }
		public double EfficiencyToEngine { get; set; }

		private readonly DelauneyMap _map = new DelauneyMap();

		public Watt GetPowerDemand(PerSecond nAuxiliary, Watt powerAuxOut)
		{
			return _map.Interpolate(nAuxiliary.Value(), powerAuxOut.Value()).SI<Watt>();
		}

		public static MappingAuxiliaryData ReadFromFile(string fileName)
		{
			var auxData = new MappingAuxiliaryData();

			var stream = new StreamReader(fileName);
			stream.ReadLine(); // skip header "Transmission ration to engine rpm [-]"
			auxData.TransitionRatio = stream.ReadLine().ToDouble();
			stream.ReadLine(); // skip header "Efficiency to engine [-]"
			auxData.EfficiencyToEngine = stream.ReadLine().ToDouble();
			stream.ReadLine(); // skip header "Efficiency auxiliary to supply [-]"
			auxData.EfficiencyToSupply = stream.ReadLine().ToDouble();

			var table = VectoCSVFile.ReadStream(stream.BaseStream);

			var data = table.Rows.Cast<DataRow>().Select(row => new {
				AuxiliarySpeed = row.ParseDouble("Auxiliary speed").RPMtoRad(),
				MechanicalPower = row.ParseDouble("Mechanical power").SI().Kilo.Watt.Cast<Watt>(),
				SupplyPower = row.ParseDouble("Supply power").SI().Kilo.Watt.Cast<Watt>()
			});

			foreach (var d in data) {
				auxData._map.AddPoint(d.AuxiliarySpeed.Value(), d.SupplyPower.Value(), d.MechanicalPower.Value());
			}
			auxData._map.Triangulate();

			return auxData;
		}
	}
}