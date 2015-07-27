﻿using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Wheels : LookupData<string, Wheels.WheelsEntry>
	{
		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.Wheels.csv";

		public Wheels()
		{
			ParseData(ReadCsvResource(ResourceId));
		}

		protected override sealed void ParseData(DataTable table)
		{
			Data = (from DataRow row in table.Rows
				select new WheelsEntry {
					WheelType = row.Field<string>(0).Replace(" ", ""),
					Inertia = row.ParseDouble(1).SI<KilogramSquareMeter>(),
					DynamicTyreRadius = row.ParseDouble(2).SI().Milli.Meter.Cast<Meter>(),
					SizeClass = row.Field<string>(3)
				}).ToDictionary(e => e.WheelType);
		}

		public class WheelsEntry
		{
			public string WheelType;
			public KilogramSquareMeter Inertia;
			public Meter DynamicTyreRadius;
			public string SizeClass;
		}
	}
}