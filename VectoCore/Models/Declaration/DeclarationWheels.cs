using System;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationWheels : LookupData<DeclarationWheels.WheelsEntry>
	{
		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.Wheels.csv";

		internal DeclarationWheels()
		{
			var csvFile = ReadCsvFile(ResourceId);
			ParseData(csvFile);
		}


		protected override sealed void ParseData(DataTable table)
		{
			_data = (from DataRow row in table.Rows
				select new WheelsEntry {
					WheelType = row[0].ToString(),
					Inertia = row.ParseDouble(1).SI<KilogramSquareMeter>(),
					DynamicTyreRadius = row.ParseDouble(2).SI().Milli.Meter.Cast<Meter>(),
					SizeClass = Int32.Parse(row[3].ToString())
				}).ToDictionary(e => e.WheelType);
		}

		public class WheelsEntry
		{
			public string WheelType;
			public KilogramSquareMeter Inertia;
			public Meter DynamicTyreRadius;
			public int SizeClass;
		}
	}
}