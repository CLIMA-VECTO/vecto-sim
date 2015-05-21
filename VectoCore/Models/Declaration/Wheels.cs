using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Resources.Declaration;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Wheels : LookupData<Wheels.WheelsEntry>
	{
		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.Wheels.csv";

		internal Wheels()
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