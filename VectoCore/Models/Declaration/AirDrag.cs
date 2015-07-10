using System;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class AirDrag : LookupData<string, AirDrag.AirDragEntry>
	{
		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VCDV.parameters.csv";

		public AirDrag()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		protected override void ParseData(DataTable table)
		{
			Data = table.Rows.Cast<DataRow>().ToDictionary(row => row.Field<string>("Parameters"), row => new AirDragEntry {
				A1 = row.ParseDouble("a1"),
				A2 = row.ParseDouble("a2"),
				A3 = row.ParseDouble("a3")
			});
		}

		public AirDragEntry Lookup(VehicleCategory category)
		{
			switch (category) {
				case VehicleCategory.CityBus:
				case VehicleCategory.InterurbanBus:
				case VehicleCategory.Coach:
					return Lookup("CoachBus");
				case VehicleCategory.Tractor:
					return Lookup("TractorSemitrailer");
				case VehicleCategory.RigidTruck:
					return Lookup("RigidSolo");
				default:
					throw new ArgumentOutOfRangeException("category", category, null);
			}
		}

		public class AirDragEntry
		{
			public double A1 { get; set; }
			public double A2 { get; set; }
			public double A3 { get; set; }
		}
	}
}