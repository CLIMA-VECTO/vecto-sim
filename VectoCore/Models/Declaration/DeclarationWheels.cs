using System;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationWheels : LookupData<string, DeclarationWheels.WheelsEntry>
	{
		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.Wheels.csv";

		public DeclarationWheels()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		protected override sealed void ParseData(DataTable table)
		{
			Data = (from DataRow row in table.Rows
				select new WheelsEntry {
					WheelType = row[0].ToString(),
					Inertia = row.ParseDouble(1).SI<KilogramSquareMeter>(),
					TyreRadius = row.ParseDouble(2).SI().Milli.Meter.Cast<Meter>(),
					SizeClass = Int32.Parse(row[3].ToString())
				}).ToDictionary(e => e.WheelType);
		}

		public class WheelsEntry
		{
			public string WheelType;
			public KilogramSquareMeter Inertia;
			public Meter TyreRadius;
			public int SizeClass;
		}
	}
}