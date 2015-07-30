using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class SteeringPump : LookupData<MissionType, VehicleClass, string, Watt>
	{
		private readonly SteeringPumpTechnologies _technologies = new SteeringPumpTechnologies();

		private readonly Dictionary<Tuple<MissionType, VehicleClass>, Watt[]> _data =
			new Dictionary<Tuple<MissionType, VehicleClass>, Watt[]>();

		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.SP-Table.csv";

		public SteeringPump()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		public override Watt Lookup(MissionType mission, VehicleClass hdvClass, string technology)
		{
			var shares = _data[Tuple.Create(mission, hdvClass)];
			var factors = _technologies.Lookup(technology);

			var sum = 0.SI<Watt>();
			for (var i = 0; i < factors.Length; i++) {
				sum += shares[i] * factors[i];
			}
			return sum;
		}

		protected override void ParseData(DataTable table)
		{
			_data.Clear();
			NormalizeTable(table);

			foreach (DataRow row in table.Rows) {
				var hdvClass = VehicleClassHelper.Parse(row.Field<string>("hdvclass/powerdemandpershare"));
				foreach (var mission in EnumHelper.GetValues<MissionType>()) {
					var values = row.Field<string>(mission.ToString().ToLower()).Split('/').ToDouble();
					values = values.Concat(Enumerable.Repeat(0.0, 3));

					_data[Tuple.Create(mission, hdvClass)] = values.Take(4).SI<Watt>().ToArray();
				}
			}
		}

		private class SteeringPumpTechnologies : LookupData<string, double[]>
		{
			private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VAUX.SP-Tech.csv";

			public SteeringPumpTechnologies()
			{
				ParseData(ReadCsvResource(ResourceId));
			}

			protected override void ParseData(DataTable table)
			{
				Data = table.Rows.Cast<DataRow>().ToDictionary(
					key => key.Field<string>("Scaling Factors"),
					value => new[] { value.ParseDouble("U"), value.ParseDouble("F"), value.ParseDouble("B"), value.ParseDouble("S") });
			}

			public override double[] Lookup(string tech)
			{
				return Data[tech];
			}
		}
	}
}