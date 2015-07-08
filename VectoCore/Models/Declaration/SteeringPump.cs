using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class SteeringPump : LookupData<MissionType, string, string, Watt>
	{
		private readonly SteeringPumpTechnologies _technologies = new SteeringPumpTechnologies();

		private readonly Dictionary<Tuple<MissionType, string>, Watt[]> _data =
			new Dictionary<Tuple<MissionType, string>, Watt[]>();

		public override Watt Lookup(MissionType mission, string hdvClass, string technology)
		{
			var shares = _data[Tuple.Create(mission, hdvClass)];
			var sum = 0.SI<Watt>();
			var factors = _technologies.Lookup(technology);
			for (var i = 0; i < 4; i++) {
				sum += shares[i] * factors[i];
			}
			return sum;
		}


		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.VAUX.SP-Table.csv"; }
		}

		protected override void ParseData(DataTable table)
		{
			_data.Clear();
			NormalizeTable(table);

			foreach (DataRow row in table.Rows) {
				var hdvClass = row.Field<string>("hdvclass/powerdemandpershare");
				foreach (MissionType mission in Enum.GetValues(typeof(MissionType))) {
					var values = row.Field<string>(mission.ToString().ToLower()).Split('/').ToDouble();
					values = values.Concat(Enumerable.Repeat(0.0, 3));

					_data[Tuple.Create(mission, hdvClass)] = values.Take(4).SI<Watt>().ToArray();
				}
			}
		}

		public class SteeringPumpTechnologies : LookupData<string, double[]>
		{
			protected override string ResourceId
			{
				get { return "TUGraz.VectoCore.Resources.Declaration.VAUX.SP-Tech.csv"; }
			}

			protected override void ParseData(DataTable table)
			{
				Data.Clear();
				foreach (DataRow row in table.Rows) {
					var tech = row.Field<string>("Scaling Factors");
					var factors = new[] { row.ParseDouble("U"), row.ParseDouble("F"), row.ParseDouble("B"), row.ParseDouble("S") };
					Data[tech] = factors;
				}
			}

			public override double[] Lookup(string tech)
			{
				return Data[tech];
			}
		}
	}
}